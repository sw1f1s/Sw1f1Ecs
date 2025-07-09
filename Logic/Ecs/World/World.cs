using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sw1f1.Ecs.Collections;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class World : IWorld {
        public int Id { get; private set; }
        private readonly EntityStorage _entityStorage;
        private readonly FilterMap _filterMap;
        private readonly ConcurrentBuffer _concurrentBuffer;
        private readonly ComponentsStorage _componentsStorage;
        private int _lock;
        private bool _isDisposed;

        bool IWorld.IsAlive => !_isDisposed;

        IComponentsStorage IWorld.ComponentsStorage => _componentsStorage;

        ref SparseArray<EntityData> IWorld.Entities => ref _entityStorage.Entities;
        
        internal World(int id) {
            Id = id;
            _entityStorage = new EntityStorage(id, Options.ENTITY_CAPACITY);
            _filterMap = new FilterMap(this);
            _concurrentBuffer = new ConcurrentBuffer(this);
            _componentsStorage = new ComponentsStorage();
        }
        
#region Entities

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T>() where T : struct, IComponent {
            if (_lock > 0) {
                throw new NotSupportedException("You cannot create entity while the world is locked");
            }
            
            var entity = CreateEntityInternal();
            entity.Set<T>();
            return entity;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Entity CopyEntity(in Entity entity) {
            if (_lock > 0) {
                throw new NotSupportedException("You cannot copy entity while the world is locked");
            }
            
            var copyEntity = CreateEntityInternal();
            ref var entityData = ref _entityStorage.Get(entity);
            foreach (var componentId in entityData.Components) {
                _componentsStorage.Get(componentId).CopyComponent(entity, copyEntity);
                _entityStorage.Get(copyEntity).AddComponent(componentId);
                _filterMap.UpdateFilters(componentId);
            }
            return copyEntity;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        bool IWorld.EntityIsAlive(in Entity entity) {
            if (!_entityStorage.Has(entity)) {
                return false;
            }
            
            ref var entityData = ref _entityStorage.Get(entity);
            return entityData.GetEntity().Gen == entity.Gen;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        void IWorld.DestroyEntity(in Entity entity) {
            if (_lock > 0) {
                throw new NotSupportedException("You cannot destroy entity while the world is locked");
            }
            
            var entityData = _entityStorage.Get(entity);
            foreach (var componentId in entityData.Components) {
                _componentsStorage.Get(componentId).RemoveComponent(entity);
                _filterMap.UpdateFilters(componentId);
            }
            _entityStorage.Return(entityData);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        bool IWorld.HasComponent<T>(in Entity entity) {
            var storage = _componentsStorage.GetComponentStorage<T>();
            return storage.HasComponent(entity);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        ref T IWorld.GetComponent<T>(in Entity entity) {
            var storage = _componentsStorage.GetComponentStorage<T>();
            return ref storage.GetComponent(entity);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        void IWorld.AddComponent<T>(in Entity entity, ref T component) {
            if (_lock > 0) {
                _concurrentBuffer.AddComponent(entity, ref component);
                return;
            }
            
            var storage = _componentsStorage.GetComponentStorage<T>();
            storage.AddComponent(entity, ref component);
            _entityStorage.Get(entity).AddComponent(storage.Id);
            _filterMap.UpdateFilters(storage.Id);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        ref T IWorld.SetComponent<T>(in Entity entity) {
            if (_lock > 0) {
                throw new NotSupportedException("You cannot set components while the world is locked");
            }
            
            var storage = _componentsStorage.GetComponentStorage<T>();
            _entityStorage.Get(entity).AddComponent(storage.Id);
            _filterMap.UpdateFilters(storage.Id);
            return ref storage.SetComponent(entity);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        void IWorld.RemoveComponent<T>(in Entity entity) {
            if (_lock > 0) {
                _concurrentBuffer.RemoveComponent<T>(entity);
                return;
            }
            
            var storage = _componentsStorage.GetComponentStorage<T>();
            RemoveComponentInternal(entity, storage);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        void IWorld.RemoveComponent(in Entity entity, int componentIdx) {
            var storage = _componentsStorage.Get(componentIdx);
            RemoveComponentInternal(entity, storage);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private Entity CreateEntityInternal() {
            ref var entityData = ref _entityStorage.GetFreeEntity();
            return entityData.GetEntity();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void RemoveComponentInternal(in Entity entity, AbstractComponentStorage storage) {
            if (storage.RemoveComponent(entity)) {
                ref var entityData = ref _entityStorage.Get(entity);
                entityData.RemoveComponent(storage.Id);
                _filterMap.UpdateFilters(storage.Id);
                if (entityData.IsEmpty) {
                    _entityStorage.Return(entityData);
                }   
            }
        }
#endregion

#region Components
        AbstractComponentStorage IWorld.GetComponentStorage(int componentId) {
            return _componentsStorage.Get(componentId);
        }

        bool IWorld.HasComponentStorage(int componentId) {
            return _componentsStorage.Has(componentId);
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

        void IWorld.Lock() {
            _lock++;
        }

        void IWorld.Unlock() {
            _lock--;
            if (_lock == 0) {
                _concurrentBuffer.Execute();
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            _filterMap.Clear();
            _componentsStorage.Clear();
            _entityStorage.Clear();
            _concurrentBuffer.Clear();
            _lock = 0;
        }

        ~World() => 
            Dispose();

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            
            WorldBuilder.Destroy(this);
            _isDisposed = true;
            _filterMap.Dispose();
            _concurrentBuffer.Dispose();
            _entityStorage.Dispose();
            _componentsStorage.Dispose();
        }
    }   
}