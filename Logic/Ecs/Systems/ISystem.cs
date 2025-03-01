namespace Sw1f1.Ecs {
    public interface ISystem { }

    public interface IInitSystem : ISystem {
        public void Init();
    }
    
    public interface IUpdateSystem : ISystem {
        public void Update();
    }
}