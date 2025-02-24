namespace Sw1f1.Ecs {
    public class FilterMask {
        protected List<int> _includes;
        protected List<int> _excludes;
            
        protected FilterMask() {
            _includes = new List<int>(8);
            _excludes = new List<int>(8);
        }

        internal int[] GetIncludes() {
            return _includes.ToArray();
        }
        
        internal int[] GetExcludes() {
            return _excludes.ToArray();
        }

        public override int GetHashCode() {
            int includeHash = 0;
            int excludeHash = 0;
            
            for (int i = 0; i < _includes.Count; i++) {
                includeHash ^= _includes[i].GetHashCode();
            }
            
            for (int i = 0; i < _excludes.Count; i++) {
                excludeHash ^= _excludes[i].GetHashCode();
            }

            return HashCode.Combine(includeHash, excludeHash);
        }

        public static FilterMask Combine(FilterMask mask1, FilterMask mask2) {
            var mask = new FilterMask();
            mask._includes.AddRange(mask1.GetIncludes());
            mask._includes.AddRange(mask2.GetIncludes());
            mask._excludes.AddRange(mask1.GetExcludes());
            mask._excludes.AddRange(mask2.GetExcludes());
            return mask;
        }
    }

    public class FilterMaskExclude<Exc1> : FilterMask
        where Exc1 : struct, IComponent {
        public FilterMaskExclude() {
            _excludes.Add(ComponentStorageIndex<Exc1>.StaticId);
        }
    }
    
    public class FilterMaskExclude<Exc1, Exc2> : FilterMask
        where Exc1 : struct, IComponent 
        where Exc2 : struct, IComponent {
        
        public FilterMaskExclude() {
            _excludes.Add(ComponentStorageIndex<Exc1>.StaticId);
            _excludes.Add(ComponentStorageIndex<Exc2>.StaticId);
        }
    }

    public class FilterMask<Inc1> : FilterMask 
            where Inc1 : struct, IComponent {
        
        public FilterMask() : base() {
            _includes.Add(ComponentStorageIndex<Inc1>.StaticId);
        }

        public class Exclude<Exc1> : FilterMask<Inc1> 
            where Exc1 : struct, IComponent {
            public Exclude() : base() {
                _excludes.Add(ComponentStorageIndex<Exc1>.StaticId);
            }
        }
        
        public class Exclude<Exc1, Exc2> : FilterMask<Inc1> 
            where Exc1 : struct, IComponent
            where Exc2 : struct, IComponent {
            public Exclude() : base() {
                _excludes.Add(ComponentStorageIndex<Exc1>.StaticId);
                _excludes.Add(ComponentStorageIndex<Exc2>.StaticId);
            }
        }
    }
    
    public class FilterMask<Inc1, Inc2> : FilterMask 
        where Inc1 : struct, IComponent
        where Inc2 : struct, IComponent {
        public FilterMask() : base() {
            _includes.Add(ComponentStorageIndex<Inc1>.StaticId);
            _includes.Add(ComponentStorageIndex<Inc2>.StaticId);
        }
        
        public class Exclude<Exc1> : FilterMask<Inc1, Inc2> 
            where Exc1 : struct, IComponent {
            public Exclude() : base() {
                _excludes.Add(ComponentStorageIndex<Exc1>.StaticId);
            }
        }
        
        public class Exclude<Exc1, Exc2> : FilterMask<Inc1, Inc2> 
            where Exc1 : struct, IComponent
            where Exc2 : struct, IComponent {
            public Exclude() : base() {
                _excludes.Add(ComponentStorageIndex<Exc1>.StaticId);
                _excludes.Add(ComponentStorageIndex<Exc2>.StaticId);
            }
        }
    }
    
    public class FilterMask<Inc1, Inc2, Inc3> : FilterMask 
        where Inc1 : struct, IComponent
        where Inc2 : struct, IComponent
        where Inc3 : struct, IComponent {
        public FilterMask() : base() {
            _includes.Add(ComponentStorageIndex<Inc1>.StaticId);
            _includes.Add(ComponentStorageIndex<Inc2>.StaticId);
            _includes.Add(ComponentStorageIndex<Inc3>.StaticId);
        }
        
        public class Exclude<Exc1> : FilterMask<Inc1, Inc2, Inc3> 
            where Exc1 : struct, IComponent {
            public Exclude() : base() {
                _excludes.Add(ComponentStorageIndex<Exc1>.StaticId);
            }
        }
        
        public class Exclude<Exc1, Exc2> : FilterMask<Inc1, Inc2, Inc3> 
            where Exc1 : struct, IComponent
            where Exc2 : struct, IComponent {
            public Exclude() : base() {
                _excludes.Add(ComponentStorageIndex<Exc1>.StaticId);
                _excludes.Add(ComponentStorageIndex<Exc2>.StaticId);
            }
        }
    }
    
    public class FilterMask<Inc1, Inc2, Inc3, Inc4> : FilterMask 
        where Inc1 : struct, IComponent
        where Inc2 : struct, IComponent
        where Inc3 : struct, IComponent 
        where Inc4 : struct, IComponent {
        public FilterMask() : base() {
            _includes.Add(ComponentStorageIndex<Inc1>.StaticId);
            _includes.Add(ComponentStorageIndex<Inc2>.StaticId);
            _includes.Add(ComponentStorageIndex<Inc3>.StaticId);
            _includes.Add(ComponentStorageIndex<Inc4>.StaticId);
        }
        
        public class Exclude<Exc1> : FilterMask<Inc1, Inc2, Inc3, Inc4> 
            where Exc1 : struct, IComponent {
            public Exclude() : base() {
                _excludes.Add(ComponentStorageIndex<Exc1>.StaticId);
            }
        }
        
        public class Exclude<Exc1, Exc2> : FilterMask<Inc1, Inc2, Inc3, Inc4> 
            where Exc1 : struct, IComponent
            where Exc2 : struct, IComponent {
            public Exclude() : base() {
                _excludes.Add(ComponentStorageIndex<Exc1>.StaticId);
                _excludes.Add(ComponentStorageIndex<Exc2>.StaticId);
            }
        }
    }
    
    public class FilterMask<Inc1, Inc2, Inc3, Inc4, Inc5> : FilterMask 
        where Inc1 : struct, IComponent
        where Inc2 : struct, IComponent
        where Inc3 : struct, IComponent 
        where Inc4 : struct, IComponent
        where Inc5 : struct, IComponent{
        public FilterMask() : base() {
            _includes.Add(ComponentStorageIndex<Inc1>.StaticId);
            _includes.Add(ComponentStorageIndex<Inc2>.StaticId);
            _includes.Add(ComponentStorageIndex<Inc3>.StaticId);
            _includes.Add(ComponentStorageIndex<Inc4>.StaticId);
            _includes.Add(ComponentStorageIndex<Inc5>.StaticId);
        }
        
        public class Exclude<Exc1> : FilterMask<Inc1, Inc2, Inc3, Inc4, Inc5> 
            where Exc1 : struct, IComponent {
            public Exclude() : base() {
                _excludes.Add(ComponentStorageIndex<Exc1>.StaticId);
            }
        }
        
        public class Exclude<Exc1, Exc2> : FilterMask<Inc1, Inc2, Inc3, Inc4, Inc5> 
            where Exc1 : struct, IComponent
            where Exc2 : struct, IComponent {
            public Exclude() : base() {
                _excludes.Add(ComponentStorageIndex<Exc1>.StaticId);
                _excludes.Add(ComponentStorageIndex<Exc2>.StaticId);
            }
        }
    }
}