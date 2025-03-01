namespace Sw1f1.Ecs {
    public interface IWorld : ISparseItem, IConcurrentSupport {
        int Id { get; }
        internal bool IsAlive { get; }
        internal SparseArray<EntityData> Entities { get; }
        internal SparseArray<AbstractComponentStorage> Components { get; }
        
        ref Entity CreateEntity<T>() where T : struct, IComponent;
        internal bool EntityIsAlive(Entity entity);
        internal void DestroyEntity(Entity entity);
        ref Entity CopyEntity(Entity entity);

        internal void AddComponent<T>(Entity entity, ref T component) where T : struct, IComponent;
        internal bool HasComponent<T>(Entity entity) where T : struct, IComponent;
        internal ref T GetComponent<T>(Entity entity) where T : struct, IComponent;
        internal ref T SetComponent<T>(Entity entity) where T : struct, IComponent;
        internal void RemoveComponent<T>(Entity entity) where T : struct, IComponent;

        internal AbstractComponentStorage GetComponentStorage(int componentId);
        internal bool HasComponentStorage(int componentId);
        
        Filter GetFilter(FilterMask mask);
        void Clear();
        internal void Destroy();
    }   
}