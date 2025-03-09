using System.Threading.Tasks;

namespace Sw1f1.Ecs {
    public abstract class FilterThreadJob {
        public void Execute(Filter filter) {
            if (!filter.IsConcurrent) {
                ExecuteDefault(filter);
            } else {
                ExecuteParallel(filter);   
            }
        }

        private void ExecuteDefault(Filter filter) {
            foreach (var e in filter) {
                ExecuteInternal(e);
            }
        }
        
        private unsafe void ExecuteParallel(Filter filter) {
            filter.Update();
            Parallel.For(0, filter.Cache.Count, i => {
                ExecuteInternal(filter.Cache.DenseItems[i].Value);
            });
        }

        protected abstract void ExecuteInternal(Entity entity);
    }
}