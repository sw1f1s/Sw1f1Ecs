using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sw1f1.Ecs.Collections;
#if UNITY_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Sw1f1.Ecs {
#if UNITY_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal struct EntityData : IDisposable {
        private Entity _entity;
        private BitMask _components;
        private bool _isDisposed;
        
        public int Id => _entity.Id;
        public BitMask Components => _components;
        public bool IsEmpty => _components.Count == 0;
        
#if DEBUG
        public IReadOnlyList<Type> TypeComponents => WorldBuilder.GetWorld(_entity.WorldId).GetTypeComponents(_components);
#endif

        public EntityData(Entity entity, int componentCapacity) {
            _entity = entity;
            _components = new BitMask(componentCapacity);
            _isDisposed = false;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public readonly Entity GetEntity() => _entity;

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
            _entity = _entity.ResetGen();
            _components.Clear();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void IncreaseGen() => 
            _entity = _entity.IncreaseGen();
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void IncreaseGen(int gen) => 
            _entity = new Entity(_entity.Id, gen, _entity.WorldId);

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