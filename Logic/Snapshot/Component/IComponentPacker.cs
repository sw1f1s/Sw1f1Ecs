using System;

namespace Sw1f1.Ecs {
    public interface IComponentPacker {
        ulong Uid { get; set; }
        Type ComponentType { get; }
        void Deserialize(in Entity entity, in ComponentSnapshot snapshot, IWorld world);
    }

    public interface IComponentPacker<TComponent> : IComponentPacker where TComponent : struct, IComponent {
        ComponentSnapshot Serialize(ref TComponent component);
    }
}