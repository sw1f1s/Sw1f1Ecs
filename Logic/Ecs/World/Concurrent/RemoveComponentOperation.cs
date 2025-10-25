namespace Sw1f1.Ecs {
    internal readonly struct RemoveComponentOperation<T> : IConcurrentOperation where T : struct, IComponent {
        public readonly Entity Entity;

        public RemoveComponentOperation(Entity entity) {
            Entity = entity;
        }

        public void Execute(IWorld world) {
            world.RemoveComponent<T>(Entity);
        }
    }   
}