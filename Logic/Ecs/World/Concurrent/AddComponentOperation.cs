namespace Sw1f1.Ecs {
    internal struct AddComponentOperation<T> : IConcurrentOperation where T : struct, IComponent {
        public Entity Entity;
        public T Component;

        public AddComponentOperation(Entity entity, T component) {
            Entity = entity;
            Component = component;
        }

        public void Execute(IWorld world) {
            world.AddComponent(Entity, ref Component);
        }
    }   
}