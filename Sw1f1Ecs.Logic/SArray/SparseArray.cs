using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class SparseArray<T> : IDisposable where T : ISparseItem {
        private T[] _denseItems;
        private uint[] _sparseItems;
        private uint _denseItemsCount;
        
        private uint _lock;
        private DelayedOperation<T>[] _delayedOps;
        private uint _delayedOpsCount;
        
        private bool _isDisposed;
        
        public T[] DenseItems => _denseItems;
        public int Count => (int)_denseItemsCount;
        
        public SparseArray(int capacity) {
            _denseItems = new T[capacity];
            _sparseItems = new uint[capacity];
            _delayedOps = new DelayedOperation<T>[capacity];
            _denseItemsCount = 0;
            _lock = 0;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Add(T item) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }
            
            if (Has(item.Id)) {
                return false;
            }
            
            if (_lock > 0) {
                return AddDelayedOp(new DelayedOperation<T>(item));
            }

            TryResize(item.Id);
            
            _denseItems[_denseItemsCount] = item;
            _sparseItems[item.Id] = ++_denseItemsCount;
            
            return true;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Has(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }
            
            return id >= 0 && id < _sparseItems.Length && _sparseItems[id] != 0;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T Get(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }
            
            uint denseIndex = _sparseItems[id] - 1;
            return ref _denseItems[denseIndex];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetSparseIndex(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }
            
            return (int)_sparseItems[id] - 1;
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
                return AddDelayedOp(new DelayedOperation<T>(id));
            }
            
            uint denseIndex = _sparseItems[id] - 1;
            _sparseItems[id] = 0;

            _denseItemsCount--;
            uint lastIndex = _denseItemsCount;
            if (lastIndex > denseIndex) {
                _denseItems[denseIndex] = _denseItems[lastIndex];
                _sparseItems[_denseItems[denseIndex].Id] = denseIndex + 1;
            }

            return true;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SparseArray<T>));
            }
            
            for (int i = 0; i < _denseItemsCount; i++) {
                int sparseIndex = _denseItems[i].Id;
                _sparseItems[sparseIndex] = 0;
            }
            _denseItemsCount = 0;
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
            
            string s = string.Empty;
            foreach (var value in this) {
                s += value + ", ";
            }
            return s;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void TryResize(int id) {
            while (_denseItemsCount >= _denseItems.Length || id >= _sparseItems.Length) {
                Resize();
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void Resize() {
            Array.Resize(ref _denseItems, _denseItems.Length * 2);
            Array.Resize(ref _sparseItems, _sparseItems.Length * 2);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private bool AddDelayedOp(DelayedOperation<T> op) {
            if (_delayedOpsCount >= _delayedOps.Length) {
                Array.Resize(ref _delayedOps, _delayedOps.Length * 2);
            }

            _delayedOps[_delayedOpsCount++] = op;
            return true;
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
            
            for (int i = 0; i < _delayedOpsCount; i++) {
                var delayedOp = _delayedOps[i];
                if (delayedOp.IsAdd) {
                    Add(delayedOp.Value);
                }else {
                    Remove(delayedOp.Index);
                }
            }

            _delayedOpsCount = 0;
            _lock = 0;
        }
        
        public void Dispose() {
            _isDisposed = true;
            for (int i = 0; i < _denseItems.Length; i++) {
                _denseItems[i] = default(T);
                _sparseItems[i] = 0;
            }
            
            _denseItemsCount = 0;
            _lock = 0;
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
                get => _data._denseItems[_idx];
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
        
        private struct DelayedOperation<T> where T : ISparseItem {
            public readonly bool IsAdd;
            public readonly int Index;
            public readonly T Value;

            public DelayedOperation(T value) {
                IsAdd = true;
                Value = value;
                Index = 0;
            }
            
            public DelayedOperation(int index) {
                IsAdd = false;
                Index = index;
                Value = default(T);
            }
        }
    } 
}