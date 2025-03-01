namespace Sw1f1.Ecs {
    public struct Entity : ISparseItem {
        public int Id { get; private set; }
        public int Gen { get; private set; }
        public int WorldId { get; private set; }
        
        /// <summary>
        /// Only read
        /// </summary>
        public IReadOnlyList<IComponent> Components => this.GetComponents();

        internal Entity(int id, int gen, int worldId) {
            Id = id;
            Gen = gen;
            WorldId = worldId;
        }
        
        internal void IncreaseGen() => 
            Gen++;

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