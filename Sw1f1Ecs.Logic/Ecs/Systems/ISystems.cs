namespace Sw1f1.Ecs {
    public interface ISystems : IDisposable {
        IWorld World { get; }
        IReadOnlyList<ISystem> AllSystems { get; }
        ISystems Add(ISystem system);
        void Init();
        void Update();

        void SetActiveGroup(string groupName, bool value);
        bool IsActiveGroup(string groupName);
    }   
}