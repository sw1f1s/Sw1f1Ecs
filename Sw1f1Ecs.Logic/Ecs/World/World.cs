using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class World : ISparseItem {
        public int Id { get; private set; }
        private readonly PoolEntity _entityPool;
        private readonly Dictionary<FilterMask, Filter> _filters = new (Options.FILTER_CAPACITY);
        private readonly SparseArray<Entity> _entities = new (Options.ENTITY_CAPACITY);
        private readonly SparseArray<ComponentStorage> _components = new (Options.COMPONENT_CAPACITY);

        internal SparseArray<Entity> Entities => _entities;
        internal SparseArray<ComponentStorage> Components => _components;
        
        internal World(int id) {
            Id = id;
            _entityPool = new PoolEntity(id, Options.ENTITY_CAPACITY);
        }
        
#region Entities
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Entity CreateEntity() {
            ref var entity = ref _entityPool.Get();
            _entities.Add(entity);
            return ref entity;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Entity CopyEntity(Entity entity) {
            ref var copyEntity = ref CreateEntity();
            foreach (var component in _components) {
                component.CopyComponent(entity, copyEntity);
            }
            return ref copyEntity;
        } 

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal bool EntityIsAlive(Entity entity) {
            if (!_entities.Has(entity.Id)) {
                return false;
            }
            
            var e = _entities.Get(entity.Id);
            return e.Gen == entity.Gen;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void DestroyEntity(Entity entity) {
            if (_entities.Remove(entity.Id)) {
                _entityPool.Return(entity);
                foreach (var storage in _components) {
                    if (storage.HasComponent(entity)) {
                        storage.RemoveComponent(entity);   
                    }
                }
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void AddComponent<T>(Entity entity, ref T component) where T : struct, IComponent {
            var storage = GetComponentStorage<T>();
            storage.AddComponent(entity, ref component);
            UpdateFilters(storage.Id);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal bool HasComponent<T>(Entity entity) where T : struct, IComponent {
            var storage = GetComponentStorage<T>();
            return storage.HasComponent(entity);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal ref T GetComponent<T>(Entity entity) where T : struct, IComponent {
            var storage = GetComponentStorage<T>();
            return ref storage.GetComponent(entity);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal ref T GetOrSetComponent<T>(Entity entity) where T : struct, IComponent {
            var storage = GetComponentStorage<T>();
            if (!storage.HasComponent(entity)) {
                UpdateFilters(storage.Id);
            }
            return ref storage.GetOrSetComponent(entity);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void RemoveComponent<T>(Entity entity) where T : struct, IComponent {
            var storage = GetComponentStorage<T>();
            storage.RemoveComponent(entity);
            UpdateFilters(storage.Id);
        }
#endregion

#region Components
        internal ComponentStorage<T> GetComponentStorage<T>() where T : struct, IComponent {
            int componentId = ComponentStorageIndex<T>.StaticId;
            if (!_components.Has(componentId)) {
                _components.Add(new ComponentStorage<T>(Options.COMPONENT_ENTITY_CAPACITY));
            }
            
            return Unsafe.As<ComponentStorage<T>>(_components.Get(componentId));
        }
        
        internal ComponentStorage GetComponentStorage(int componentId) {
            return _components.Get(componentId);
        }
        
        internal bool HasComponentStorage(int componentId) {
            return _components.Has(componentId);
        }
#endregion

#region Filters
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Filter GetFilter(FilterMask mask) {
            if (_filters.TryGetValue(mask, out var filter)) {
                return filter;
            }
            
            var newFilter = new Filter(mask, this);
            _filters.Add(mask, newFilter);
            return newFilter;
        }
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void UpdateFilters(int componentId) {
            foreach (var filter in _filters) {
                filter.Value.Update(componentId);
            }
        }
#endregion

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            foreach (var filter in _filters) {
                filter.Value.Dispose();
            }
            
            _filters.Clear();
            _entities.Clear();
            foreach (var component in _components) {
                component.Clear();
            }
            _entityPool.Clear();
        }

        internal void Destroy() {
            _entities.Dispose();
            foreach (var component in _components) {
                component.DeepClear();
            }
            _components.Dispose();
            foreach (var filter in _filters) {
                filter.Value.Dispose();
            }
            _filters.Clear();
            _entityPool.Clear();
        }
    }   
}