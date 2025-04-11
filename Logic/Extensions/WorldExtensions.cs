using System;
using System.Collections.Generic;
using Sw1f1.Ecs.Collections;

namespace Sw1f1.Ecs {
    public static class WorldExtensions {
        public static void Destroy(this IWorld world) {
            world.Dispose();
        }
        
        public static bool IsAlive(this IWorld world) {
            return world.IsAlive && WorldBuilder.AliveWorld(world.Id);
        }
        
        internal static IReadOnlyList<Type> GetTypeComponents(this IWorld world, in BitMask mask) {
            var components = new Type[mask.Count];
            int index = 0;
            foreach (var componentId in mask) {
                if (world.HasComponentStorage(componentId)) {
                    var storage = world.GetComponentStorage(componentId);
                    components[index] = storage.ComponentType;   
                }
                index++;
            }
            return components;
        }
    }   
}