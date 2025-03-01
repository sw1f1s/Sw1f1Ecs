namespace Sw1f1.Ecs.DI {
    public struct SystemsInject : IDataInject {
        public ISystems Value { get; private set; }
        void IDataInject.Fill(ISystems systems) {
            Value = systems;
        }
    }   
}