using System;
using System.Runtime.CompilerServices;
using Sw1f1.Ecs.Collections;

namespace Sw1f1.Ecs {
    public static class WorldBuilder {
        private static SparseArray<IWorld> _worlds = new SparseArray<IWorld>(Options.WORLD_CAPACITY);
        private static uint[] _freeIndexes = new uint[2] { 1, 0 };
        private static uint _freeIndexesCount = 2;
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static IWorld Build() {
            var world = CreateWorld();
            _worlds.Add(world.Id, world);
            return world;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void AllDestroy() {
            foreach (var world in _worlds) {
                world.Destroy();
            }
            _worlds.Clear();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static IWorld GetWorld(int worldId) {
            return _worlds.Get(worldId);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool AliveWorld(int worldId) {
            return _worlds.Has(worldId);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void Destroy(IWorld world) {
            _worlds.Remove(world.Id);
            _freeIndexes[_freeIndexesCount++] = (uint)world.Id;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private static IWorld CreateWorld() {
            TryResize();
            int index = (int)_freeIndexes[--_freeIndexesCount];
            return new World(index);   
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private static void TryResize() {
            if (_freeIndexesCount > 0) {
                return;
            }

            int lastSize = _freeIndexes.Length;
            _freeIndexesCount = (uint)lastSize;
            Array.Resize (ref _freeIndexes, lastSize * 2);
            for (int i = lastSize; i < _freeIndexes.Length; i++) {
                _freeIndexes[_freeIndexes.Length - 1 - i] = (uint)i;
            }
        }
    }
}