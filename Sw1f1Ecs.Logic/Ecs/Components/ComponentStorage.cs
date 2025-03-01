using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal sealed class ComponentStorage<T> : AbstractComponentStorage where T : struct, IComponent {
        private readonly SparseArray<EntityID> _components;
        private T[] _componentData;
        private readonly AutoResetHandler<T> _autoResetHandler;
        private readonly AutoCopyHandler<T> _autoCopyHandler;
        private readonly T _defaultInstance = default;
        
        private bool _isDisposed;
        
        public override bool IsConcurrent => false;

        public override int Id => ComponentStorageIndex<T>.StaticId;

        internal ComponentStorage(int capacity) {
            _components = new SparseArray<EntityID>(Options.ENTITY_CAPACITY);
            _componentData = new T[capacity];

            if (TryGetInterface(ref _defaultInstance, out IAutoCopyComponent<T> autoCopy)) {
                _autoCopyHandler = autoCopy.Copy;
            }

            if (TryGetInterface(ref _defaultInstance, out IAutoResetComponent<T> autoReset)) {
                _autoResetHandler = autoReset.Reset;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent(Entity entity, ref T component) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ComponentStorage<T>));
            }
            
            if (HasComponentInternal(entity)) {
                throw new Exception($"{entity} already contains {typeof(T).Name}");
            }
            AddComponentInternal(entity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IComponent GetGeneralizedComponent(Entity entity) {
            return GetComponent(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent(Entity entity) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ComponentStorage<T>));
            }
            
            if (!HasComponentInternal(entity)) {
                throw new Exception($"{entity} not contains {typeof(T).Name}");
            }

            return ref GetComponentInternal(entity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T SetComponent(Entity entity) {
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
        public override bool HasComponent(Entity entity) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ComponentStorage<T>));
            }
            
            return HasComponentInternal(entity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void RemoveComponent(Entity entity) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ComponentStorage<T>));
            }
            
            if (!HasComponentInternal(entity)) {
                throw new Exception($"{entity} not contains {typeof(T).Name}");
            }
            _components.Remove(entity.Id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CopyComponent(Entity fromEntity, Entity toEntity) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ComponentStorage<T>));
            }
            
            if (!HasComponentInternal(fromEntity) || HasComponentInternal(toEntity)) {
                return;
            }

            T srcComponent = GetComponent(fromEntity);
            var newComponent = new T();
            
            _autoCopyHandler?.Invoke(ref srcComponent, ref newComponent);
            AddComponentInternal(toEntity, newComponent);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddComponentInternal(Entity entity, T component) {
            _components.Add(new EntityID(entity.Id));
            int index = _components.GetSparseIndex(entity.Id);
            TryResize(index);
            _componentData[index] = component;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private bool HasComponentInternal(Entity entity) {
            return _components.Has(entity.Id);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private ref T GetComponentInternal(Entity entity) {
            int index = _components.GetSparseIndex(entity.Id);
            return ref _componentData[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Clear() {
            _components.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryResize(int id) {
            while (id >= _componentData.Length) {
                Resize();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize() {
            Array.Resize(ref _componentData, _componentData.Length * 2);
        }

        public override void Dispose() {
            _isDisposed = true;
            _componentData = new T[_componentData.Length];
            _components.Dispose();
            _components.Dispose();
        }
    }   
}