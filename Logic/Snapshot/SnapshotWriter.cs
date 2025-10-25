using System.Collections.Generic;

namespace Sw1f1.Ecs {
    public sealed class SnapshotWriter {
        private readonly ComponentSnapshotFactory _factory;
        public SnapshotWriter(ComponentSnapshotFactory factory) {
            _factory = factory;
        }

        public WorldSnapshot Write(IWorld world) {
            var entities = GetEntities(world);
            return new WorldSnapshot(world.Id, entities);
        }
        
        public EntitySnapshot Write(in Entity entity) {
            var world = WorldBuilder.GetWorld(entity.WorldId);
            var entityData = world.Entities.Get(entity.Id);
            var components = GetComponents(in entityData, world);
            return new EntitySnapshot(entity.Id, entity.Gen, components);
        }
        
        public ComponentSnapshot Write<TComponent>(in Entity entity) where TComponent : struct, IComponent {
            var world = WorldBuilder.GetWorld(entity.WorldId);
            return Write<TComponent>(in entity, world);
        }

        public ComponentSnapshot Write<TComponent>(in Entity entity, IWorld world) where TComponent : struct, IComponent {
            var componentStorage = world.GetComponentStorage<TComponent>();
            return componentStorage.GetComponentSnapshot(_factory, in entity);
        }
        
        public ComponentSnapshot Write<TComponent>(ref TComponent component) where TComponent : struct, IComponent {
            return _factory.GetSnapshot(ref component);
        }

        private EntitySnapshot[] GetEntities(IWorld world) {
            var entities = new List<EntitySnapshot>();;
            foreach (var entityData in world.Entities) {
                var entity = entityData.GetEntity();
                var components = GetComponents(in entityData, world);
                if (components.Length == 0) {
                    continue;
                }
                
                entities.Add(new EntitySnapshot(entity.Id, entity.Gen, components));
            }
            return entities.ToArray();
        }

        private ComponentSnapshot[] GetComponents(in EntityData entityData, IWorld world) {
            var entity = entityData.GetEntity();
            var components = new List<ComponentSnapshot>();
            foreach (var componentId in entityData.Components) {
                var componentStorage = world.GetComponentStorage(componentId);
                if (!componentStorage.IsSerializableComponent) {
                    continue;
                }
                
                var component = componentStorage.GetComponentSnapshot(_factory, in entity);
                components.Add(component);
            }
            
            return components.ToArray();
        }
    }
}