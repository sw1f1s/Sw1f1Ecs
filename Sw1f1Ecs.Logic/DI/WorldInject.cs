namespace Sw1f1.Ecs.DI {
    public struct WorldInject : IDataInject {
        public World Value { get; private set; }
        
        void IDataInject.Fill(Systems systems) {
            Value = systems.World;
        }
    }   
}