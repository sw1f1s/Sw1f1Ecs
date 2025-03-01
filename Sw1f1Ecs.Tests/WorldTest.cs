using NUnit.Framework;
using Sw1f1.Ecs.DI;

namespace Sw1f1.Ecs.Tests {
    [TestFixture]
    public class WorldTest {
        [Test]
        public void Run_CreateWorlds() {
            var world1 = WorldBuilder.Build(true);
            var world2 = WorldBuilder.Build(false);
            var world3 = WorldBuilder.Build(true);
            var world4 = WorldBuilder.Build(false);
            
            Assert.That(world1.Id, Is.EqualTo(0));
            Assert.That(world2.Id, Is.EqualTo(1));
            Assert.That(world3.Id, Is.EqualTo(2));
            Assert.That(world4.Id, Is.EqualTo(3));
            
            Assert.That(world1.IsAlive(), Is.True);
            Assert.That(world2.IsAlive(), Is.True);
            Assert.That(world3.IsAlive(), Is.True);
            Assert.That(world4.IsAlive(), Is.True);
            
            world1.Destroy();
            world3.Destroy();
            Assert.That(world1.IsAlive(), Is.False);
            Assert.That(world3.IsAlive(), Is.False);
            
            var world5 = WorldBuilder.Build(true);
            var world6 = WorldBuilder.Build(false);
            Assert.That(world5.Id, Is.EqualTo(2));
            Assert.That(world6.Id, Is.EqualTo(0));
            
            Assert.That(world1.IsAlive(), Is.False);
            Assert.That(world3.IsAlive(), Is.False);
            Assert.That(world5.IsAlive(), Is.True);
            Assert.That(world6.IsAlive(), Is.True);
        }
        
        [TestCase(1, false)]
        [TestCase(10, false)]
        [TestCase(100, false)]
        [TestCase(1000, false)]
        [TestCase(1, true)]
        [TestCase(10, true)]
        [TestCase(100, true)]
        [TestCase(1000, true)]
        public void Run_CreateEntity(int count, bool isConcurrent) {
            var world = WorldBuilder.Build(isConcurrent);
            var filter1 = world.GetFilter(new FilterMask<Component1>());
            var filter3 = world.GetFilter(new FilterMask<Component3>());
            for (int i = 0; i < count; i++) {
                var entity = world.CreateEntity<IsTestEntity>();
                entity.Add(new Component1());
                entity.GetOrSet<Component3>();
            }
            
            Assert.That(filter1.GetCount(), Is.EqualTo(count));
            Assert.That(filter3.GetCount(), Is.EqualTo(count));
            foreach (var filter in filter1) {
                filter.Remove<Component1>();
            }
            
            Assert.That(filter1.GetCount(), Is.EqualTo(0));
            Assert.That(filter3.GetCount(), Is.EqualTo(count));
            world.Destroy();
        }
        
        [TestCase(false)]
        [TestCase(true)]
        public void Run_LifeEntity(bool isConcurrent) {
            var world = WorldBuilder.Build(isConcurrent);
            
            var entity1 = world.CreateEntity<IsTestEntity>();
            Assert.That(entity1.Id, Is.EqualTo(0));
            Assert.That(entity1.Gen, Is.EqualTo(0));
            
            var entity2 = world.CreateEntity<IsTestEntity>();
            Assert.That(entity2.Id, Is.EqualTo(1));
            Assert.That(entity2.Gen, Is.EqualTo(0));
            
            entity1.Destroy();
            
            Assert.That(entity1.IsAlive(), Is.False);
            
            var entity3 = world.CreateEntity<IsTestEntity>();
            Assert.That(entity3.Id, Is.EqualTo(0));
            Assert.That(entity3.Gen, Is.EqualTo(1));
            
            entity2.Remove<IsTestEntity>();
            Assert.That(entity2.IsAlive(), Is.False);
            
            world.Destroy();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Run_CopyEntity(bool isConcurrent) {
            var world = WorldBuilder.Build(isConcurrent);
            var entity1 = world.CreateEntity<IsTestEntity>();
            entity1.Add(new Component1(100));
            entity1.GetOrSet<Component2>();
            entity1.Add(new Component3(1));

            var copy = entity1.Copy();
            Assert.That(copy.Has<Component1>(), Is.True);
            Assert.That(copy.Has<Component2>(), Is.True);
            Assert.That(copy.Has<Component3>(), Is.True);
            
            Assert.That(copy.Get<Component1>().Value,  Is.EqualTo(0));
            Assert.That(copy.Get<Component2>().Value, Is.True);
            Assert.That(copy.Get<Component3>().Value, Is.EqualTo(1f));
            
            world.Destroy();
        }
        
        [TestCase(true)]
        [TestCase(false)]
        public void Run_Components(bool isConcurrent) {
            var world = WorldBuilder.Build(isConcurrent);
            
            var entity1 = world.CreateEntity<IsTestEntity>();
            entity1.Add(new Component1(100));
            Assert.That(entity1.Has<Component1>(), Is.True);
            
            ref var component = ref entity1.Get<Component1>();
            Assert.That(component.Value == 100, Is.True);
            
            component.Value = 200;
            Assert.That(entity1.Get<Component1>().Value == 200, Is.True);

            entity1.Remove<Component1>();
            Assert.That(entity1.Has<Component1>(), Is.False);
            world.Destroy();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Run_Filters(bool isConcurrent) {
            var world = WorldBuilder.Build(isConcurrent);
            
            var entity1 = world.CreateEntity<IsTestEntity>();
            entity1.Add(new Component1(100));
            entity1.Add(new Component2(true));
            entity1.Add(new Component3(15.5f));
            
            var entity2 = world.CreateEntity<IsTestEntity>();
            entity2.Add(new Component1(200));
            entity2.Add(new Component3(0.5f));
            
            var entity3 = world.CreateEntity<IsTestEntity>();
            entity3.Add(new Component1(50));
            
            var filter1 = world.GetFilter(new FilterMask<Component1>());
            Assert.That(filter1.GetCount(), Is.EqualTo(3));
            Assert.That(filter1.GetEntities()[0] == entity1, Is.True);
            Assert.That(filter1.GetEntities()[1] == entity2, Is.True);
            Assert.That(filter1.GetEntities()[2] == entity3, Is.True);
            
            var filter2 = world.GetFilter(new FilterMask<Component1>.Exclude<Component3>());
            Assert.That(filter2.GetCount(), Is.EqualTo(1));
            Assert.That(filter2.GetEntities()[0], Is.EqualTo(entity3));
            
            var filter3 = world.GetFilter(new FilterMask<Component1>.Exclude<Component2>());
            Assert.That(filter3.GetCount(), Is.EqualTo(2));
            Assert.That(filter3.GetEntities()[0], Is.EqualTo(entity2));
            Assert.That(filter3.GetEntities()[1], Is.EqualTo(entity3));
            
            world.Destroy();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Run_Systems(bool isConcurrent) {
            var world = WorldBuilder.Build(isConcurrent);
            var systems = new Systems(world);
            systems
                .Add(new TestInitSystem())
                .Add(new TestUpdate1System())
                .Add(new TestUpdate2System())
                .Inject();
            
            systems.Init();
            var filter1 = world.GetFilter(new FilterMask<Component1>());
            Assert.That(filter1.GetCount(), Is.EqualTo(2));
            foreach (var entity in filter1) {
                Assert.That(entity.Get<Component1>().Value, Is.EqualTo(1));
            }
            
            systems.Update();
            foreach (var entity in filter1) {
                Assert.That(entity.Get<Component1>().Value, Is.EqualTo(2));
            }
            
            systems.Update();
            foreach (var entity in filter1) {
                Assert.That(entity.Get<Component1>().Value, Is.EqualTo(3));
            }
            
            systems.Dispose();
            world.Destroy();
        }
        
        [TestCase(true)]
        [TestCase(false)]
        public void Run_GroupSystems(bool isConcurrent) {
            var world = WorldBuilder.Build(isConcurrent);
            var systems = new Systems(world);
            systems
                .Add(new TestInitSystem())
                .Add(new TestGroupSystems())
                .Inject();
            
            systems.Init();
            var filter1 = world.GetFilter(new FilterMask<Component1>());
            Assert.That(filter1.GetCount(), Is.EqualTo(2));
            foreach (var entity in filter1) {
                Assert.That(entity.Get<Component1>().Value, Is.EqualTo(1));

                if (entity.Has<Component3>()) {
                    Assert.That(entity.Get<Component3>().Value, Is.EqualTo(0.5f));   
                }
            }
            
            systems.Update();
            foreach (var entity in filter1) {
                Assert.That(entity.Get<Component1>().Value, Is.EqualTo(2));
                if (entity.Has<Component3>()) {
                    Assert.That(entity.Get<Component3>().Value, Is.EqualTo(1.5f));   
                }
            }
            
            systems.SetActiveGroup(nameof(TestSubGroupSystems), false);
            systems.Update();
            foreach (var entity in filter1) {
                Assert.That(entity.Get<Component1>().Value, Is.EqualTo(3));
                if (entity.Has<Component3>()) {
                    Assert.That(entity.Get<Component3>().Value, Is.EqualTo(1.5f));   
                }
            }
            
            systems.SetActiveGroup(nameof(TestGroupSystems), false);
            systems.Update();
            foreach (var entity in filter1) {
                Assert.That(entity.Get<Component1>().Value, Is.EqualTo(3));
                if (entity.Has<Component3>()) {
                    Assert.That(entity.Get<Component3>().Value, Is.EqualTo(1.5f));   
                }
            }
            
            systems.Dispose();
            world.Destroy();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Run_Exceptions(bool isConcurrent) {
            var world = WorldBuilder.Build();
            var entity1 = world.CreateEntity<IsTestEntity>();
            var entity2 = world.CreateEntity<IsTestEntity>();
            Assert.Throws<Exception>(() => {
                entity1.Get<Component1>();
            });
            
            Assert.Throws<Exception>(() => {
                entity1.Remove<Component1>();
            });
            
            entity1.Destroy();
            Assert.Throws<Exception>(() => {
                entity1.Has<Component1>();
            });
            
            Assert.Throws<Exception>(() => {
                entity1.GetOrSet<Component1>();
            });
            
            Assert.Throws<Exception>(() => {
                entity1.Add(new Component1());
            });
            
            world.Destroy();
            Assert.Throws<Exception>(() => {
                entity1.Has<Component1>();
            });
            
            Assert.Throws<Exception>(() => {
                entity2.Has<Component1>();
            });
        }
    }
    
    public struct IsTestEntity : IComponent { }

    public struct Component1 : IComponent {
        public int Value;

        public Component1(int value) {
            Value = value;
        }
    }
    
    public struct Component2 : IComponent, IAutoResetComponent<Component2>, IAutoCopyComponent<Component2> {
        public bool Value;

        public Component2(bool value) {
            Value = value;
        }

        public void Reset(ref Component2 c) {
            c.Value = true;
        }

        public void Copy(ref Component2 src, ref Component2 dst) {
            dst.Value = src.Value;
        }
    }
    
    public struct Component3 : IComponent, IAutoCopyComponent<Component3> {
        public float Value;

        public Component3(float value) {
            Value = value;
        }

        public void Copy(ref Component3 src, ref Component3 dst) {
            dst.Value = src.Value;
        }
    }

    public sealed class TestInitSystem : IInitSystem {
        private WorldInject _worldInject = default;
        
        public void Init() {
            var entity1 = _worldInject.Value.CreateEntity<IsTestEntity>();
            entity1.Add(new Component1(1));
            
            var entity2 = _worldInject.Value.CreateEntity<IsTestEntity>();
            entity2.Add(new Component1(1));
            entity2.Add(new Component3(0.5f));
        }
    }
    
    public sealed class TestUpdate1System: IUpdateSystem {
        private FilterInject<Include<Component1>> _filter = default;
        public void Update() {
            foreach (var entity in _filter.Value) {
                ref var component = ref entity.Get<Component1>();
                component.Value += 1;
            }
        }
    }
    
    public sealed class TestUpdate2System : IUpdateSystem {
        private FilterInject<Include<Component1>, Exclude<Component2>> _filter = default;

        public void Update() {
            foreach (var entity in _filter.Value) {
                entity.Add(new Component2());   
            }
        }
    }
    
    public sealed class TestUpdate3System : IUpdateSystem {
        private FilterInject<Include<Component3>> _filter = default;

        public void Update() {
            foreach (var entity in _filter.Value) {
                ref var c = ref entity.Get<Component3>();
                c.Value += 1;
            }
        }
    }

    public sealed class TestGroupSystems : IGroupSystem {
        public string GroupName => nameof(TestGroupSystems);
        public bool State => true;

        public ISystem[] Systems => new ISystem[] {
            new TestUpdate1System(),
            new TestSubGroupSystems(),
        };
    }

    public sealed class TestSubGroupSystems : IGroupSystem {
        public string GroupName => nameof(TestSubGroupSystems);
        public bool State => true;
        public ISystem[] Systems => new ISystem[] {
            new TestUpdate3System(),
        };
    }
}