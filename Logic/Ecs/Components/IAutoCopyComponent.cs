namespace Sw1f1.Ecs {
    public interface IAutoCopyComponent<T> where T : struct, IComponent {
        public void Copy(ref T src, ref T dst);
    }   
}