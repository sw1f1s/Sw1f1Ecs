using System;
using Sw1f1.Ecs.Collections;

namespace Sw1f1.Ecs {
    public interface IWorld : IDisposable {
        int Id { get; }
        internal bool IsAlive { get; }
        internal ref SparseArray<EntityData> Entities { get; }
        
        Entity CreateEntity<T>() where T : struct, IComponent;
        internal bool EntityIsAlive(Entity entity);
        internal void DestroyEntity(Entity entity);
        Entity CopyEntity(Entity entity);

        internal bool HasComponent<T>(Entity entity) where T : struct, IComponent;
        internal ref T GetComponent<T>(Entity entity) where T : struct, IComponent;
        
        internal void AddComponent<T>(Entity entity, ref T component) where T : struct, IComponent;
        internal ref T SetComponent<T>(Entity entity) where T : struct, IComponent;
        internal void RemoveComponent<T>(Entity entity) where T : struct, IComponent;

        internal AbstractComponentStorage GetComponentStorage(int componentId);
        internal bool HasComponentStorage(int componentId);
        
        Filter GetFilter(FilterMask mask);
        internal void UpdateFilters();
        internal void Lock();
        internal void Unlock();
        void Clear();
    }   
}