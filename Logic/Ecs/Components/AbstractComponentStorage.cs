using System;

namespace Sw1f1.Ecs {
    internal abstract class AbstractComponentStorage : IDisposable {
        public bool IsOneTickComponent { get; protected set; }
        public abstract Type ComponentType { get; }
        public abstract int Id { get; }
        public abstract int Count { get; }
        public abstract IComponent GetGeneralizedComponent(in Entity entity);
        public abstract int[] GetRentedPoolEntities();
        public abstract bool HasComponent(in Entity entity);
        public abstract bool RemoveComponent(in Entity entity);
        public abstract void CopyComponent(in Entity fromEntity, in Entity toEntity);
        internal abstract void Clear();
        public abstract void Dispose();
        
        protected delegate void AutoResetHandler<T>(ref T c);
        protected delegate void AutoCopyHandler<T>(ref T src, ref T dst);
        protected delegate void AutoDestroyHandler<T>(ref T c);
        protected delegate void AutoPoolResetHandler<T>(ref T c, IPoolFactory poolFactory);
        protected delegate void AutoPoolDestroyHandler<T>(ref T c, IPoolFactory poolFactory);
        
        protected bool TryGetInterface<T, TInterface>(ref T defaultInstance, out TInterface obj) {
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
