using System;
using System.Collections.Generic;
using Sw1f1.Ecs.Collections;

namespace Sw1f1.Ecs {
    public interface IWorld : IDisposable {
        int Id { get; }
        internal bool IsAlive { get; }
        internal IComponentsStorage ComponentsStorage { get; }
        internal ref SparseArray<EntityData> Entities { get; }
#if DEBUG
        event Action<IWorld, Entity> OnCreateEntity;
        event Action<IWorld, Entity> OnCopyEntity;
        event Action<IWorld, Entity> OnDestroyEntity;
        event Action<IWorld, Entity, Type> OnAddComponent;
        event Action<IWorld, Entity, Type> OnRemoveComponent;
        IEnumerable<Entity> AllEntities();
#endif
        
        Entity CreateEntity<T>() where T : struct, IComponent;
        internal bool EntityIsAlive(in Entity entity);
        internal void DestroyEntity(in Entity entity);
        Entity CopyEntity(in Entity entity);

        internal bool HasComponent<T>(in Entity entity) where T : struct, IComponent;
        internal ref T GetComponent<T>(in Entity entity) where T : struct, IComponent;
        
        internal void AddComponent<T>(in Entity entity, ref T component) where T : struct, IComponent;
        internal ref T SetComponent<T>(in Entity entity) where T : struct, IComponent;
        internal void RemoveComponent<T>(in Entity entity) where T : struct, IComponent;
        internal void RemoveComponent(in Entity entity, int componentIdx);

        internal AbstractComponentStorage GetComponentStorage(int componentId);
        internal bool HasComponentStorage(int componentId);
        
        Filter GetFilter(FilterMask mask);
        internal void UpdateFilters();
        internal void Lock();
        internal void Unlock();
        void Clear();
    }   
}