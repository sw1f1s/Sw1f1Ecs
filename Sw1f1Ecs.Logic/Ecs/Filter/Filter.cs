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
        internal void Update(int componentId) {
            if (_includes.Has(componentId) || _excludes.Has(componentId)) {
                _needUpdate = true;
            }
        }
        
        public Enumerator GetEnumerator() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            
            return new Enumerator(this);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetCount() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }

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
        public IReadOnlyList<Entity> GetEntities() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            
            var list = new List<Entity>();
            foreach (var entity in this) {
                list.Add(entity);
            }

            return list;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void Update() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (!_needUpdate) {
                return;
            }

            foreach (var entity in this) { }
        }

        public void Dispose() {
            _isDisposed = true;
            _world = null;
            _includes?.Clear();
            _excludes?.Clear();
            _сache?.Dispose();
            _сache = null;
            _includes = null;
            _excludes = null;
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
                    _cache = _filter._сache.GetEnumerator();
                } else {
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