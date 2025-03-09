using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
    public struct Entity {
        public static readonly Entity Empty = new Entity(-1, -1, -1);
        public int Id { get; private set; }
        public int Gen { get; private set; }
        public int WorldId { get; private set; }

#if DEBUG
        public IReadOnlyList<IComponent> Components => this.GetComponents();  
#endif
        internal Entity(int id, int gen, int worldId) {
            Id = id;
            Gen = gen;
            WorldId = worldId;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void IncreaseGen() => 
            Gen++;
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal void ResetGen() => 
            Gen = 0;

        public override bool Equals(object? obj) {
            return obj is Entity other && Equals(other);
        }

        public bool Equals(Entity other) {
            return Id == other.Id && Gen == other.Gen && WorldId == other.WorldId;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Id, Gen, WorldId);
        }

        public static bool operator ==(Entity left, Entity right) {
            return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right) {
            return !(left == right);
        }

        public override string ToString() {
            return $"Entity[{Id}|{Gen}|{WorldId}]";
        }
    }   
}