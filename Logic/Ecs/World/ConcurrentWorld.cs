using System.Runtime.CompilerServices;
using System.Threading;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class ConcurrentWorld : IWorld {
        public int Id { get; private set; }
        private readonly EntityStorage _entityStorage;
        private readonly FilterMap _filterMap;
        private readonly ReaderWriterLockSlim _accessLock = new(LockRecursionPolicy.SupportsRecursion);
        private SparseArray<AbstractComponentStorage> _components = new (Options.COMPONENT_CAPACITY);
        private bool _isDisposed;
        
        bool IWorld.IsAlive => !_isDisposed;
#if DEBUG
        SparseArray<EntityData> IWorld.SafeEntities => _entityStorage.Entities.AsSafeArray();
#endif

        ref UnsafeSparseArray<EntityData> IWorld.Entities => ref _entityStorage.Entities;
        
        public bool IsConcurrent => true;
        
        internal ConcurrentWorld(int id) {
            Id = id;
            _entityStorage = new EntityStorage(id, Options.ENTITY_CAPACITY);
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
                ref var entityData = ref _entityStorage.Get(entity);
                foreach (var componentId in entityData.Components) {
                    _components.Get(componentId).CopyComponent(entity, copyEntity);
                    _entityStorage.Get(copyEntity).AddComponent(componentId);
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
                if (!_entityStorage.Has(entity)) {
                    return false;
                }
            
                ref var e = ref _entityStorage.Get(entity);
                return e.GetEntity().Gen == entity.Gen;
            }finally {
                _accessLock.ExitReadLock();
            }
        }
        
        void IWorld.DestroyEntity(Entity entity) {
            _accessLock.EnterWriteLock();
            try {
                ref var entityData = ref _entityStorage.Get(entity);
                foreach (var componentId in entityData.Components) {
                    _components.Get(componentId).RemoveComponent(entity);
                    _filterMap.UpdateFilters(componentId);
                }
                _entityStorage.Return(entityData);
            }finally {
                _accessLock.ExitWriteLock();
            }
        }
        
        void IWorld.AddComponent<T>(Entity entity, ref T component) {
            _accessLock.EnterWriteLock();
            try {
                var storage = GetComponentStorage<T>();
                storage.AddComponent(entity, ref component);
                _entityStorage.Get(entity).AddComponent(storage.Id);
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
                _entityStorage.Get(entity).AddComponent(storage.Id);
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
                ref var entityData = ref _entityStorage.Get(entity);
                entityData.RemoveComponent(storage.Id);
                _filterMap.UpdateFilters(storage.Id);
                if (entityData.IsEmpty) {
                    _entityStorage.Return(entityData);
                }
            }finally {
                _accessLock.ExitWriteLock();
            }
        }
        
        private Entity CreateEntityInternal() {
            _accessLock.EnterWriteLock();
            try {
                ref var entityData = ref _entityStorage.GetFreeEntity();
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
            _filterMap.Clear();
            foreach (var component in _components) {
                component.Clear();
            }
            _entityStorage.Clear();
        }
        
        ~ConcurrentWorld() => 
            Dispose();

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            
            _isDisposed = true;
            _filterMap.Dispose();
            _entityStorage.Dispose();
            foreach (var component in _components) {
                component.Dispose();
            }
            _components.Dispose();
        }
    }   
}