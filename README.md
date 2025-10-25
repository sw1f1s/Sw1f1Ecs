# Sw1f1Ecs - C# Entity Component System framework
## Content
- [API](#api)
    - [Worlds](#worlds)
    - [Components](#components)
    - [Entities](#entities)
    - [Filters](#filters)
    - [Systems](#systems)
    - [Group Systems](#groupsystems)
    - [DI](#di)
    - [Snapshot](#snapshot)

## API
### Worlds
Create world
```c#
IWorld world1 = WorldBuilder.Build();
```
Destroy world
```c#
world.Destroy();
```

### Components
```c#
public struct Component1 : IComponent {
    public int Value;

    public Component1(int value) {
            Value = value;
    }
}
```
IAutoResetComponent is needed to create a new component with already defined variables
```c#
public struct Component2 : IComponent, IAutoResetComponent<Component2>, {
    public int Value;

    public Component2(int value) {
        Value = value;
    }

    public void Reset(ref Component2 c) {
        c.Value = 1;
    }
}
```
IAutoCopyComponent is needed to copy a component from one entity to another
```c#
public struct Component3 : IComponent, IAutoCopyComponent<Component3>, {
    public int Value;

    public Component3(int value) {
        Value = value;
    }

    public void Copy(ref Component3 src, ref Component3 dst) {
        dst.Value = src.Value;
    }
}
```

### Entities
Entities can only be created with a component. When the last component is deleted, the entity is deleted too.
```c#
Entity entity = world.CreateEntity<IsTestEntity>();
```

```c#
//adding a new component
entity.Add(new Component1());

//take component
ref Component1 c1 = ref entity.Get<Component1>();

//create a new one and take or take an already created component from the entity (may use IAutoResetComponent)
ref Component2 c2 = ref entity.GetOrSet<Component2>();

//check for component presence
bool isHasComponent2 = entity.Has<Component2>();

//removing a component from an entity
entity.Remove<Component1>();
```

Copying an entity creates a new entity with the same components. Copying a component can be configured using the IAutoCopyComponent interface.
```c#
var copy = entity1.Copy();
```

### Filters
Filters maintain a list of all entities with certain components
```c#
//contains entities with component Component1
var filter1 = world.GetFilter(new FilterMask<Component1>());

//contains entities with components Component1, Component2 and without component Component3
var filter2 = world.GetFilter(new FilterMask<Component1,Component2>.Exclude<Component3>());

//contains entities with component Component1 and without component Component3
var filter3 = world.GetFilter(new FilterMask<Component1>.Exclude<Component3>());

foreach (Entity entity in filter1) {
    //any actions with the entity
}
```

Multithreaded filter for working with entities in different threads
FilterThreadJob not support - set component, clone entity and create entity
```c#
private class Component1FilterThreadJob : FilterThreadJob {
    protected override void ExecuteInternal(Entity entity) {
        ref var c = ref entity.Get<Component1>();
        //any actions with the entity
    }
}
```
```c#
var filter = world.GetFilter(new FilterMask<Component1>());
var сomponent1FilterTread = new Component1FilterThreadJob();
сomponent1FilterTread.Execute(filter);
```

### Systems
The IInitSystem interface is needed to initialize the system.
```c#
public sealed class TestInitSystem : IInitSystem {
    private WorldInject _worldInject = default;
        
    public void Init() {
        //any action
    }
}
```
The IUpdateSystem interface is needed to update the system
```c#
public sealed class TestUpdate2System : IUpdateSystem {
    private FilterInject<Include<Component1>, Exclude<Component2>> _filter = default;

    public void Update() {
        foreach (var entity in _filter.Value) {
            entity.Add(new Component2());   
        }
    }
}
```
Create systems. Supports 'Inject' data
```c#
IWorld world = WorldBuilder.Build();
Systems systems = new Systems(world);
systems
    .Add(new TestInitSystem())
    .Add(new TestUpdate1System())
    .Inject();

systems.Init();
systems.Update();
```

### GroupSystems
```c#
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

//disabling a group of systems TestSubGroupSystems
systems.SetActiveGroup(nameof(TestSubGroupSystems), false);
```

### DI
Supports injection World/Filter/Systems/CustomData
```c#
public sealed class TestInjectSystem : IInitSystem, IUpdateSystem {
    private readonly WorldInject _world = default;
    private readonly FilterInject<Include<Component1, Component2>, Exclude<Component3>> _filterInject = default;
    private readonly SystemsInject _systemsInject = default;
    private readonly CustomInject<TestData> _testData = default;

    public void Init() { }
    public void Update() { }
}
```

### SNAPSHOT
Supports world snapshot
```c#
 var componentFactory = new ComponentSnapshotFactory()
        .Register<DefaultComponentPacker<Component1>>()
        .Register<DefaultComponentPacker<Component2>>();
            
 var snapshotWriter = new SnapshotWriter(componentFactory);
 var snapshotReader = new SnapshotReader(componentFactory);

 //Create snapshot
 var snapshot = snapshotWriter.Write(world1);

 //Create world by snapshot
 var world2 = snapshotReader.Read(snapshot);

 //Past snapshot on world
 snapshotReader.Read(snapshot, world3);
```
