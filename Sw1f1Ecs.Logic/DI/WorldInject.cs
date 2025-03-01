namespace Sw1f1.Ecs.DI {
    public struct WorldInject : IDataInject {
        public IWorld Value { get; private set; }
        
        void IDataInject.Fill(ISystems systems) {
            Value = systems.World;
        }
    }   
}