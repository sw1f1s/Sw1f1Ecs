using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_5_3_OR_NEWER
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace Sw1f1.Ecs {
    public unsafe struct UnsafeSparseArray<T> : IDisposable where T : unmanaged {
        private Entry* _denseItems;
        private uint* _sparseItems;
        private uint _length;
        private uint _denseItemsCount;
        private bool _isDisposed;
        
        internal Entry* DenseItems => _denseItems;
        public uint Length => _length;
        public uint Count => _denseItemsCount;
        
        public UnsafeSparseArray(uint capacity) {
            _length = capacity;
            _denseItemsCount = 0;
            _isDisposed = false;
#if UNITY_5_3_OR_NEWER
            _denseItems = (Entry*)UnsafeUtility.Malloc(sizeof(Entry) * _length, UnsafeUtility.AlignOf<Entry>(), Allocator.Persistent);
            _sparseItems = (uint*)UnsafeUtility.Malloc(sizeof(uint) * _length, 4, Allocator.Persistent);
            UnsafeUtility.MemClear(_denseItems, sizeof(Entry) * _length);
            UnsafeUtility.MemClear(_sparseItems, sizeof(uint) * _length);
#else            
            _denseItems = (Entry*)NativeMemory.AllocZeroed((nuint)(sizeof(Entry) * _length));
            _sparseItems = (uint*)NativeMemory.AllocZeroed(sizeof(uint) * _length);
#endif
        }
        
        public UnsafeSparseArray(in UnsafeSparseArray<T> copy) {
            _length = copy._length;
            _denseItemsCount = 0;
            _isDisposed = false;
#if UNITY_5_3_OR_NEWER
            _denseItems = (Entry*)UnsafeUtility.Malloc(sizeof(Entry) * _length, UnsafeUtility.AlignOf<Entry>(), Allocator.Persistent);
            _sparseItems = (uint*)UnsafeUtility.Malloc(sizeof(uint) * _length, 4, Allocator.Persistent);
            UnsafeUtility.MemClear(_denseItems, sizeof(Entry) * _length);
            UnsafeUtility.MemClear(_sparseItems, sizeof(uint) * _length);
            UnsafeUtility.MemCpy(_denseItems, copy._denseItems, (uint)(sizeof(Entry) * _length));
            UnsafeUtility.MemCpy(_sparseItems, copy._sparseItems, sizeof(uint) * _length);
#else            
            _denseItems = (Entry*)NativeMemory.AllocZeroed((nuint)(sizeof(Entry) * _length));
            _sparseItems = (uint*)NativeMemory.AllocZeroed(sizeof(uint) * _length);
            Unsafe.CopyBlock(_denseItems, copy._denseItems, (uint)(sizeof(Entry) * _length));
            Unsafe.CopyBlock(_sparseItems, copy._sparseItems, sizeof(uint) * _length);
#endif
        }

        public SparseArray<T> AsSafeArray() {
            var array = new SparseArray<T>(_length);
            for (int i = 0; i < _denseItemsCount; i++) {
                array.Add((int)_denseItems[i].Index, _denseItems[i].Value);
            }

            return array;
        } 
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Add(int id, T item) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeSparseArray<T>));
            }
            
            TryResize(id);
            _denseItems[_denseItemsCount] = new Entry((uint)id, item);
            _sparseItems[id] = ++_denseItemsCount;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Has(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeSparseArray<T>));
            }

            return id >= 0 && id < _length && _sparseItems[id] != 0;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T Get(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeSparseArray<T>));
            }
            
            uint denseIndex = _sparseItems[id] - 1;
            return ref _denseItems[denseIndex].Value;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T GetFirst() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeSparseArray<T>));
            }

            if (_denseItemsCount == 0) {
                throw new IndexOutOfRangeException();
            }
            
            return ref _denseItems[0].Value;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T GetLast() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeSparseArray<T>));
            }

            if (_denseItemsCount == 0) {
                throw new IndexOutOfRangeException();
            }
            
            return ref _denseItems[_denseItemsCount - 1].Value;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Remove(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeSparseArray<T>));
            }
            
            uint denseIndex = _sparseItems[id] - 1;
            _sparseItems[id] = 0;

            _denseItemsCount--;
            uint lastIndex = _denseItemsCount;
            if (lastIndex > denseIndex) {
                _denseItems[denseIndex] = _denseItems[lastIndex];
                _sparseItems[_denseItems[denseIndex].Index] = denseIndex + 1;
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeSparseArray<T>));
            }
            
            for (int i = 0; i < _denseItemsCount; i++) {
                uint sparseIndex = _denseItems[i].Index;
                _sparseItems[sparseIndex] = 0;
            }
            _denseItemsCount = 0;
        }

        public Enumerator<T> GetEnumerator() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeSparseArray<T>));
            }
            
            return new Enumerator<T>(this);
        }

        public override string ToString() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeSparseArray<T>));
            }
            
            string s = string.Empty;
            foreach (var value in this) {
                s += value + ", ";
            }
            return s;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void TryResize(int id) {
            while (_denseItemsCount >= _length || id >= _length) {
                Resize();
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void Resize() {
            uint newLength = _length * 2;
#if UNITY_5_3_OR_NEWER
            Entry* newDenseItems = (Entry*)UnsafeUtility.Malloc(sizeof(Entry) * newLength, UnsafeUtility.AlignOf<Entry>(), Allocator.Persistent);
            uint* newSparseItems = (uint*)UnsafeUtility.Malloc(sizeof(uint) * newLength, 4, Allocator.Persistent);
            UnsafeUtility.MemClear(newDenseItems, sizeof(Entry) * newLength);
            UnsafeUtility.MemClear(newSparseItems, sizeof(uint) * newLength);
            UnsafeUtility.MemCpy(newDenseItems, _denseItems, (uint)(sizeof(Entry) * _length));
            UnsafeUtility.MemCpy(newSparseItems, _sparseItems, sizeof(uint) * _length);
            UnsafeUtility.Free(_denseItems, Allocator.Persistent);
            UnsafeUtility.Free(_sparseItems, Allocator.Persistent);
#else            
            Entry* newDenseItems = (Entry*)NativeMemory.AllocZeroed((nuint)(sizeof(Entry) * newLength));
            uint* newSparseItems = (uint*)NativeMemory.AllocZeroed(sizeof(uint) * newLength);
            Unsafe.CopyBlock(newDenseItems, _denseItems, (uint)(sizeof(Entry) * _length));
            Unsafe.CopyBlock(newSparseItems, _sparseItems, sizeof(uint) * _length);
            NativeMemory.Free(_denseItems);
            NativeMemory.Free(_sparseItems);
#endif

            _denseItems = newDenseItems;
            _sparseItems = newSparseItems;
            _length = newLength;
        }
        
        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            
            _isDisposed = true;
#if UNITY_5_3_OR_NEWER
            UnsafeUtility.Free(_denseItems, Allocator.Persistent);
            UnsafeUtility.Free(_sparseItems, Allocator.Persistent);
#else            
            NativeMemory.Free(_denseItems);
            NativeMemory.Free(_sparseItems);
#endif
            
            _denseItemsCount = 0;
            _length = 0;
        }
        
        internal struct Entry {
            public uint Index;
            public T Value;

            public Entry(uint index, T value) {
                Index = index;
                Value = value;
            }
        }

        public struct Enumerator<T> : IDisposable where T : unmanaged{
            private UnsafeSparseArray<T> _data;
            private uint _count;
            private int _idx;

            internal Enumerator (in UnsafeSparseArray<T> data) {
                _data = data;
                _count = data.Count;
                _idx = -1;
            }

            public ref T Current {
                [MethodImpl (MethodImplOptions.AggressiveInlining)]
                get => ref _data._denseItems[_idx].Value;
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public bool MoveNext () {
                return ++_idx < _count;
            }

            public void Dispose() {
                _data = default;
                _count = 0;
                _idx = 0;
            }
        }
    }   
}