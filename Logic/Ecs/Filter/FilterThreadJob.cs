using System.Threading.Tasks;

namespace Sw1f1.Ecs {
    public abstract class FilterThreadJob {
        public void Execute(Filter filter) {
            filter.World.Lock();
            filter.UpdateIfDirty();
            Parallel.For(0, filter.Entities.Count, i => {
                ExecuteInternal(filter.Entities.DenseItems[i].Value);
            });
            filter.World.Unlock();
        }

        protected abstract void ExecuteInternal(Entity entity);
    }
}