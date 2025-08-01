using System;
using System.Collections.Generic;

namespace Sw1f1.Ecs {
    internal class SystemContainer : IDisposable {
#if DEBUG
        public event Action<ISystem> OnAddSystem;
        public event Action<ISystem> OnStartSystemExecute;
        public event Action<ISystem> OnEndSystemExecute;
#endif
        
        private readonly List<ISystem> _allSystems;
        private readonly List<IInitSystem> _initSystems;
        private readonly List<IUpdateSystem> _updateSystems;
        private readonly List<ISystem> _cachedSystems;
        private bool _isDisposed;

        internal SystemContainer(int capacity = Options.SYSTEMS_CAPACITY) {
            _allSystems = new List<ISystem>(capacity);
            _initSystems = new List<IInitSystem>(capacity);
            _updateSystems = new List<IUpdateSystem>(capacity);
            _cachedSystems = new List<ISystem>(capacity);
        }

        internal void AddSystem(ISystem system) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SystemContainer));
            }
            
            _allSystems.Add(system);
            
            if (system is IInitSystem initSystem) {
                _initSystems.Add(initSystem);
            }
            
            if (system is IUpdateSystem updateSystem) {
                _updateSystems.Add(updateSystem);
            }
            
#if DEBUG
            if (system is not InternalGroupSystem) {
                OnAddSystem?.Invoke(system);   
            }
#endif
        }

        internal void Init() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SystemContainer));
            }
            
            for (int i = 0; i < _initSystems.Count; i++) {
                _initSystems[i].Init();
            }
        }

        internal void Update() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SystemContainer));
            }

            for (int i = 0; i < _updateSystems.Count; i++) {
#if DEBUG
                if (_updateSystems[i] is not InternalGroupSystem) {
                    OnStartSystemExecute?.Invoke(_updateSystems[i]);
                }
#endif
                _updateSystems[i].Update();
#if DEBUG
                if (_updateSystems[i] is not InternalGroupSystem) {
                    OnEndSystemExecute?.Invoke(_updateSystems[i]);
                }
#endif
            }
        }

        internal IReadOnlyList<ISystem> GetAllSystems() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(SystemContainer));
            }
            
            _cachedSystems.Clear();
            foreach (var system in _allSystems) {
                if (system is InternalGroupSystem internalGroupSystem) {
                    _cachedSystems.AddRange(internalGroupSystem.GetAllSystems());
                } else {
                    _cachedSystems.Add(system);
                }
            }
            
            return _cachedSystems;
        }

        public void Dispose() {
            _isDisposed = true;
            _allSystems.Clear();
            _initSystems.Clear();
            _updateSystems.Clear();
            _cachedSystems.Clear();
        }
    }   
}