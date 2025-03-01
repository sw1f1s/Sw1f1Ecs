using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal class EntityData : ISparseItem, IDisposable {
        private Entity _entity;
        private BitMask _components;
        public int Id => _entity.Id;
        public BitMask Components => _components;
        public bool IsEmpty => _components.Count == 0;

        public EntityData(Entity entity, int componentCapacity) {
            _entity = entity;
            _components = new BitMask(componentCapacity);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Entity GetEntity() => ref _entity;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void AddComponent(int componentId) {
            _components.Set(componentId);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent(int componentId) {
            _components.Unset(componentId);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void ClearComponents() {
            _components.Clear();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void IncreaseGen() => _entity.IncreaseGen();

        public void Dispose() {
            _components?.Clear();
            _components = null;
        }
    } 
}