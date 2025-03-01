namespace Sw1f1.Ecs {
    public interface IGroupSystem : ISystem {
        public string GroupName { get; }
        public bool State { get; }
        public ISystem[] Systems { get; }
    }   
}