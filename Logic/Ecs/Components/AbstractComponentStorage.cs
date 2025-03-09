using System;

namespace Sw1f1.Ecs {
    internal abstract class AbstractComponentStorage : IConcurrentSupport, IDisposable {
        public abstract Type ComponentType { get; }
        public abstract int Id { get; }
        public abstract bool IsConcurrent { get; }
        public abstract IComponent GetGeneralizedComponent(Entity entity);
        public abstract bool HasComponent(Entity entity);
        public abstract void RemoveComponent(Entity entity);
        public abstract void CopyComponent(Entity fromEntity, Entity toEntity);
        internal abstract void Clear();
        public abstract void Dispose();
        
        protected delegate void AutoResetHandler<T>(ref T c);

        protected delegate void AutoCopyHandler<T>(ref T src, ref T dst);
        
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
