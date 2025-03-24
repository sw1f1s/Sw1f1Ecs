using System.Threading.Tasks;

namespace Sw1f1.Ecs {
    public abstract class FilterThreadJob {
        public void Execute(Filter filter) {
            filter.World.Lock();
            filter.Update();
            Parallel.For(0, filter.Cache.Count, i => {
                ExecuteInternal(filter.Cache.DenseItems[i].Value);
            });
            filter.World.Unlock();
        }

        protected abstract void ExecuteInternal(Entity entity);
    }
}