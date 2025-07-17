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
    internal class ComponentsStorage : IComponentsStorage, IDisposable {
        private readonly List<int> _oneTickComponents = new List<int>();
        private SparseArray<AbstractComponentStorage> _components = new SparseArray<AbstractComponentStorage>(Options.COMPONENT_CAPACITY);
        private bool _isDisposed;

        public IReadOnlyList<int> OneTickStorages => _oneTickComponents;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public AbstractComponentStorage Get(int componentId) {
            return _components.Get(componentId);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal bool Has(int componentId) {
            return _components.Has(componentId);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal ComponentStorage<T> GetComponentStorage<T>() where T : struct, IComponent {
            int componentId = ComponentStorageIndex<T>.StaticId;
            if (!_components.Has(componentId)) {
                var storage = new ComponentStorage<T>();
                _components.Add(componentId, storage);
                if (storage.IsOneTickComponent) {
                    _oneTickComponents.Add(componentId);   
                }
            }
            
            return Unsafe.As<ComponentStorage<T>>(_components.Get(componentId));
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal AbstractComponentStorage GetComponentStorage(int componentId) {
            return _components.Get(componentId);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void Clear() {
            foreach (var component in _components) {
                component.Clear();
            }
            _oneTickComponents.Clear();
        }

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            
            _isDisposed = true;
            _components.Dispose();
            _oneTickComponents.Clear();
        }
    }
}