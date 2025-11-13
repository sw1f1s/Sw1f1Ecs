using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sw1f1.Ecs.Collections;
#if UNITY_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Sw1f1.Ecs {
#if UNITY_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal class FilterMap : IDisposable {
        private readonly List<Filter> _filters = new List<Filter>(Options.FILTER_CAPACITY);
        private readonly Dictionary<int, List<int>> _filterComponentsMaps = new Dictionary<int, List<int>>(Options.FILTER_CAPACITY);
        private readonly Dictionary<int, List<int>> _filterMaskMaps = new Dictionary<int, List<int>>(Options.FILTER_CAPACITY);
        private IWorld _world;
        private SparseArray<int> _componentForUpdate = new SparseArray<int>(Options.COMPONENT_CAPACITY);

        public FilterMap(IWorld world) {
            _world = world;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Filter GetFilter(FilterMask mask) {
            int hashId = mask.GetHashId();
            if (TryGetFilter(mask, hashId, out Filter filter)) {
                return filter;
            }

            var newFilter = CreateNewFilter(mask, hashId);
            foreach (var componentId in mask.GetIncludes()) {
                _filterComponentsMaps.TryAdd(componentId, new List<int>());
                _filterComponentsMaps[componentId].Add(_filters.Count - 1);
            }
            
            foreach (var componentId in mask.GetExcludes()) {
                _filterComponentsMaps.TryAdd(componentId, new List<int>());
                _filterComponentsMaps[componentId].Add(_filters.Count - 1);
            }
            return newFilter;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void SetDirty(int componentId) {
            if (!_componentForUpdate.Has(componentId)) {
                _componentForUpdate.Add(componentId, componentId);
            }
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void SetDirty() {
            if (_componentForUpdate.Count == 0) {
                return;
            }
            
            foreach (var component in _componentForUpdate) {
                if (_filterComponentsMaps.TryGetValue(component, out var filterIndexes)) {
                    foreach (var index in filterIndexes) {
                        _filters[index].SetDirty();
                    }
                }
            }
            
            _componentForUpdate.Clear();
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private bool TryGetFilter(FilterMask mask, int hashId, out Filter filter) {
            filter = null;
            if (_filterMaskMaps.TryGetValue(hashId, out var collisions)) {
                for (int i = 0; i < collisions.Count; i++) {
                    var f = _filters[collisions[i]];
                    if (mask.GetIncludes().HasAllCollision(f.Includes) && mask.GetExcludes().HasAllCollision(f.Excludes)) {
                        filter = f;
                        return true;
                    }
                }
            }

            return false;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private Filter CreateNewFilter(FilterMask mask, int hashId) {
            var newFilter = new Filter(mask, this, _world);
            int filterIndex = _filters.Count;
            _filters.Add(newFilter);
            if (_filterMaskMaps.TryGetValue(hashId, out var collisions)) {
                collisions.Add(filterIndex);
            }else {
                _filterMaskMaps.Add(hashId, new List<int>(filterIndex)); 
            }

            return newFilter;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            foreach (var filter in _filters) {
                filter.Dispose();
            }
            _filters.Clear();
            _filterMaskMaps.Clear();
            _componentForUpdate.Clear();
            _filterComponentsMaps.Clear();
        }

        public void Dispose() {
            _world = null;
            foreach (var filter in _filters) {
                filter.Dispose();
            }
            _filters.Clear();
            _filterMaskMaps.Clear();
            _filterComponentsMaps.Clear();
            _componentForUpdate.Dispose();
        }
    }   
}