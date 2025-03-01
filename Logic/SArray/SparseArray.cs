using System;
using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class SparseArray<T> : IDisposable where T : ISparseItem {
        private SparseSet<T> _data;
        private uint _lock;
        private SparseSet<DelayedOperation<T>> _delayedOps;
        private bool _isDisposed;
        
        public T[] DenseItems => _data.DenseItems;
        public int Count => _data.Count;
        
        public SparseArray(int capacity) {
            _data = new SparseSet<T>(capacity);
            _delayedOps = new SparseSet<DelayedOperation<T>>(4);
            _lock = 0;
        }
        
        public SparseArray(SparseArray<T> copy) {
            _data = copy._data;
            _delayedOps = copy._delayedOps;
            _lock = copy._lock;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Add(T item) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }
            
            if (Has(item.Id)) {
                throw new Exception($"{nameof(SparseArray<T>)} already contains an item with id {item.Id}");
            }
            
            if (_lock > 0) {
                AddDelayedOp(new DelayedOperation<T>(item));
                return;
            }

            _data.Add(item);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Has(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }
            
            if (_lock > 0 && _delayedOps.Has(id)) {
                return _delayedOps.Get(id).IsAdd;
            }

            return _data.Has(id);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T Get(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }
            
            if (_lock > 0 && _delayedOps.Has(id) && _delayedOps.Get(id).IsAdd) {
                ref var ops = ref _delayedOps.Get(id);
                return ref ops.GetRefValue(ref ops);
            }
            
            return ref _data.Get(id);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetSparseIndex(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }

            return _data.GetSparseIndex(id);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Remove(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }
            
            if (!Has(id)) {
                return false;
            }
            
            if (_lock > 0) {
                AddDelayedOp(new DelayedOperation<T>(id));
                return true;
            }
            _data.Remove(id);
            return true;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }
            
            _data.Clear();
            _delayedOps.Clear();
        }

        public Enumerator<T> GetEnumerator() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }
            
            return new Enumerator<T>(this);
        }

        public override string ToString() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }
            return _data.ToString();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void AddDelayedOp(DelayedOperation<T> op) {
            if (_delayedOps.Has(op.Id)) {
                _delayedOps.Remove(op.Id);
            }
            _delayedOps.Add(op);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void Lock() {
            _lock++;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void Unlock() {
            _lock--;
            if (_lock > 0) {
                return;
            }

            foreach (var delayedOp in _delayedOps) {
                if (delayedOp.IsAdd) {
                    Add(delayedOp.Value);
                }else {
                    Remove(delayedOp.Id);
                }
            }
            _delayedOps.Clear();
            _lock = 0;
        }
        
        public void Dispose() {
            _isDisposed = true;
            _data.Dispose();
            _delayedOps.Dispose();
        }

        public struct Enumerator<T> : IDisposable where T : ISparseItem {
            private readonly SparseArray<T> _data;
            private readonly int _count;
            private int _idx;

            internal Enumerator (SparseArray<T> data) {
                _data = data;
                _count = data.Count;
                _idx = -1;
                _data.Lock();
            }

            public T Current {
                [MethodImpl (MethodImplOptions.AggressiveInlining)]
                get => _data.DenseItems[_idx];
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public bool MoveNext () {
                return ++_idx < _count;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() {
                _data?.Unlock();
            }
        }
    } 
}