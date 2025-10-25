namespace Sw1f1.Ecs {
    public sealed class SnapshotReader {
        private readonly ComponentSnapshotFactory _factory;
        public SnapshotReader(ComponentSnapshotFactory factory) {
            _factory = factory;
        }
        
        public IWorld Read(in WorldSnapshot snapshot) {
            var world = WorldBuilder.Build();
            Read(snapshot, world);
            return world;
        }
        
        public void Read(in WorldSnapshot snapshot, IWorld world) {
            foreach (var entitySnapshot in snapshot.Entities) {
                Read(in entitySnapshot, world);
            }
        }
        
        public void Read(in EntitySnapshot snapshot, IWorld world) {
            var entity = world.CreateEntity(snapshot.Id, snapshot.Gen);
            Read(in entity, in snapshot, world);
        }
        
        public void Read(in Entity entity, in EntitySnapshot snapshot, IWorld world) {
            foreach (var componentSnapshot in snapshot.Components) {
                _factory.ReplaceComponent(in entity, in componentSnapshot, world);
            }
        }
    }
}