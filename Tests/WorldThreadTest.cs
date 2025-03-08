using NUnit.Framework;

namespace Sw1f1.Ecs.Tests {
    [TestFixture]
    public class WorldThreadTest {
        [TestCase(1)]
        [TestCase(100)]
        [TestCase(1000)]
        public void Run_FilterCreateEntityTreadJob(int count) {
            var world = WorldBuilder.Build(true);
            var filter1 = world.GetFilter(new FilterMask<Component1>());
            var filter2 = world.GetFilter(new FilterMask<Component2>());
            var createEntityFilterThread = new CreateEntityFilterThreadJob(world);
            var entities = new Entity[count];
            for (int i = 0; i < count; i++) {
                var entity = world.CreateEntity<IsTestEntity>();
                entity.Add(new Component1(0));
                entities[i] = entity;
            }
            
            createEntityFilterThread.Execute(filter1);
            Assert.That(filter2.GetCount(), Is.EqualTo(count));
            world.Destroy();
        }

        
        [TestCase(1)]
        [TestCase(100)]
        [TestCase(1000)]
        public void Run_FilterIncreaseComponentTreadJob(int count) {
            var world = WorldBuilder.Build(true);
            var filter = world.GetFilter(new FilterMask<Component1>());
            var increaseComponent1FilterTread = new IncreaseComponent1FilterThreadJob();
            var entities = new Entity[count];
            for (int i = 0; i < count; i++) {
                var entity = world.CreateEntity<IsTestEntity>();
                entity.Add(new Component1(0));
                entities[i] = entity;
            }
           
            increaseComponent1FilterTread.Execute(filter);
            for (int i = 0; i < count; i++) {
                Assert.That(entities[i].Get<Component1>().Value, Is.EqualTo(1));
            }
            
            increaseComponent1FilterTread.Execute(filter);
            for (int i = 0; i < count; i++) {
                Assert.That(entities[i].Get<Component1>().Value, Is.EqualTo(2));
            }
            
            world.Destroy();
        }
        
        [TestCase(1)]
        [TestCase(100)]
        [TestCase(1000)]
        public void Run_FilterAddComponentTreadJob(int count) {
            var world = WorldBuilder.Build(true);
            var filter = world.GetFilter(new FilterMask<Component1>());
            var addComponentFilterTread = new AddComponentFilterThreadJob();
            var entities = new Entity[count];
            for (int i = 0; i < count; i++) {
                var entity = world.CreateEntity<IsTestEntity>();
                entity.Add(new Component1(0));
                entities[i] = entity;
            }
           
            addComponentFilterTread.Execute(filter);
            for (int i = 0; i < count; i++) {
                Assert.That(entities[i].Has<Component2>(), Is.True, $"Component2 should exist on entity{entities[i]}");
                Assert.That(entities[i].Has<Component3>(), Is.True, $"Component3 should exist on entity{entities[i]}");
            }
            
            world.Destroy();
        }
        
        [TestCase(1)]
        [TestCase(100)]
        [TestCase(1000)]
        public void Run_FilterRemoveComponentTreadJob(int count) {
            var world = WorldBuilder.Build(true);
            var filter = world.GetFilter(new FilterMask<Component1>());
            var removeComponentFilterTread = new RemoveComponentFilterThreadJob();
            var entities = new Entity[count];
            for (int i = 0; i < count; i++) {
                var entity = world.CreateEntity<IsTestEntity>();
                entity.Add(new Component1(0));
                entity.Add(new Component2());
                entities[i] = entity;
            }
           
            removeComponentFilterTread.Execute(filter);
            for (int i = 0; i < count; i++) {
                Assert.That(entities[i].Has<Component1>(), Is.True, $"Component1 should exist on entity{entities[i]}");
                Assert.That(entities[i].Has<Component2>(), Is.False, $"Component2 should exist on entity{entities[i]}");
            }
            
            world.Destroy();
        }
        
        [OneTimeTearDown]
        public void Cleanup() {
            WorldBuilder.AllDestroy();
        }
        
        private class IncreaseComponent1FilterThreadJob : FilterThreadJob {
            protected override void ExecuteInternal(Entity entity) {
                ref var c = ref entity.Get<Component1>();
                c.Value++;
            }
        }
        
        private class CreateEntityFilterThreadJob : FilterThreadJob {
            private readonly IWorld _world;
            public CreateEntityFilterThreadJob(IWorld world) {
                _world = world;
            }
            
            protected override void ExecuteInternal(Entity entity) {
                ref var e = ref _world.CreateEntity<IsTestEntity>();
                e.Add(new Component2());
            }
        }
        
        private class AddComponentFilterThreadJob : FilterThreadJob {
            protected override void ExecuteInternal(Entity entity) {
                entity.Add(new Component2());
                entity.GetOrSet<Component3>();
            }
        }
        
        private class RemoveComponentFilterThreadJob : FilterThreadJob {
            protected override void ExecuteInternal(Entity entity) {
                entity.Remove<Component2>();
            }
        }
    }   
}