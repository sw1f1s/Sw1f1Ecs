using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Sw1f1.Ecs.Tests {
    [TestFixture]
    public class WorldTestSnapshot {
        private const int DEFAULT_INT = 10;
        private const bool DEFAULT_BOOL = true;
        
        [Test]
        public void Run_WriteAndReadSnapshot() {
            const int entityCount = 1000;
            
            var componentFactory = new ComponentSnapshotFactory()
                .Register<DefaultComponentPacker<IsTestEntity>>()
                .Register<DefaultComponentPacker<Component1>>()
                .Register<DefaultComponentPacker<Component2>>();
            
            var snapshotWriter = new SnapshotWriter(componentFactory);
            var snapshotReader = new SnapshotReader(componentFactory);
            var world1 = WorldBuilder.Build();
            CreateEntities(entityCount, world1);

            var snapshot = snapshotWriter.Write(world1);
            Assert.That(snapshot.Entities.Length, Is.EqualTo(entityCount));
            foreach (var entity in snapshot.Entities) {
                Assert.That(entity.Components.Length, Is.EqualTo(3));
                Assert.That(entity.Components[0].TypeId, Is.EqualTo(TypeIdUtility.GetTypeId<IsTestEntity>()));
                Assert.That(entity.Components[1].TypeId, Is.EqualTo(TypeIdUtility.GetTypeId<Component1>()));
                Assert.That(entity.Components[2].TypeId, Is.EqualTo(TypeIdUtility.GetTypeId<Component2>()));
            }
            
            var world2 = WorldBuilder.Build();
            var filter1 = world2.GetFilter(new FilterMask<IsTestEntity, Component1, Component2>());
            var filter3 = world2.GetFilter(new FilterMask<Component3>());
            snapshotReader.Read(snapshot, world2);
            Assert.That(filter1.GetCount(), Is.EqualTo(entityCount));
            Assert.That(filter3.GetCount(), Is.EqualTo(0));
            
            foreach (var entity in filter1) {
                Assert.That(entity.Get<Component1>().Value, Is.EqualTo(DEFAULT_INT));
                Assert.That(entity.Get<Component2>().Value, Is.EqualTo(DEFAULT_BOOL));
            }
            
            WorldBuilder.AllDestroy();
        }

        [Test]
        public void Run_NotContainsPackerForComponent() {
            var componentFactory = new ComponentSnapshotFactory()
                .Register<DefaultComponentPacker<IsTestEntity>>()
                .Register<DefaultComponentPacker<Component1>>();
            
            var snapshotWriter = new SnapshotWriter(componentFactory);
            var world = WorldBuilder.Build();
            CreateEntities(1, world);

            Assert.Throws<Exception>(() => {
                var snapshot = snapshotWriter.Write(world);
            });
            
            WorldBuilder.AllDestroy();
        }

        private void CreateEntities(int count, IWorld world) {
            for (int i = 0; i < count; i++) {
                var entity = world.CreateEntity<IsTestEntity>();
                entity.Add(new Component1(DEFAULT_INT));
                entity.Add(new Component2(DEFAULT_BOOL));
                entity.Add(new Component3());
            }
        }
    }
}