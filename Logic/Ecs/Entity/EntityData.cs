using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal struct EntityData : IDisposable {
        private Entity _entity;
        private UnsafeBitMask _components;
        private bool _isDisposed;
        
        public int Id => _entity.Id;
        public UnsafeBitMask Components => _components;
        public bool IsEmpty => _components.Count == 0;
        
#if DEBUG
        public BitMask SafeComponents => _components.AsSafe();
        public IReadOnlyList<Type> TypeComponents => WorldBuilder.GetWorld(_entity.WorldId).GetTypeComponents(_components);
#endif

        public EntityData(Entity entity, uint componentCapacity) {
            _entity = entity;
            _components = new UnsafeBitMask(componentCapacity);
            _isDisposed = false;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Entity GetEntity() => _entity;

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
        public void Clear() {
            _entity.ResetGen();
            _components.Clear();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void IncreaseGen() => _entity.IncreaseGen();

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            
            _isDisposed = true;
            _components.Dispose();
            _entity = default;
            _components = default;
        }
    } 
}