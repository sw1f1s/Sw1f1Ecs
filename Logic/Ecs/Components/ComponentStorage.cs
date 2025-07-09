using System;
using System.Runtime.CompilerServices;
using Sw1f1.Ecs.Collections;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal sealed class ComponentStorage<T> : AbstractComponentStorage where T : struct, IComponent {
        private readonly T _defaultInstance = default;
        private AutoResetHandler<T> _autoResetHandler;
        private AutoCopyHandler<T> _autoCopyHandler;
        private AutoDestroyHandler<T> _autoDestroyHandler;
        private SparseArray<T> _components;
        private bool _isDisposed;

        public override Type ComponentType => typeof(T);
        public override int Id => ComponentStorageIndex<T>.StaticId;

        internal ComponentStorage() {
            _components = new SparseArray<T>(Options.ENTITY_CAPACITY);
            if (TryGetInterface(ref _defaultInstance, out IAutoCopyComponent<T> autoCopy)) {
                _autoCopyHandler = autoCopy.Copy;
            }

            if (TryGetInterface(ref _defaultInstance, out IAutoResetComponent<T> autoReset)) {
                _autoResetHandler = autoReset.Reset;
            }
            
            if (TryGetInterface(ref _defaultInstance, out IAutoDestroyComponent<T> autoDestroy)) {
                _autoDestroyHandler = autoDestroy.Destroy;
            }
            
            IsOneTickComponent = _defaultInstance is IOneTickComponent;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent(in Entity entity, ref T component) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ComponentStorage<T>));
            }
            
            if (HasComponentInternal(entity)) {
                throw new Exception($"{entity} already contains {typeof(T).Name}");
            }
            AddComponentInternal(entity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IComponent GetGeneralizedComponent(in Entity entity) {
            return GetComponent(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int[] GetEntities() {
            var entities = new int[_components.Count];
            for (int i = 0; i < _components.Count; i++) {
                entities[i] = (int)_components.DenseItems[i].Index;
            }
            return entities;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent(in Entity entity) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ComponentStorage<T>));
            }
            
            if (!HasComponentInternal(entity)) {
                throw new Exception($"{entity} not contains {typeof(T).Name}");
            }

            return ref GetComponentInternal(entity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T SetComponent(in Entity entity) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ComponentStorage<T>));
            }
            
            if (HasComponentInternal(entity)) {
                throw new Exception($"{entity} already contains {typeof(T).Name}");
            }
                
            var newComponent = new T();
            _autoResetHandler?.Invoke(ref newComponent);
            AddComponentInternal(entity, newComponent);
            return ref GetComponentInternal(entity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool HasComponent(in Entity entity) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ComponentStorage<T>));
            }
            
            return HasComponentInternal(entity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool RemoveComponent(in Entity entity) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ComponentStorage<T>));
            }
            
            if (HasComponentInternal(entity)) {
                _autoDestroyHandler?.Invoke(ref GetComponentInternal(entity));
                _components.Remove(entity.Id);
                return true;
            }
            
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CopyComponent(in Entity fromEntity, in Entity toEntity) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ComponentStorage<T>));
            }
            
            if (!HasComponentInternal(fromEntity) || HasComponentInternal(toEntity)) {
                return;
            }

            T srcComponent = GetComponent(fromEntity);
            var newComponent = srcComponent;
            
            _autoCopyHandler?.Invoke(ref srcComponent, ref newComponent);
            AddComponentInternal(toEntity, newComponent);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddComponentInternal(in Entity entity, T component) {
            _components.Add(entity.Id, component);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private bool HasComponentInternal(in Entity entity) {
            return _components.Has(entity.Id);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private ref T GetComponentInternal(in Entity entity) {
            return ref _components.Get(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Clear() {
            _components.Clear();
        }

        public override void Dispose() {
            if (_isDisposed) {
                return;
            }
            
            _isDisposed = true;
            _autoResetHandler = null;
            _autoCopyHandler = null;
            _autoDestroyHandler = null;
            _components.Dispose();
            _components = default;
        }
    }   
}