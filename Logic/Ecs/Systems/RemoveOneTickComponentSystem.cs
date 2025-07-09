namespace Sw1f1.Ecs {
    internal class RemoveOneTickComponentSystem : IUpdateSystem {
        private readonly IWorld _world;
        public RemoveOneTickComponentSystem(IWorld world) {
            _world = world;
        }

        void IUpdateSystem.Update() {
            foreach (var componentId in _world.ComponentsStorage.OneTickStorages) {
                var storage = _world.ComponentsStorage.Get(componentId);
                foreach (var entityIdx in storage.GetEntities()) {
                   var entity =  _world.Entities.Get(entityIdx).GetEntity();
                   _world.RemoveComponent(entity, componentId);
                }
            }
        }
    }
}