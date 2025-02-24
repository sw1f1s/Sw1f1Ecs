using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
    internal abstract class ComponentStorage : ISparseItem {
        public abstract int Id { get; }
        public abstract bool HasComponent(Entity entity);
        public abstract void RemoveComponent(Entity entity);
        public abstract void CopyComponent(Entity fromEntity, Entity toEntity);
        public abstract void Clear();
        public abstract void DeepClear();
    }
    
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    //TODO: Доработать многопоточное добалвение и удаление компонентов
    internal sealed class ComponentStorage<T> : ComponentStorage where T : struct, IComponent {
        private volatile SparseArray<EntityID> _components;
        private volatile T[] _componentData;
        private readonly AutoResetHandler _autoResetHandler;
        private readonly AutoCopyHandler _autoCopyHandler;
        private readonly T _defaultInstance = default;
        private readonly object _resizeLock = new object();
        
        private delegate void AutoResetHandler (ref T c);
        private delegate void AutoCopyHandler (ref T src, ref T dst);
        private struct EntityID : ISparseItem {
            public int Id { get; private set; }

            public EntityID(int id) {
                Id = id;
            }
        }

        public override int Id => ComponentStorageIndex<T>.StaticId;

        internal ComponentStorage(int capacity) {
            _components = new SparseArray<EntityID>(Options.ENTITY_CAPACITY);
            _componentData = new T[capacity];

            if (TryGetInterface(out IAutoCopyComponent<T> autoCopy)) {
                _autoCopyHandler = autoCopy.Copy;
            }
            
            if (TryGetInterface(out IAutoResetComponent<T> autoReset)) {
                _autoResetHandler = autoReset.Reset;
            }
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void AddComponent(Entity entity, ref T component) {
            if (HasComponent(entity)) {
                throw new Exception($"{entity} already contains {typeof(T).Name}");
            }

            _components.Add(new EntityID(entity.Id));
            int index = _components.GetSparseIndex(entity.Id);
            TryResize(index);
            _componentData[index] = component;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent(Entity entity) {
            if (!HasComponent(entity)) {
                throw new Exception($"{entity} not contains {typeof(T).Name}");
            }
            
            int index = _components.GetSparseIndex(entity.Id);
            return ref _componentData[index];
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T GetOrSetComponent(Entity entity) {
            if (!HasComponent(entity)) {
                var newComponent = new T();
                _autoResetHandler?.Invoke(ref newComponent);
                AddComponent(entity, ref newComponent);
            }
            
            int index = _components.GetSparseIndex(entity.Id);
            return ref _componentData[index];
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override bool HasComponent(Entity entity) {
            return _components.Has(entity.Id);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void RemoveComponent(Entity entity) {
            if (!HasComponent(entity)) {
                throw new Exception($"{entity} not contains {typeof(T).Name}");
            }
            
            _components.Remove(entity.Id);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void CopyComponent(Entity fromEntity, Entity toEntity) {
            if (!HasComponent(fromEntity) || HasComponent(toEntity)) {
                return;
            }
            
            
            var newComponent = new T();
            _autoCopyHandler?.Invoke(ref GetComponent(fromEntity), ref newComponent);
            AddComponent(toEntity, ref newComponent);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void Clear() {
            _components.Clear();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void DeepClear() {
            _componentData = new T[_componentData.Length];
            _components.DeepClear();
        }

        private void TryResize(int id) {
            while (id >= _componentData.Length) {
                if (!Monitor.IsEntered(_resizeLock)) {
                    lock (_resizeLock) {
                        if (id >= _componentData.Length) {
                            Resize();
                        }
                    }   
                } else {
                    Resize();
                }
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void Resize() {
            Array.Resize(ref _componentData, _componentData.Length * 2); 
        }
        
        private bool TryGetInterface<TInterface>(out TInterface obj) {
            obj = default;

            if (typeof(TInterface).IsAssignableFrom(typeof(T))) {
                if (_defaultInstance is TInterface instance) {
                    obj = instance;
                    return true;
                }
            }

            return false;
        }
    }   
}