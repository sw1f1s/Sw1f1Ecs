using System;
using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal struct SparseSet<T> : IDisposable where T : ISparseItem {
        private T[] _denseItems;
        private uint[] _sparseItems;
        private uint _denseItemsCount;

        public T[] DenseItems => _denseItems;
        public int Count => (int)_denseItemsCount;
        
        public SparseSet(int capacity) {
            _denseItems = new T[capacity];
            _sparseItems = new uint[capacity];
            _denseItemsCount = 0;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Add(T item) {
            TryResize(item.Id);
            _denseItems[_denseItemsCount] = item;
            _sparseItems[item.Id] = ++_denseItemsCount;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Has(int id) {
            return id >= 0 && id < _sparseItems.Length && _sparseItems[id] != 0;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T Get(int id) {
            uint denseIndex = _sparseItems[id] - 1;
            return ref _denseItems[denseIndex];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetSparseIndex(int id) {
            return (int)_sparseItems[id] - 1;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Remove(int id) {
            uint denseIndex = _sparseItems[id] - 1;
            _sparseItems[id] = 0;

            _denseItemsCount--;
            uint lastIndex = _denseItemsCount;
            if (lastIndex > denseIndex) {
                _denseItems[denseIndex] = _denseItems[lastIndex];
                _sparseItems[_denseItems[denseIndex].Id] = denseIndex + 1;
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            for (int i = 0; i < _denseItemsCount; i++) {
                int sparseIndex = _denseItems[i].Id;
                _sparseItems[sparseIndex] = 0;
            }
            _denseItemsCount = 0;
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
        
        public void Dispose() {
            for (int i = 0; i < _denseItems.Length; i++) {
                _denseItems[i] = default(T);
                _sparseItems[i] = 0;
            }
            
            _denseItemsCount = 0;
        }
        
        public struct Enumerator<T> where T : ISparseItem {
            private readonly SparseSet<T> _data;
            private readonly int _count;
            private int _idx;

            internal Enumerator (SparseSet<T> data) {
                _data = data;
                _count = data.Count;
                _idx = -1;
            }

            public T Current {
                [MethodImpl (MethodImplOptions.AggressiveInlining)]
                get => _data._denseItems[_idx];
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public bool MoveNext () {
                return ++_idx < _count;
            }
        }
    }   
}