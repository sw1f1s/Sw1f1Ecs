using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Sw1f1.Ecs.DI;
using Sw1f1.Ecs.Tests;

namespace Sw1f1.Ecs.Benchmarks {
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class Sw1f1EcsBanchmarks {
        private World _world;
        private Filter _filter;
        private Systems _systems;
        private Entity _entity;
        
        [GlobalSetup]
        public void Setup() {
            _world = WorldBuilder.Build();
            _systems = new Systems(_world)
                .Add(new TestInitSystem())
                .Add(new TestUpdate1System())
                .Add(new TestUpdate2System())
                .Inject();
            
            _filter = _world.GetFilter(new FilterMask<Component1, Component2>.Exclude<Component3>());
            _entity = _world.CreateEntity();
            _entity.Add(new Component1());
            _entity.GetOrSet<Component2>();
        }
        
        [Benchmark]
        public void CreateEntity1() { 
            _world.CreateEntity();
        } 
        
        [Benchmark]
        public void CreateEntity100() {
            for (int i = 0; i < 100; i++) {
                _world.CreateEntity();
            }
        } 
        
        [Benchmark]
        public void GetComponent() {
            _entity.Get<Component1>();
        } 
        
        [Benchmark]
        public void GetOrSetComponent() {
            _entity.GetOrSet<Component1>();
        } 
        
        [Benchmark]
        public void HasComponent() {
            _entity.Has<Component1>();
        } 
        
        [Benchmark]
        public void GetFilter1() {
            foreach (var entity in _filter) { }
        } 
        
        [Benchmark]
        public void Run1() {
            _systems.Init();
            _systems.Update();
        }
        
        [Benchmark]
        public void Run100() {
            _systems.Init();
            for (int i = 0; i < 100; i++) {
                _systems.Update();
            }
        }
    }   
}