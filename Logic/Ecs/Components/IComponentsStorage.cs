using System.Collections.Generic;

namespace Sw1f1.Ecs {
    internal interface IComponentsStorage {
        IReadOnlyList<int> OneTickStorages { get; }
        AbstractComponentStorage Get(int componentId);
    }
}