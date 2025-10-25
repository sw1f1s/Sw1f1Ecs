using System;

namespace Sw1f1.Ecs {
    internal interface IComponentStorage : IDisposable {
        bool IsSerializableComponent { get; }
        bool IsOneTickComponent { get; }
        Type ComponentType { get; }
        int Id { get; }
        int Count { get; }
        ComponentSnapshot GetComponentSnapshot(ComponentSnapshotFactory factory,  in Entity entity);
        IComponent GetGeneralizedComponent(in Entity entity);
        int[] GetRentedPoolEntities();
        bool HasComponent(in Entity entity);
        bool RemoveComponent(in Entity entity);
        void CopyComponent(in Entity fromEntity, in Entity toEntity);
        void Clear();
        
        delegate void AutoResetHandler<T>(ref T c);
        delegate void AutoCopyHandler<T>(ref T src, ref T dst);
        delegate void AutoDestroyHandler<T>(ref T c);
        delegate void AutoPoolResetHandler<T>(ref T c, IPoolFactory poolFactory);
        delegate void AutoPoolDestroyHandler<T>(ref T c, IPoolFactory poolFactory);
        
        protected static bool TryGetInterface<T, TInterface>(ref T defaultInstance, out TInterface obj) {
            obj = default;
            if (typeof(TInterface).IsAssignableFrom(typeof(T))) {
                if (defaultInstance is TInterface instance) {
                    obj = instance;
                    return true;
                }
            }
            return false;
        }
    }
}
