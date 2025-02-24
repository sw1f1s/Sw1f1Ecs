namespace Sw1f1.Ecs.DI {
    public struct SystemsInject : IDataInject {
        public Systems Value { get; private set; }
        void IDataInject.Fill(Systems systems) {
            Value = systems;
        }
    }   
}