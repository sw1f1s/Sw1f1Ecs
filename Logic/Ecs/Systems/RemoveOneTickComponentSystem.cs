using System.Buffers;
using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
    internal class RemoveOneTickComponentSystem : IUpdateSystem {
        private readonly IWorld _world;
        public RemoveOneTickComponentSystem(IWorld world) {
            _world = world;
        }

        void IUpdateSystem.Update() {
            foreach (var componentId in _world.ComponentsStorage.OneTickStorages) {
                var storage = _world.ComponentsStorage.Get(componentId);
                var entities = storage.GetRentedPoolEntities();
                for (int i = storage.Count - 1; i >= 0; i--) {
                    int entityIdx = entities[i];
                    var entity = _world.Entities.Get(entityIdx).GetEntity();
                    _world.RemoveComponent(entity, componentId);
                }
                ArrayPool<int>.Shared.Return(entities);
            }
        }
    }
}