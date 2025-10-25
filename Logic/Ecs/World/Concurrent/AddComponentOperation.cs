namespace Sw1f1.Ecs {
    internal readonly struct AddComponentOperation<T> : IConcurrentOperation where T : struct, IComponent {
        public readonly Entity Entity;
        public readonly T Component;

        public AddComponentOperation(Entity entity, in T component) {
            Entity = entity;
            Component = component;
        }

        public void Execute(IWorld world) {
            world.AddComponent(Entity, in Component);
        }
    }   
}