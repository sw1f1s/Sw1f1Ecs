using NUnit.Framework;
using Sw1f1.Ecs.DI;

namespace Sw1f1.Ecs.Tests {
    [TestFixture]
    public class WorldTestDI {
        [Test]
        public void Run_DI() {
            var world = WorldBuilder.Build();
            var systems = new Systems(world);
            systems
                .Add(new TestInjectSystem())
                .Inject(new TestData());
            
            var entity1 = world.CreateEntity<IsTestEntity>();
            entity1.GetOrSet<Component1>();
            entity1.GetOrSet<Component2>();
            entity1.GetOrSet<Component3>();
            
            var entity2 = world.CreateEntity<IsTestEntity>();
            entity2.GetOrSet<Component1>();
            entity2.GetOrSet<Component2>();
            
            var entity3 = world.CreateEntity<IsTestEntity>();
            entity3.GetOrSet<Component1>();
            
            systems.Update();
            
            systems.Dispose();
            world.Destroy();
        }
        
        [OneTimeTearDown]
        public void Cleanup() {
            WorldBuilder.AllDestroy();
        }
    }   
    
    public sealed class TestInjectSystem : IUpdateSystem {
        private readonly WorldInject _world = default;
        private readonly FilterInject<Include<Component1, Component2>, Exclude<Component3>> _filterInject = default;
        private readonly SystemsInject _systemsInject = default;
        private readonly CustomInject<TestData> _testData = default;
        
        public void Update() {
            Assert.That(_world.Value, Is.Not.Null);
            Assert.That(_filterInject.Value, Is.Not.Null);
            Assert.That(_filterInject.Value.GetCount(), Is.EqualTo(1));
            Assert.That(_testData.Value, Is.Not.Null);
            Assert.That(_systemsInject.Value, Is.Not.Null);
        }
    }

    public class TestData {
        public int Value1;
        public float Value2;
        public string Value3;
    }
}