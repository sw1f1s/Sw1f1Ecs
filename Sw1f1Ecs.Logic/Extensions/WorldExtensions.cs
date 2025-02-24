namespace Sw1f1.Ecs {
    public static class WorldExtensions {
        public static void Destroy(this World world) {
            WorldBuilder.Destroy(world);
        }
    }   
}