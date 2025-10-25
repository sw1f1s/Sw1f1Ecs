using System.Collections.Generic;
using Sw1f1.Ecs.Collections;

namespace Sw1f1.Ecs {
    internal interface IComponentsStorage {
        IReadOnlyList<int> OneTickStorages { get; }
        IComponentStorage Get(int componentId);
    }
}