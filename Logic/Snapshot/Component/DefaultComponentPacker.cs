using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sw1f1.Ecs {
    public class DefaultComponentPacker<TComponent> : IComponentPacker<TComponent> where TComponent : struct, IComponent {
        public ulong Uid { get; set; }
        public Type ComponentType => typeof(TComponent);

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public virtual ComponentSnapshot Serialize(ref TComponent component) {
            int size = Unsafe.SizeOf<TComponent>();
            var buffer = new byte[size];
            MemoryMarshal.Write(buffer.AsSpan(0, size), ref component);
            return new ComponentSnapshot(Uid, size, buffer);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public virtual void Deserialize(in Entity entity, in ComponentSnapshot snapshot, IWorld world) {
            var component = Deserialize(in snapshot);
            world.ReplaceComponent(in entity, in component);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public virtual TComponent Deserialize(in ComponentSnapshot snapshot) {
            return MemoryMarshal.Read<TComponent>(snapshot.Buffer.AsSpan(0, snapshot.Length));
        }
    }
}