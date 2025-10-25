using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
    public sealed class ComponentSnapshotFactory {
        private readonly Dictionary<ulong, Type> _uidMapper = new Dictionary<ulong, Type>();
        private readonly Dictionary<Type, IComponentPacker> _packers = new Dictionary<Type, IComponentPacker>();
        
        public ComponentSnapshotFactory Register<TComponent, TPacker>() where TComponent : struct, ISerializableComponent where TPacker : IComponentPacker<TComponent> {
            var packer = (TPacker)Activator.CreateInstance(typeof(TPacker));
            packer.Uid = GetTypeId<TComponent>();
            _packers.Add(typeof(TComponent), (TPacker)Activator.CreateInstance(typeof(TPacker)));
            return this;
        }
        
        public ComponentSnapshotFactory Register<TComponent, TPacker>(TPacker packer) where TComponent : struct, ISerializableComponent where TPacker : IComponentPacker<TComponent> {
            packer.Uid = GetTypeId<TComponent>();
            _packers.Add(typeof(TComponent), packer);
            return this;
        }
        
        public ComponentSnapshotFactory Register<TPacker>(TPacker packer) where TPacker : IComponentPacker {
            packer.Uid = GetTypeId(packer.ComponentType);
            _packers.Add(packer.ComponentType, packer);
            return this;
        }
        
        public ComponentSnapshotFactory Register<TPacker>() where TPacker : IComponentPacker {
            var packer = (TPacker)Activator.CreateInstance(typeof(TPacker));
            packer.Uid = GetTypeId(packer.ComponentType);
            _packers.Add(packer.ComponentType, packer);
            return this;
        }

        internal ComponentSnapshot GetSnapshot<TComponent>(ref TComponent component) where TComponent : struct, IComponent {
            if (_packers.TryGetValue(component.GetType(), out var packer)) {
                return Unsafe.As<IComponentPacker<TComponent>>(packer).Serialize(ref component);
            }
            
            throw new Exception($"Component {component.GetType()} is not registered");
        }
        
        internal void ReplaceComponent(in Entity entity, in ComponentSnapshot snapshot, IWorld world) {
            if (_uidMapper.TryGetValue(snapshot.TypeId, out var type)) {
                if (_packers.TryGetValue(type, out var packer)) {
                    packer.Deserialize(in entity, in snapshot, world);
                    return;
                }
            }
            
            throw new Exception($"Component {snapshot.TypeId} is not registered");
        }

        private ulong GetTypeId<TComponent>() {
            var uid = TypeIdUtility.GetTypeId<TComponent>();
            _uidMapper.TryAdd(uid, typeof(TComponent));
            return uid;
        }
        
        private ulong GetTypeId(Type componentType) {
            var uid = TypeIdUtility.GetTypeId(componentType);
            _uidMapper.TryAdd(uid, componentType);
            return uid;
        }
    }
}