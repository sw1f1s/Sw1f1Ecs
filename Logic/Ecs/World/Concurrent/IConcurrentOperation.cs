namespace Sw1f1.Ecs {
    internal interface IConcurrentOperation {
        public void Execute(IWorld world);
    }   
}