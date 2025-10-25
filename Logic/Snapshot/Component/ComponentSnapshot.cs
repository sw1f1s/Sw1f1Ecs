namespace Sw1f1.Ecs {
    public readonly struct ComponentSnapshot {
        public readonly ulong TypeId;
        public readonly int Length;
        public readonly byte[] Buffer;

        public ComponentSnapshot(ulong typeId, int length, byte[] buffer) {
            TypeId = typeId;
            Length = length;
            Buffer = buffer;
        }
    }
}