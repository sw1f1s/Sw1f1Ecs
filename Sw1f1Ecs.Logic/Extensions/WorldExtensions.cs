namespace Sw1f1.Ecs {
    public static class WorldExtensions {
        public static void Destroy(this IWorld world) {
            WorldBuilder.Destroy(world);
        }
        
        public static bool IsAlive(this IWorld world) {
            return world.IsAlive && WorldBuilder.AliveWorld(world.Id);
        }
    }   
}