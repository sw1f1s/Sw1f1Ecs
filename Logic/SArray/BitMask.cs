using System;
using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public struct BitMask {
        private const int BitsPerElement = 32;
        private uint[] _bits;
        private uint _count;
        
        public uint Count => _count;
        
        public BitMask(int capacity) {
            int arraySize = (capacity + BitsPerElement - 1) / BitsPerElement;
            _bits = new uint[arraySize];
            _count = 0;
        }

        private BitMask(BitMask other) {
            _bits = new uint[other._bits.Length];
            _count = other._count;
            Array.Copy(other._bits, _bits, other._bits.Length);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Set(int id) {
            var (arrayIndex, bitIndex) = GetIndices(id);
            TryResize(arrayIndex + 1);
            _bits[arrayIndex] |= 1u << bitIndex;
            _count++;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Unset(int id) {
            var (arrayIndex, bitIndex) = GetIndices(id);
            if (arrayIndex < _bits.Length) {
                _bits[arrayIndex] &= ~(1u << bitIndex);
            }
            _count--;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Has(int id) {
            var (arrayIndex, bitIndex) = GetIndices(id);
            return arrayIndex < _bits.Length && (_bits[arrayIndex] & (1u << bitIndex)) != 0;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            Array.Clear(_bits, 0, _bits.Length);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool HasAllCollision(BitMask other) {
            if (_bits.Length < other._bits.Length)
                return false;
            
            for (int i = 0; i < other._bits.Length; i++) {
                if ((_bits[i] & other._bits[i]) != other._bits[i]) {
                    return false;
                }
            }
            return true;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool HasAnyCollision(BitMask other) {
            int minLength = Math.Min(_bits.Length, other._bits.Length);
            for (int i = 0; i < minLength; i++) {
                if ((_bits[i] & other._bits[i]) != 0) {
                    return true;
                }
            }
            return false;
        }
        
        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        public int GetHashId() {
            unchecked {
                const int prime = 16777619;
                int hash = (int)2166136261;

                foreach (uint bit in _bits) {
                    hash ^= (int)bit;
                    hash *= prime;
                }

                return hash;
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void TryResize(int minCapacity) {
            if (_bits.Length < minCapacity) {
                int newCapacity = Math.Max(_bits.Length * 2, minCapacity);
                uint[] newBits = new uint[newCapacity];
                Array.Copy(_bits, newBits, _bits.Length);
                _bits = newBits;
            }
        }

        public static BitMask operator &(BitMask a, BitMask b) {
            int resultLength = Math.Min(a._bits.Length, b._bits.Length);
            BitMask result = new BitMask(resultLength * BitsPerElement);
        
            for (int i = 0; i < resultLength; i++) {
                result._bits[i] = a._bits[i] & b._bits[i];
            }
        
            return result;
        }
    
        public static BitMask operator |(BitMask a, BitMask b) {
            int resultLength = Math.Max(a._bits.Length, b._bits.Length);
            BitMask result = new BitMask(resultLength * BitsPerElement);
            int minLength = Math.Min(a._bits.Length, b._bits.Length);
        
            for (int i = 0; i < minLength; i++) {
                result._bits[i] = a._bits[i] | b._bits[i];
            }
            
            BitMask larger = a._bits.Length > b._bits.Length ? a : b;
            for (int i = minLength; i < resultLength; i++) {
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
            private uint[] _bits;
            private int _currentArrayIndex;
            private int _currentBitIndex;

            internal Enumerator (BitMask data) {
                _bits = data._bits;
                _currentArrayIndex = 0;
                _currentBitIndex = -1;
            }

            public int Current {
                [MethodImpl (MethodImplOptions.AggressiveInlining)]
                get => _currentArrayIndex * BitsPerElement + _currentBitIndex;
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public bool MoveNext () {
                for (int arrayIndex = _currentArrayIndex; arrayIndex < _bits.Length; arrayIndex++) {
                    uint chunk = _bits[arrayIndex];
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
                _bits = null;
                _currentArrayIndex = 0;
                _currentBitIndex = 0;
            }
        }
    }   
}