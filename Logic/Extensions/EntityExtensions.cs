using System;
using System.Collections.Generic;

namespace Sw1f1.Ecs {
    public static class EntityExtensions {
        private const string WORLD_EXCEPTION_DEAD_MESSAGE = "World {0} is dead.";
        private const string ENTITY_EXCEPTION_DEAD_MESSAGE = "{0} is dead.";
        
        public static bool IsAlive(this Entity entity) {
            if (!WorldBuilder.AliveWorld(entity.WorldId)) {
                return false;
            }
            
            var world = WorldBuilder.GetWorld(entity.WorldId);
            return world.EntityIsAlive(entity);
        }
        
        public static bool Has<T>(this Entity entity) where T : struct, IComponent {
            if (!WorldBuilder.AliveWorld(entity.WorldId)) {
                throw new Exception(string.Format(WORLD_EXCEPTION_DEAD_MESSAGE, entity.WorldId));
            }
            
            var world = WorldBuilder.GetWorld(entity.WorldId);
            if (!world.EntityIsAlive(entity)) {
                throw new Exception(string.Format(ENTITY_EXCEPTION_DEAD_MESSAGE, entity));
            }
            
            return world.HasComponent<T>(entity);
        }
        
        public static ref T Get<T>(this Entity entity) where T : struct, IComponent {
            if (!WorldBuilder.AliveWorld(entity.WorldId)) {
                throw new Exception(string.Format(WORLD_EXCEPTION_DEAD_MESSAGE, entity.WorldId));
            }
            
            var world = WorldBuilder.GetWorld(entity.WorldId);
            if (!world.EntityIsAlive(entity)) {
                throw new Exception(string.Format(ENTITY_EXCEPTION_DEAD_MESSAGE, entity));
            }
            
            return ref world.GetComponent<T>(entity);
        }
        
        public static void Add<T>(this Entity entity, T component) where T : struct, IComponent {
            if (!WorldBuilder.AliveWorld(entity.WorldId)) {
                throw new Exception(string.Format(WORLD_EXCEPTION_DEAD_MESSAGE, entity.WorldId));
            }
            
            var world = WorldBuilder.GetWorld(entity.WorldId);
            if (!world.EntityIsAlive(entity)) {
                throw new Exception(string.Format(ENTITY_EXCEPTION_DEAD_MESSAGE, entity));
            }
            
            world.AddComponent<T>(entity, ref component);
        }
        
        public static void Replace<T>(this Entity entity, T component) where T : struct, IComponent {
            if (!WorldBuilder.AliveWorld(entity.WorldId)) {
                throw new Exception(string.Format(WORLD_EXCEPTION_DEAD_MESSAGE, entity.WorldId));
            }
            
            var world = WorldBuilder.GetWorld(entity.WorldId);
            if (!world.EntityIsAlive(entity)) {
                throw new Exception(string.Format(ENTITY_EXCEPTION_DEAD_MESSAGE, entity));
            }
            
            world.RemoveComponent<T>(entity);
            world.AddComponent<T>(entity, ref component);
        }
        
        public static ref T Set<T>(this Entity entity) where T : struct, IComponent {
            if (!WorldBuilder.AliveWorld(entity.WorldId)) {
                throw new Exception(string.Format(WORLD_EXCEPTION_DEAD_MESSAGE, entity.WorldId));
            }
            
            var world = WorldBuilder.GetWorld(entity.WorldId);
            if (!world.EntityIsAlive(entity)) {
                throw new Exception(string.Format(ENTITY_EXCEPTION_DEAD_MESSAGE, entity));
            }
            
            return ref world.SetComponent<T>(entity);
        }
        
        public static ref T GetOrSet<T>(this Entity entity) where T : struct, IComponent {
            if (!WorldBuilder.AliveWorld(entity.WorldId)) {
                throw new Exception(string.Format(WORLD_EXCEPTION_DEAD_MESSAGE, entity.WorldId));
            }
            
            var world = WorldBuilder.GetWorld(entity.WorldId);
            if (!world.EntityIsAlive(entity)) {
                throw new Exception(string.Format(ENTITY_EXCEPTION_DEAD_MESSAGE, entity));
            }

            if (entity.Has<T>()) {
                return ref entity.Get<T>();
            }
            
            return ref entity.Set<T>();
        }
        
        public static void Remove<T>(this Entity entity) where T : struct, IComponent {
            if (!WorldBuilder.AliveWorld(entity.WorldId)) {
                throw new Exception(string.Format(WORLD_EXCEPTION_DEAD_MESSAGE, entity.WorldId));
            }
            
            var world = WorldBuilder.GetWorld(entity.WorldId);
            if (!world.EntityIsAlive(entity)) {
                throw new Exception(string.Format(ENTITY_EXCEPTION_DEAD_MESSAGE, entity));
            }
            
            world.RemoveComponent<T>(entity);
        }
        
        public static Entity Copy(this Entity entity) {
            if (!WorldBuilder.AliveWorld(entity.WorldId)) {
                throw new Exception(string.Format(WORLD_EXCEPTION_DEAD_MESSAGE, entity.WorldId));
            }
            
            var world = WorldBuilder.GetWorld(entity.WorldId);
            if (!world.EntityIsAlive(entity)) {
                throw new Exception(string.Format(ENTITY_EXCEPTION_DEAD_MESSAGE, entity));
            }
            
            return world.CopyEntity(entity);
        }
        
        public static void Destroy(this Entity entity) {
            if (!WorldBuilder.AliveWorld(entity.WorldId)) {
                throw new Exception(string.Format(WORLD_EXCEPTION_DEAD_MESSAGE, entity.WorldId));
            }
            
            var world = WorldBuilder.GetWorld(entity.WorldId);
            if (!world.EntityIsAlive(entity)) {
                throw new Exception(string.Format(ENTITY_EXCEPTION_DEAD_MESSAGE, entity));
            }
            
            world.DestroyEntity(entity);
        }
   
#if DEBUG
        internal static IReadOnlyList<IComponent> GetComponents(this Entity entity) {
            if (!WorldBuilder.AliveWorld(entity.WorldId)) {
                throw new Exception(string.Format(WORLD_EXCEPTION_DEAD_MESSAGE, entity.WorldId));
            }
            
            var world = WorldBuilder.GetWorld(entity.WorldId);
            if (!world.EntityIsAlive(entity)) {
                throw new Exception(string.Format(ENTITY_EXCEPTION_DEAD_MESSAGE, entity));
            }
            
            ref var entityData = ref world.Entities.Get(entity.Id);
            var components = new IComponent[entityData.Components.Count];
            int index = 0;
            foreach (var componentId in entityData.Components) {
                var storage = world.GetComponentStorage(componentId);
                components[index] = storage.GetGeneralizedComponent(entity);
                index++;
            }
            
            Array.Sort(components, (x, y) =>
                string.Compare(x.GetType().Name, y.GetType().Name, StringComparison.OrdinalIgnoreCase));
            
            return components;
        }
#endif
    }   
}