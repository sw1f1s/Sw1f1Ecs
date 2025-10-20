namespace Sw1f1.Ecs {
    public interface IAutoDestroyComponent<T> where T : struct, IComponent {
        public void Destroy(ref T c);
    }
}