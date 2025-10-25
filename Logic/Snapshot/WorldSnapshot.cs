namespace Sw1f1.Ecs {
    public readonly struct WorldSnapshot {
        public readonly int Id;
        public readonly EntitySnapshot[] Entities;

        public WorldSnapshot(int id, EntitySnapshot[] entities) {
            Id = id;
            Entities = entities;
        }
    }
}