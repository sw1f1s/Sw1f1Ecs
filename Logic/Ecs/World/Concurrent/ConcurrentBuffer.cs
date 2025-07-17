using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
#if UNITY_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Sw1f1.Ecs {
#if UNITY_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal sealed class ConcurrentBuffer {
        private readonly ReaderWriterLockSlim _accessLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly List<IConcurrentOperation> _operations = new List<IConcurrentOperation>(Options.CONCURRENT_OPERATION_CAPACITY);
        private IWorld _world;
        private bool _isDisposed;

        internal ConcurrentBuffer(IWorld world) {
            _world = world;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(Entity entity, ref T component) where T : struct, IComponent {
            _accessLock.EnterWriteLock();
            try {
                var op = new AddComponentOperation<T>(entity, component);
                _operations.Add(op);
            }finally {
                _accessLock.ExitWriteLock();
            }
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent<T>(Entity entity) where T : struct, IComponent{
            _accessLock.EnterWriteLock();
            try {
                var op = new RemoveComponentOperation<T>(entity);
                _operations.Add(op);
            }finally {
                _accessLock.ExitWriteLock();
            }
        }

        public void Execute() {
            foreach (var op in _operations) {
                op.Execute(_world);
            }
            
            _operations.Clear();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            _operations.Clear();
        }
        
        ~ConcurrentBuffer() => 
            Dispose();

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            
            _isDisposed = true;
            _world = null;
            _accessLock.Dispose();
            _operations.Clear();
        }
    }   
}