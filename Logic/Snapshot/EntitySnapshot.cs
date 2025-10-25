namespace Sw1f1.Ecs {
    public readonly struct EntitySnapshot {
        public readonly int Id;
        public readonly int Gen;
        public readonly ComponentSnapshot[] Components;

        public EntitySnapshot(int id, int gen, ComponentSnapshot[] components) {
            Id = id;
            Gen = gen;
            Components = components;
        }
    }
}