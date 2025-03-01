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
        
        private void ExecuteParallel(Filter filter) {
            filter.Update();
            filter.Cache.Lock();
            Parallel.For(0, filter.Cache.Count, i => {
                ExecuteInternal(filter.Cache.DenseItems[i]);
            });
            filter.Cache.Unlock();
        }

        protected abstract void ExecuteInternal(Entity entity);
    }
}