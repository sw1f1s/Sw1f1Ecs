using System;
using System.Collections.Generic;

namespace Sw1f1.Ecs {
    public sealed class Systems : ISystems {
        private IWorld _world;
        private readonly SystemContainer _systemContainer;
        private readonly Dictionary<string, InternalGroupSystem> _groupSystems = new Dictionary<string, InternalGroupSystem>(Options.SYSTEMS_CAPACITY);
        private bool _isDisposed;
        
        public IWorld World => _world;
        public IReadOnlyList<ISystem> AllSystems => _systemContainer.GetAllSystems();
        
        public Systems(IWorld world) {
            _world = world;
            _systemContainer = new SystemContainer();
        }

        public ISystems Add(ISystem system) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(Systems));
            }

            if (system is IGroupSystem groupSystem) {
                return Add(CreateGroupSystem(groupSystem));
            }
            
            _systemContainer.AddSystem(system);
            return this;
        }

        public void Init() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(Systems));
            }
            _systemContainer.Init();
        }

        public void Update() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(Systems));
            }
            _systemContainer.Update();
        }

        public void Dispose() {
            _isDisposed = true;
            _world = null;
            _systemContainer.Dispose();
            _groupSystems.Clear();
        }
        
#region Groups
        internal InternalGroupSystem CreateGroupSystem(IGroupSystem system) {
            var g = new InternalGroupSystem(this, system);
            _groupSystems.Add(system.GroupName, g);
            return g;
        }

        public void SetActiveGroup(string groupName, bool value) {
            if (_groupSystems.TryGetValue(groupName, out var group)) {
                group.SetActive(value);
            }
        }

        public bool IsActiveGroup(string groupName) {
            if (_groupSystems.TryGetValue(groupName, out var group)) {
                return group.IsActive;
            }

            return false;
        }
#endregion
    }   
}