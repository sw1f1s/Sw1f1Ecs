using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class SparseArray<T> : IDisposable where T : ISparseItem {
        private volatile T[] _denseItems;
        private volatile int[] _sparseItems;
        private volatile int _denseItemsCount;
        
        private int _lock;
        private DelayedOperation<T>[] _delayedOps;
        private int _delayedOpsCount;
        
        private readonly object _resizeLock = new object();
        
        public T[] DenseItems => _denseItems;
        public int Count => _denseItemsCount;
        
        public SparseArray(int capacity) {
            _denseItems = new T[capacity];
            _sparseItems = new int[capacity];
            _delayedOps = new DelayedOperation<T>[capacity];
            _denseItemsCount = 0;
            _lock = 0;
            for (int i = 0; i < _sparseItems.Length; i++) {
                _sparseItems[i] = -1;
            }
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Add(T item) {
            if (Has(item.Id)) {
                return false;
            }
            
            if (_lock > 0) {
                return AddDelayedOp(new DelayedOperation<T>(item));
            }

            TryResize(item.Id);
            
            int value = Interlocked.Increment(ref _denseItemsCount) - 1;
            _denseItems[value] = item;
            Interlocked.Exchange(ref _sparseItems[item.Id], value);
            
            return true;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Has(int id) {
            return id >= 0 && id < _sparseItems.Length && _sparseItems[id] != -1;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T Get(int id) {
            int denseIndex = _sparseItems[id];
            return ref _denseItems[denseIndex];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetSparseIndex(int id) {
            return _sparseItems[id];
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Remove(int id) {
            if (!Has(id)) {
                return false;
            }
            
            if (_lock > 0) {
                return AddDelayedOp(new DelayedOperation<T>(id));
            }
            
            int denseIndex = _sparseItems[id];
            _sparseItems[id] = -1;

            int lastIndex = Interlocked.Decrement(ref _denseItemsCount);
            if (lastIndex > denseIndex) {
                _denseItems[denseIndex] = _denseItems[lastIndex];
                Interlocked.Exchange(ref _sparseItems[_denseItems[denseIndex].Id], denseIndex);
            }

            return true;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            for (int i = 0; i < _denseItemsCount; i++) {
                int sparseIndex = _denseItems[i].Id;
                _sparseItems[sparseIndex] = -1;
            }
            _denseItemsCount = 0;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void DeepClear() {
            for (int i = 0; i < _denseItems.Length; i++) {
                _denseItems[i] = default(T);
                _sparseItems[i] = -1;
            }
            
            _denseItemsCount = 0;
            _lock = 0;
        }

        public Enumerator<T> GetEnumerator() {
            return new Enumerator<T>(this);
        }

        public override string ToString() {
            string s = string.Empty;
            foreach (var value in this) {
                s += value + ", ";
            }
            return s;
        }
        
        private void TryResize(int id) {
            while (_denseItemsCount >= _denseItems.Length || id >= _sparseItems.Length) {
                if (!Monitor.IsEntered(_resizeLock)) {
                    lock (_resizeLock) {
                        if (_denseItemsCount >= _denseItems.Length || id >= _sparseItems.Length) {
                            Resize();
                        }
                    }   
                } else {
                    Resize();
                }
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void Resize() {
            Array.Resize(ref _denseItems, _denseItems.Length * 2);
            Array.Resize(ref _sparseItems, _sparseItems.Length * 2);
            for (int i = _denseItemsCount; i < _sparseItems.Length; i++) {
                _sparseItems[i] = -1;
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private bool AddDelayedOp(DelayedOperation<T> op) {
            if (_delayedOpsCount >= _delayedOps.Length) {
                Array.Resize(ref _delayedOps, _delayedOps.Length * 2);
            }

            var value = Interlocked.Increment(ref _delayedOpsCount) - 1;
            _delayedOps[value] = op;
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
            DeepClear();
        }

        public struct Enumerator<T> : IDisposable where T : ISparseItem {
            private readonly SparseArray<T> _data;
            private readonly int _count;
            private int _idx;

            internal Enumerator (SparseArray<T> data) {
                _data = data;
                _count = data._denseItemsCount;
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
                _data.Unlock();
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