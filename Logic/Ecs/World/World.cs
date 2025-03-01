using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class World : IWorld {
        public int Id { get; private set; }
        private readonly PoolEntity _entityPool;
        private readonly FilterMap _filterMap;
        private readonly SparseArray<EntityData> _entities = new (Options.ENTITY_CAPACITY);
        private readonly SparseArray<AbstractComponentStorage> _components = new (Options.COMPONENT_CAPACITY);
        private bool _isDestroyed;
        
        bool IWorld.IsAlive => !_isDestroyed;
        SparseArray<EntityData> IWorld.Entities => _entities;
        
        public bool IsConcurrent => false;
        
        internal World(int id) {
            Id = id;
            _entityPool = new PoolEntity(id, Options.ENTITY_CAPACITY);
            _filterMap = new FilterMap(this);
        }
        
#region Entities

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Entity CreateEntity<T>() where T : struct, IComponent {
            ref var entity = ref CreateEntityInternal();
            entity.Set<T>();
            return ref entity;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Entity CopyEntity(Entity entity) {
            ref var copyEntity = ref CreateEntityInternal();
            var entityData = _entities.Get(entity.Id);
            foreach (var componentId in entityData.Components) {
                _components.Get(componentId).CopyComponent(entity, copyEntity);
                _entities.Get(copyEntity.Id).AddComponent(componentId);
                _filterMap.UpdateFilters(componentId);
            }
            return ref copyEntity;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        bool IWorld.EntityIsAlive(Entity entity) {
            if (!_entities.Has(entity.Id)) {
                return false;
            }
            
            var e = _entities.Get(entity.Id);
            return e.GetEntity().Gen == entity.Gen;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        void IWorld.DestroyEntity(Entity entity) {
            var entityData = _entities.Get(entity.Id);
            foreach (var componentId in entityData.Components) {
                _components.Get(componentId).RemoveComponent(entity);
                _filterMap.UpdateFilters(componentId);
            }
            _entityPool.Return(entityData);
            _entities.Remove(entity.Id);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        void IWorld.AddComponent<T>(Entity entity, ref T component) {
            var storage = GetComponentStorage<T>();
            storage.AddComponent(entity, ref component);
            _entities.Get(entity.Id).AddComponent(storage.Id);
            _filterMap.UpdateFilters(storage.Id);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        bool IWorld.HasComponent<T>(Entity entity) {
            var storage = GetComponentStorage<T>();
            return storage.HasComponent(entity);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        ref T IWorld.GetComponent<T>(Entity entity) {
            var storage = GetComponentStorage<T>();
            return ref storage.GetComponent(entity);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        ref T IWorld.SetComponent<T>(Entity entity) {
            var storage = GetComponentStorage<T>();
            _entities.Get(entity.Id).AddComponent(storage.Id);
            _filterMap.UpdateFilters(storage.Id);
            return ref storage.SetComponent(entity);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        void IWorld.RemoveComponent<T>(Entity entity) {
            var storage = GetComponentStorage<T>();
            storage.RemoveComponent(entity);
            var entityData = _entities.Get(entity.Id);
            entityData.RemoveComponent(storage.Id);
            _filterMap.UpdateFilters(storage.Id);
            if (entityData.IsEmpty) {
                _entityPool.Return(entityData);
                _entities.Remove(entity.Id);
            }
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private ref Entity CreateEntityInternal() {
            ref var entityData = ref _entityPool.Get();
            _entities.Add(entityData);
            return ref entityData.GetEntity();
        }
#endregion

#region Components
        private ComponentStorage<T> GetComponentStorage<T>() where T : struct, IComponent {
            int componentId = ComponentStorageIndex<T>.StaticId;
            if (!_components.Has(componentId)) {
                _components.Add(new ComponentStorage<T>(Options.COMPONENT_ENTITY_CAPACITY));
            }
            
            return Unsafe.As<ComponentStorage<T>>(_components.Get(componentId));
        }

        AbstractComponentStorage IWorld.GetComponentStorage(int componentId) {
            return _components.Get(componentId);
        }

        bool IWorld.HasComponentStorage(int componentId) {
            return _components.Has(componentId);
        }
#endregion

#region Filters
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Filter GetFilter(FilterMask mask) {
            return _filterMap.GetFilter(mask);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        void IWorld.UpdateFilters() {
            _filterMap.UpdateFilters();
        }
#endregion

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            _entities.Clear();
            _filterMap.Clear();
            foreach (var component in _components) {
                component.Clear();
            }
            _entityPool.Clear();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        void IWorld.Destroy() {
            _isDestroyed = true;
            _entities.Dispose();
            foreach (var component in _components) {
                component.Dispose();
            }
            _components.Dispose();
            _filterMap.Dispose();
            _entityPool.Dispose();
        }
    }   
}