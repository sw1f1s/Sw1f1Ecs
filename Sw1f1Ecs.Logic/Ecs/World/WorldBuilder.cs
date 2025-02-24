using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
    public static class WorldBuilder {
        private static SparseArray<World> Worlds = new(Options.WORLD_CAPACITY);

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static World Build() {
            var world = new World(Worlds.Count);
            Worlds.Add(world);
            return world;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static World GetWorld(int worldId) {
            return Worlds.Get(worldId);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool AliveWorld(int worldId) {
            return Worlds.Has(worldId);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void Destroy(World world) {
            if (Worlds.Remove(world.Id)) {
                world.Destroy();
            }
        }
    }
}