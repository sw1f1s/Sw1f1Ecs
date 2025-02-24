namespace Sw1f1.Ecs {
    internal class ComponentStorageIndex {
        internal static int Counter = -1;
        internal static object _lock = new object();
    }

    internal sealed class ComponentStorageIndex<T> : ComponentStorageIndex where T : struct, IComponent {
        private static int _staticId = -1;

        internal static int StaticId {
            get {
                if (!Monitor.IsEntered(_lock)) {
                    lock (_lock) {
                        UpdateId();
                    }    
                }

                UpdateId();
                return _staticId;
            }
        }

        private static void UpdateId() {
            if (_staticId >= 0) {
                return;
            }
            
            _staticId = ++Counter;   
        }
    }
}