using Sw1f1.Ecs.Collections;

namespace Sw1f1.Ecs {
    public interface IPoolFactory {
        PooledList<T> Rent<T>(int initialCapacity = 4);
    }
}