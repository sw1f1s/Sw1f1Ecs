using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class Filter : IDisposable {
        private World _world;
        private SparseArray<Entity> _сache = new(Options.ENTITY_CAPACITY);
        
        private int[] _includes;
        private int[] _excludes;
        
        private bool _needUpdate;
        private bool _isDisposed;
        
        public SparseArray<Entity> Cache => _сache;
        
        public Filter(FilterMask mask, World world) {
            _world = world;
            _includes = mask.GetIncludes();
            _excludes = mask.GetExcludes();
            _needUpdate = true;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Update(int componentId) {
            for (int i = 0; i < _includes.Length; i++) {
                if (_includes[i] == componentId) {
                    _needUpdate = true;
                    break;
                }
            }
            
            for (int i = 0; i < _excludes.Length; i++) {
                if (_excludes[i] == componentId) {
                    _needUpdate = true;
                    break;
                }
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

        public void Update() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (!_needUpdate) {
                return;
            }

            foreach (var entity in this) { }
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

        public void Dispose() {
            _isDisposed = true;
            _world = null;
            _includes = null;
            _excludes = null;
            _сache?.DeepClear();
            _сache = null;
        }
        
        public struct Enumerator : IDisposable {
            private readonly bool _needUpdate;
            
            private SparseArray<Entity>.Enumerator<Entity> _collection;
            private Filter _filter;

            internal Enumerator (Filter filter) {
                _filter = filter;
                _needUpdate = _filter._needUpdate;

                if (_needUpdate) {
                    _collection = _filter._world.Entities.GetEnumerator();   
                    _filter._сache.Clear();
                    _filter._needUpdate = false;
                } else {
                    _collection = _filter._сache.GetEnumerator();
                }
            }

            public Entity Current {
                [MethodImpl (MethodImplOptions.AggressiveInlining)]
                get => _collection.Current;
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public bool MoveNext () {
                return MoveNextCollection();
            }

            public void Dispose() {
                _collection.Dispose();
                _filter = null;
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            private bool MoveNextCollection() {
                if (!_collection.MoveNext()) {
                    return false;
                }
                
                var entity = _collection.Current;
                if (!_needUpdate) {
                    return true;
                }
                
                for (int i = 0; i < _filter._includes.Length; i++) {
                    int index = _filter._includes[i];
                    if (!_filter._world.HasComponentStorage(index)) {
                        return false;
                    }

                    var storage = _filter._world.GetComponentStorage(index);
                    if (!storage.HasComponent(entity)) {
                        return MoveNextCollection();
                    }
                }
                
                for (int i = 0; i < _filter._excludes.Length; i++) {
                    int index = _filter._excludes[i];
                    if (!_filter._world.HasComponentStorage(index)) {
                       continue;
                    }

                    var storage = _filter._world.GetComponentStorage(index);
                    if (storage.HasComponent(entity)) {
                        return MoveNextCollection();
                    }
                }

                _filter._сache.Add(entity);
                return true;
            }
        }
    }
}