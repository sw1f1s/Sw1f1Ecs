namespace Sw1f1.Ecs {
    /// <summary>
    /// Работает только на изменение данных в компонентах, добавление и удаление компонентов в большом количестве сущностей может привести к гонке данных
    /// </summary>
    public abstract class FilterThreadJob {
        public void Execute(Filter filter) {
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