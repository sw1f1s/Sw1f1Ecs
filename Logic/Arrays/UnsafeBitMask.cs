using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_5_3_OR_NEWER
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace Sw1f1.Ecs {
    public unsafe struct UnsafeBitMask : IDisposable {
        private const int BitsPerElement = 32;
        private uint* _bits;
        private uint _length;
        private uint _count;
        private bool _isDisposed;
        
        public uint Count => _count;
        
        public UnsafeBitMask(uint capacity) {
            _length = (capacity + BitsPerElement - 1) / BitsPerElement;
            _count = 0;
            _isDisposed = false;
#if UNITY_5_3_OR_NEWER
            _bits = (uint*)UnsafeUtility.Malloc(sizeof(uint) * _length, 4, Allocator.Persistent);
            UnsafeUtility.MemClear(_bits, sizeof(uint) * _length);
#else            
            _bits = (uint*)NativeMemory.AllocZeroed(sizeof(uint) * _length);
#endif
        }

        private UnsafeBitMask(in UnsafeBitMask copy) {
            if (copy._isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeBitMask));
            }
            
            _length = copy._length;
            _count = copy._count;
            _isDisposed = false;
            
#if UNITY_5_3_OR_NEWER
            _bits = (uint*)UnsafeUtility.Malloc(sizeof(uint) * _length, 4, Allocator.Persistent);
            UnsafeUtility.MemClear(_bits, sizeof(uint) * _length);
            UnsafeUtility.MemCpy(_bits, copy._bits, sizeof(uint) * _length);
#else            
            _bits = (uint*)NativeMemory.AllocZeroed(sizeof(uint) * _length);
            Unsafe.CopyBlock(_bits, copy._bits, sizeof(uint) * _length);
#endif
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public BitMask AsSafe() {
            BitMask bitMask = new BitMask((int)_length);
            foreach (var value in this){
                bitMask.Set(value);
            }
            return bitMask;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Set(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeBitMask));
            }
            
            var (arrayIndex, bitIndex) = GetIndices(id);
            TryResize(arrayIndex);
            _bits[arrayIndex] |= 1u << bitIndex;
            _count++;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Unset(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeBitMask));
            }
            
            var (arrayIndex, bitIndex) = GetIndices(id);
            if (arrayIndex < _length) {
                _bits[arrayIndex] &= ~(1u << bitIndex);
            }
            _count--;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Has(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeBitMask));
            }
            
            var (arrayIndex, bitIndex) = GetIndices(id);
            return arrayIndex < _length && (_bits[arrayIndex] & (1u << bitIndex)) != 0;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeBitMask));
            }
            
            for (uint i = 0; i < _length; i++) {
                _bits[i] = 0;
            }
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool HasAllCollision(in UnsafeBitMask other) {
            if (_isDisposed || other._isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeBitMask));
            }
            
            for (int i = 0; i < other._length; i++) {
                uint bit = i >= _length ? 0 : _bits[i];
                if ((bit & other._bits[i]) != other._bits[i]) {
                    return false;
                }
            }
            return true;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool HasAnyCollision(in UnsafeBitMask other) {
            if (_isDisposed || other._isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeBitMask));
            }
            
            uint minLength = Math.Min(_length, other._length);
            for (int i = 0; i < minLength; i++) {
                if ((_bits[i] & other._bits[i]) != 0) {
                    return true;
                }
            }
            return false;
        }
        
        public Enumerator GetEnumerator() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeBitMask));
            }
            
            return new Enumerator(this);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void TryResize(int minCapacity) {
            while (_length <= minCapacity) {
                Resize();
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void Resize() {
            uint newLength = _length * 2;
#if UNITY_5_3_OR_NEWER
            uint* newBits = (uint*)UnsafeUtility.Malloc(sizeof(uint) * newLength, 4, Allocator.Persistent);
            UnsafeUtility.MemClear(newBits, sizeof(uint) * newLength);
            UnsafeUtility.MemCpy(newBits, _bits, sizeof(uint) * _length);
            UnsafeUtility.Free(_bits, Allocator.Persistent);
#else            
            uint* newBits = (uint*)NativeMemory.AllocZeroed(sizeof(uint) * newLength);
            Unsafe.CopyBlock(newBits, _bits, sizeof(uint) * _length);
            NativeMemory.Free(_bits);
#endif
            
            _bits = newBits;
            _length = newLength;
        }
        
        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            
            _isDisposed = true;
#if UNITY_5_3_OR_NEWER
            UnsafeUtility.Free(_bits, Allocator.Persistent);
#else            
            NativeMemory.Free(_bits);
#endif
            _length = 0;
            _count = 0;
        }

        public static UnsafeBitMask operator &(in UnsafeBitMask a, in UnsafeBitMask b) {
            if (a._isDisposed || b._isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeBitMask));
            }
            
            uint resultLength = Math.Min(a._length, b._length);
            UnsafeBitMask result = new UnsafeBitMask(resultLength * BitsPerElement);
        
            for (int i = 0; i < resultLength; i++) {
                result._bits[i] = a._bits[i] & b._bits[i];
            }
        
            return result;
        }
    
        public static UnsafeBitMask operator |(in UnsafeBitMask a, in UnsafeBitMask b) {
            if (a._isDisposed || b._isDisposed) {
                throw new ObjectDisposedException(nameof(UnsafeBitMask));
            }
            
            uint resultLength = Math.Max(a._length, b._length);
            UnsafeBitMask result = new UnsafeBitMask(resultLength * BitsPerElement);
            uint minLength = Math.Min(a._length, b._length);
        
            for (int i = 0; i < minLength; i++) {
                result._bits[i] = a._bits[i] | b._bits[i];
            }
            
            UnsafeBitMask larger = a._length > b._length ? a : b;
            for (uint i = minLength; i < resultLength; i++) {
                result._bits[i] = larger._bits[i];
            }
        
            return result;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private static (int arrayIndex, int bitIndex) GetIndices(int id) {
            int arrayIndex = id / BitsPerElement;
            int bitIndex = id % BitsPerElement;
            return (arrayIndex, bitIndex);
        }
        
        public struct Enumerator : IDisposable {
            private UnsafeBitMask _data;
            private int _currentArrayIndex;
            private int _currentBitIndex;

            internal Enumerator (in UnsafeBitMask data) {
                _data = data;
                _currentArrayIndex = 0;
                _currentBitIndex = -1;
            }

            public int Current {
                [MethodImpl (MethodImplOptions.AggressiveInlining)]
                get => _currentArrayIndex * BitsPerElement + _currentBitIndex;
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public bool MoveNext () {
                for (int arrayIndex = _currentArrayIndex; arrayIndex < _data._length; arrayIndex++) {
                    uint chunk = _data._bits[arrayIndex];
                    if (chunk == 0) {
                        continue;
                    }
        
                    for (int bitIndex = _currentBitIndex + 1; bitIndex < BitsPerElement; bitIndex++) {
                        if ((chunk & (1u << bitIndex)) != 0) {
                            _currentArrayIndex = arrayIndex;
                            _currentBitIndex = bitIndex;
                            return true;
                        }
                    }

                    _currentBitIndex = -1;
                }

                return false;
            }

            public void Dispose() {
                _data = default;
                _currentArrayIndex = 0;
                _currentBitIndex = 0;
            }
        }
    }   
}