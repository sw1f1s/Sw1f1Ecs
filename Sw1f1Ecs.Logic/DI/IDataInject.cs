namespace Sw1f1.Ecs.DI {
    public interface IDataInject {
        void Fill (ISystems systems);
    }
    
    public interface IInclude {
        public FilterMask GetMask();
    }
    
    public interface IExclude {
        public FilterMask GetMask();
    }
    
    public interface ICustomDataInject {
        void Fill (object[] injects);
    }
}