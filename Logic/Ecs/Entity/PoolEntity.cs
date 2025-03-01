using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal class PoolEntity : IDisposable {
        private readonly int _worldId;
        private readonly int _capacity;
        private EntityData[] _freeEntities;
        private int[] _freeIndexes;
        private int _freeEntityCount;
        
        private bool _isDisposed;

        public PoolEntity(int worldId, int capacity) {
            _worldId = worldId;
            _capacity = capacity;
            Clear();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref EntityData Get() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(PoolEntity));
            }
            
            Resize();
            int value = Interlocked.Decrement(ref _freeEntityCount);
            ref var entity = ref _freeEntities[_freeIndexes[value]];
            entity.IncreaseGen();
            
            return ref entity;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Return(EntityData entityData) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(PoolEntity));
            }
            
            int value = Interlocked.Increment(ref _freeEntityCount) - 1;
            _freeIndexes[value] = entityData.Id;
            entityData.ClearComponents();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(PoolEntity));
            }
            
            _freeEntities = new EntityData[_capacity];
            _freeIndexes = new int[_capacity];
            _freeEntityCount = 0;
            for (int i = _freeEntities.Length - 1; i >= 0; i--) {
                _freeEntities[i] = new EntityData(new Entity(i, -1, _worldId), Options.COMPONENT_ENTITY_CAPACITY);
                _freeIndexes[_freeEntityCount] = i;
                _freeEntityCount++;
            }
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void Resize() {
            if (_freeEntityCount > 0) {
                return;
            }
            
            int last = _freeEntities.Length;
            int newCapacity = _freeEntities.Length * 2;
            Array.Resize(ref _freeEntities, newCapacity);
            Array.Resize(ref _freeIndexes, newCapacity);
            for (int i = newCapacity - 1; i >= last; i--) {
                _freeEntities[i] = new EntityData(new Entity(i, -1, _worldId), Options.COMPONENT_ENTITY_CAPACITY);
                _freeIndexes[_freeEntityCount] = i;
                _freeEntityCount++;
            }
        }

        public void Dispose() {
            _isDisposed = true;
            _freeEntities = null;
            _freeIndexes = null;
            _freeEntityCount = 0;
        }
    }   
}