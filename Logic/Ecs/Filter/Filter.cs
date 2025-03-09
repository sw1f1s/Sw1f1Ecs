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
        private UnsafeSparseArray<Entity> _cache = new(Options.ENTITY_CAPACITY);
        
        private UnsafeBitMask _includes;
        private UnsafeBitMask _excludes;
        
        private bool _needUpdate;
        private bool _isDisposed;
        
        internal UnsafeSparseArray<Entity> Cache => _cache;
        public bool IsConcurrent => _world.IsConcurrent;
        public UnsafeBitMask Includes => _includes;
        public UnsafeBitMask Excludes => _excludes;
        
#if DEBUG
        internal SparseArray<Entity> SafeCache => _cache.AsSafeArray();
        internal BitMask SafeIncludes => _includes.AsSafe();
        internal BitMask SafeExcludes => _excludes.AsSafe();
        internal IReadOnlyList<Type> TypeIncludes => _world.GetTypeComponents(_includes);
        internal IReadOnlyList<Type> TypeExcludes => _world.GetTypeComponents(_excludes);
#endif
        
        internal Filter(FilterMask mask, IWorld world) {
            _world = world;
            _includes = mask.GetIncludes();
            _excludes = mask.GetExcludes();
            _needUpdate = true;
        }
        
        public Enumerator GetEnumerator() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            
            Update();
            return new Enumerator(this);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public uint GetCount() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            
            Update();
            return _cache.Count;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public List<Entity> FillEntities(List<Entity> entities) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            
            Update();
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

            Update();
            foreach (var entity in this) {
                return entity;
            }

            return Entity.Empty;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void SetNeedUpdate() {
            _needUpdate = true;
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

            _cache.Clear();
            foreach (ref var entityData in _world.Entities) {
                if (entityData.Components.HasAllCollision(_includes) && !entityData.Components.HasAnyCollision(_excludes)) {
                    var entity = entityData.GetEntity();
                    _cache.Add(entity.Id, entity);
                }
            }

            _needUpdate = false;
        }

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            
            _isDisposed = true;
            _world = null;
            _includes.Dispose();
            _excludes.Dispose();
            _cache.Dispose();
        }
        
        public struct Enumerator : IDisposable {
            private UnsafeSparseArray<Entity>.Enumerator<Entity> _cache;

            internal Enumerator (Filter filter) {
                _cache = filter._cache.GetEnumerator();
            }

            public Entity Current {
                [MethodImpl (MethodImplOptions.AggressiveInlining)]
                get => _cache.Current;
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public bool MoveNext () {
                return _cache.MoveNext();
            }

            public void Dispose() {
                _cache.Dispose();
                _cache = default;
            }
        }
    }
}