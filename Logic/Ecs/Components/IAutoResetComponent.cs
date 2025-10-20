namespace Sw1f1.Ecs {
    public interface IAutoResetComponent<T> where T : struct, IComponent {
        public void Reset(ref T c);
    }   
}