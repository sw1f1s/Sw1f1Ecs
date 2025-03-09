using System.Runtime.CompilerServices;
using System.Threading;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class ConcurrentWorld : IWorld {
        public int Id { get; private set; }
        private readonly PoolEntity _entityPool;
        private readonly FilterMap _filterMap;
        private readonly ReaderWriterLockSlim _accessLock = new(LockRecursionPolicy.SupportsRecursion);
        private UnsafeSparseArray<EntityData> _entities = new (Options.ENTITY_CAPACITY);
        private SparseArray<AbstractComponentStorage> _components = new (Options.COMPONENT_CAPACITY);
        private bool _isDestroyed;
        
        bool IWorld.IsAlive => !_isDestroyed;
#if DEBUG
        SparseArray<EntityData> IWorld.SafeEntities => _entities.AsSafeArray();
#endif

        ref UnsafeSparseArray<EntityData> IWorld.Entities => ref _entities;
        
        public bool IsConcurrent => true;
        
        internal ConcurrentWorld(int id) {
            Id = id;
            _entityPool = new PoolEntity(id, Options.ENTITY_CAPACITY);
            _filterMap = new FilterMap(this);
        }
        
#region Entities
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T>() where T : struct, IComponent {
            var entity = CreateEntityInternal();
            entity.Set<T>();
            return entity;
        }
        
        public Entity CopyEntity(Entity entity) {
            var copyEntity = CreateEntityInternal();
            _accessLock.EnterWriteLock();
            try {
                ref var entityData = ref _entities.Get(entity.Id);
                foreach (var componentId in entityData.Components) {
                    _components.Get(componentId).CopyComponent(entity, copyEntity);
                    _entities.Get(copyEntity.Id).AddComponent(componentId);
                    _filterMap.UpdateFilters(componentId);
                }
                return copyEntity;
            }finally {
                _accessLock.ExitWriteLock();
            }
        }

        bool IWorld.EntityIsAlive(Entity entity) {
            _accessLock.EnterReadLock();
            try {
                if (!_entities.Has(entity.Id)) {
                    return false;
                }
            
                ref var e = ref _entities.Get(entity.Id);
                return e.GetEntity().Gen == entity.Gen;
            }finally {
                _accessLock.ExitReadLock();
            }
        }
        
        void IWorld.DestroyEntity(Entity entity) {
            _accessLock.EnterWriteLock();
            try {
                ref var entityData = ref _entities.Get(entity.Id);
                foreach (var componentId in entityData.Components) {
                    _components.Get(componentId).RemoveComponent(entity);
                    _filterMap.UpdateFilters(componentId);
                }
                _entityPool.Return(ref entityData);
                _entities.Remove(entity.Id);
            }finally {
                _accessLock.ExitWriteLock();
            }
        }
        
        void IWorld.AddComponent<T>(Entity entity, ref T component) {
            _accessLock.EnterWriteLock();
            try {
                var storage = GetComponentStorage<T>();
                storage.AddComponent(entity, ref component);
                _entities.Get(entity.Id).AddComponent(storage.Id);
                _filterMap.UpdateFilters(storage.Id);
            }finally {
                _accessLock.ExitWriteLock();
            }
        }
        
        bool IWorld.HasComponent<T>(Entity entity) {
            _accessLock.EnterReadLock();
            try {
                var storage = GetComponentStorage<T>();
                return storage.HasComponent(entity);
            }finally {
                _accessLock.ExitReadLock();
            }
        }
        
        ref T IWorld.GetComponent<T>(Entity entity) {
            _accessLock.EnterReadLock();
            try {
                var storage = GetComponentStorage<T>();
                return ref storage.GetComponent(entity);
            }finally {
                _accessLock.ExitReadLock();
            }
        }
        
        ref T IWorld.SetComponent<T>(Entity entity) {
            _accessLock.EnterWriteLock();
            try {
                var storage = GetComponentStorage<T>();
                _entities.Get(entity.Id).AddComponent(storage.Id);
                _filterMap.UpdateFilters(storage.Id);
                return ref storage.SetComponent(entity);
            }finally {
                _accessLock.ExitWriteLock();
            }
        }
        
        void IWorld.RemoveComponent<T>(Entity entity) {
            _accessLock.EnterWriteLock();
            try {
                var storage = GetComponentStorage<T>();
                storage.RemoveComponent(entity);
                ref var entityData = ref _entities.Get(entity.Id);
                entityData.RemoveComponent(storage.Id);
                _filterMap.UpdateFilters(storage.Id);
                if (entityData.IsEmpty) {
                    _entityPool.Return(ref entityData);
                    _entities.Remove(entity.Id);
                }
            }finally {
                _accessLock.ExitWriteLock();
            }
        }
        
        private Entity CreateEntityInternal() {
            _accessLock.EnterWriteLock();
            try {
                ref var entityData = ref _entityPool.Get();
                _entities.Add(entityData.Id, entityData);
                return entityData.GetEntity();
            }finally {
                _accessLock.ExitWriteLock();
            }
        }
#endregion

#region Components
        private ComponentStorage<T> GetComponentStorage<T>() where T : struct, IComponent {
            int componentId = ComponentStorageIndex<T>.StaticId;
            if (!_components.Has(componentId)) {
                _components.Add(componentId, new ComponentStorage<T>());
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
            if (_isDestroyed) {
                return;
            }
            
            _isDestroyed = true;
            _filterMap.Dispose();
            _entityPool.Dispose();
            _entities.Dispose();
            foreach (var component in _components) {
                component.Dispose();
            }
            _components.Dispose();
        }
    }   
}