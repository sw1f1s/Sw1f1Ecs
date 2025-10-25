using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Sw1f1.Ecs.Collections;
#if UNITY_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Sw1f1.Ecs {
#if UNITY_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal class EntityStorage : IDisposable {
        private readonly int _worldId;
        private SparseArray<EntityData> _entities;
        private SparseArray<EntityData> _pool;
        private bool _isDisposed;
        public ref SparseArray<EntityData> Entities => ref _entities;
        
        public EntityStorage(int worldId, uint capacity) {
            _worldId = worldId;
            _entities = new SparseArray<EntityData>(capacity);
            _pool = new SparseArray<EntityData>(capacity);
            for (int i = _pool.Length - 1; i >= 0; i--) {
                var data = new EntityData(new Entity(i, -1, _worldId), Options.COMPONENT_ENTITY_CAPACITY);
                _pool.Add(i, data);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(Entity entity) {
            return _entities.Has(entity.Id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(int id) {
            return _entities.Has(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref EntityData Get(Entity entity) {
            return ref _entities.Get(entity.Id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref EntityData Get(int id) {
            return ref _entities.Get(id);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref EntityData GetFreeEntity() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(EntityStorage));
            }

            TryIncreasePool();
            var entity = _pool.GetLast();
            entity.IncreaseGen();
            _pool.Remove(entity.Id);
            _entities.Add(entity.Id, entity);
            return ref _entities.Get(entity.Id);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref EntityData GetFreeEntity(int id, int gen) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(EntityStorage));
            }

            TryIncreasePool(id);
            
            var entity = _pool.Get(id);
            entity.IncreaseGen(gen);
            _pool.Remove(entity.Id);
            _entities.Add(entity.Id, entity);
            return ref _entities.Get(entity.Id);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Return(EntityData entity) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(EntityStorage));
            }
            entity.ClearComponents();
            _entities.Remove(entity.Id);
            _pool.Add(entity.Id, entity);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(EntityStorage));
            }

            foreach (ref var entity in _entities) {
                _pool.Add(entity.Id, entity);
            }
            _entities.Clear();
            foreach (ref var entity in _pool) {
                entity.Clear();
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void TryIncreasePool() {
            if (_pool.Count > 0) {
                return;
            }

            int length = _pool.Length;
            int newLength = length * 2 - 1;
            for (int i = newLength; i >= length; i--) {
                var data = new EntityData(new Entity(i, -1, _worldId), Options.COMPONENT_ENTITY_CAPACITY);
                _pool.Add(i, data);
            }
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void TryIncreasePool(int minCount) {
            if (_pool.Length > minCount) {
                return;
            }
            
            int length = _pool.Length;
            int newLength = length * 2;
            while (newLength <= minCount) {
                newLength *= 2;
            }
            
            for (int i = newLength - 1; i >= length; i--) {
                var data = new EntityData(new Entity(i, -1, _worldId), Options.COMPONENT_ENTITY_CAPACITY);
                _pool.Add(i, data);
            }
        }

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            
            _isDisposed = true;
            foreach (ref var entity in _entities) {
                entity.Dispose();
            }
            
            foreach (ref var entity in _pool) {
                entity.Dispose();
            }
            _entities.Dispose();
            _pool.Dispose();
            _entities = default;
            _pool = default;
        }
    }   
}