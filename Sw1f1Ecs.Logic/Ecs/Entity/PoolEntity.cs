using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public class PoolEntity {
        private readonly int _worldId;
        private readonly int _capacity;
        private Entity[] _freeEntities;
        private int[] _freeIndexes;
        private int _freeEntityCount;

        public PoolEntity(int worldId, int capacity) {
            _worldId = worldId;
            _capacity = capacity;
            Clear();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Entity Get() {
            if (_freeEntityCount == 0) {
                Resize();
            }
            
            _freeEntityCount--;
            ref var entity = ref _freeEntities[_freeIndexes[_freeEntityCount]];
            entity.IncreaseGen();
            
            return ref entity;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Return(Entity entity) {
            _freeIndexes[_freeEntityCount] = entity.Id;
            _freeEntityCount++;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            _freeEntities = new Entity[_capacity];
            _freeIndexes = new int[_capacity];
            _freeEntityCount = 0;
            for (int i = _freeEntities.Length - 1; i >= 0; i--) {
                _freeEntities[i] = new Entity(i, -1, _worldId);
                _freeIndexes[_freeEntityCount] = i;
                _freeEntityCount++;
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void Resize() {
            int last = _freeEntities.Length;
            int newCapacity = _freeEntities.Length * 2;
            Array.Resize(ref _freeEntities, newCapacity);
            Array.Resize(ref _freeIndexes, newCapacity);
            for (int i = newCapacity - 1; i >= last; i--) {
                _freeEntities[i] = new Entity(i, -1, _worldId);
                _freeIndexes[_freeEntityCount] = i;
                _freeEntityCount++;
            }
        }
    }   
}