using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class Filter : IDisposable, IConcurrentSupport {
        private IWorld _world;
        private SparseArray<Entity> _сache = new(Options.ENTITY_CAPACITY);
        
        private BitMask _includes;
        private BitMask _excludes;
        
        private bool _needUpdate;
        private bool _isDisposed;
        
        internal SparseArray<Entity> Cache => _сache;
        public bool IsConcurrent => _world.IsConcurrent;
        
        public Filter(FilterMask mask, IWorld world) {
            _world = world;
            _includes = mask.GetIncludes();
            _excludes = mask.GetExcludes();
            _needUpdate = true;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void NeedUpdate() {
            _needUpdate = true;
        }
        
        public Enumerator GetEnumerator() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            
            _world.UpdateFilters();
            return new Enumerator(this);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetCount() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            _world.UpdateFilters();
            if (!_needUpdate) {
                return _сache.Count;
            }

            int count = 0;
            foreach (var entity in this) {
                count++;
            }

            return count;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public List<Entity> FillEntities(List<Entity> entities) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            
            _world.UpdateFilters();
            entities.Clear();
            foreach (var entity in this) {
                entities.Add(entity);
            }

            return entities;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Entity GetFirstOrDefault() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            
            _world.UpdateFilters();
            foreach (var entity in this) {
                return entity;
            }

            return Entity.Empty;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void Update() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }

            _world.UpdateFilters();
            if (!_needUpdate) {
                return;
            }

            foreach (var entity in this) { }
        }

        public void Dispose() {
            _isDisposed = true;
            _world = null;
            _includes.Clear();
            _excludes.Clear();
            _сache?.Dispose();
            _сache = null;
        }
        
        public struct Enumerator : IDisposable {
            private readonly bool _useCache;
            private SparseArray<Entity>.Enumerator<Entity> _cache;
            private SparseArray<EntityData>.Enumerator<EntityData> _collection;
            private Filter _filter;

            internal Enumerator (Filter filter) {
                _filter = filter;
                _useCache = !_filter._needUpdate;

                if (_useCache) {
                    _collection = default;
                    _cache = _filter._сache.GetEnumerator();
                } else {
                    _cache = default;
                    _collection = _filter._world.Entities.GetEnumerator();   
                    _filter._сache.Clear();
                    _filter._needUpdate = false;
                }
            }

            public Entity Current {
                [MethodImpl (MethodImplOptions.AggressiveInlining)]
                get => (_useCache) ? _cache.Current : _collection.Current.GetEntity();
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public bool MoveNext () {
                if (_useCache) {
                    return CalculateCacheNext();
                }

                return CalculateCollectionNext();
            }

            public void Dispose() {
                _cache.Dispose();
                _collection.Dispose();
                _filter = null;
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            private bool CalculateCacheNext() {
                if (!_cache.MoveNext()) {
                    return false;
                }

                return true;
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            private bool CalculateCollectionNext() {
                if (!_collection.MoveNext()) {
                    return false;
                }
                
                var entityData = _collection.Current;
                if (entityData.Components.HasAllCollision(_filter._includes) && !entityData.Components.HasAnyCollision(_filter._excludes)) {
                    _filter._сache.Add(entityData.GetEntity());
                    return true;
                }
                
                return CalculateCollectionNext();
            }
        }
    }
}