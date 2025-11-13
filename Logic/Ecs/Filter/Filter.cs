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
    public sealed class Filter : IDisposable {
        private FilterMap _map;
        private IWorld _world;
        private SparseArray<Entity> _entities = new SparseArray<Entity>(Options.ENTITY_CAPACITY);
        
        private int _mainComponent;
        private BitMask _includes;
        private BitMask _excludes;
        
        private bool _isDirty;
        private bool _isDisposed;
        
        internal SparseArray<Entity> Entities => _entities;
        public BitMask Includes => _includes;
        public BitMask Excludes => _excludes;
        
        internal IWorld World => _world;
        
#if DEBUG
        internal IReadOnlyList<Type> TypeIncludes => _world.GetTypeComponents(_includes);
        internal IReadOnlyList<Type> TypeExcludes => _world.GetTypeComponents(_excludes);
#endif
        
        internal Filter(FilterMask mask, FilterMap map, IWorld world) {
            _map = map;
            _world = world;
            _mainComponent = mask.MainComponent;
            _includes = mask.GetIncludes();
            _excludes = mask.GetExcludes();
            _isDirty = true;
        }
        
        public Enumerator GetEnumerator() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            
            UpdateIfDirty();
            return new Enumerator(this);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public uint GetCount() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            
            UpdateIfDirty();
            return _entities.Count;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void FillEntities(ref List<Entity> entities) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            
            UpdateIfDirty();
            entities.Clear();
            foreach (var entity in this) {
                entities.Add(entity);
            }
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Entity GetFirstOrDefault() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }

            UpdateIfDirty();
            foreach (var entity in this) {
                return entity;
            }

            return Entity.Empty;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void SetDirty() {
            _isDirty = true;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void UpdateIfDirty() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            
            _map.SetDirty();
            if (!_isDirty) {
                return;
            }

            _entities.Clear();
            _isDirty = false;
            
            if (!_world.HasComponentStorage(_mainComponent)) {
                return;
            }
            
            foreach (var entity in _world.GetComponentStorage(_mainComponent).Entities) {
                var entityData = _world.Entities.Get(entity.Id);
                if (entityData.Components.HasAllCollision(_includes) && !entityData.Components.HasAnyCollision(_excludes)) {
                    _entities.Add(entity.Id, entity);
                }
            }
        }

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            
            _isDisposed = true;
            _world = null;
            _map = null;
            _includes.Dispose();
            _excludes.Dispose();
            _entities.Dispose();
        }
        
        public struct Enumerator : IDisposable {
            private SparseArray<Entity>.Enumerator<Entity> _cache;

            internal Enumerator (Filter filter) {
                _cache = filter._entities.GetEnumerator();
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