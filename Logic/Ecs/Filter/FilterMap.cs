using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sw1f1.Ecs {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal class FilterMap : IDisposable {
        private readonly List<Filter> _filters = new(Options.FILTER_CAPACITY);
        private readonly Dictionary<int, List<int>> _filterComponentsMaps = new (Options.FILTER_CAPACITY);
        private readonly Dictionary<int, int> _filterMaskMaps = new (Options.FILTER_CAPACITY);
        private IWorld _world;
        private SparseSet<ComponentId> _componentForUpdate = new (Options.COMPONENT_CAPACITY);

        public FilterMap(IWorld world) {
            _world = world;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Filter GetFilter(FilterMask mask) {
            int hashId = mask.GetHashId();
            if (_filterMaskMaps.TryGetValue(hashId, out var index)) {
                return _filters[index];
            }
            
            var newFilter = new Filter(mask, _world);
            int filterIndex = _filters.Count;
            _filters.Add(newFilter);
            _filterMaskMaps.Add(hashId, filterIndex);
            foreach (var componentId in mask.GetIncludes()) {
                _filterComponentsMaps.TryAdd(componentId, new List<int>());
                _filterComponentsMaps[componentId].Add(filterIndex);
            }
            
            foreach (var componentId in mask.GetExcludes()) {
                _filterComponentsMaps.TryAdd(componentId, new List<int>());
                _filterComponentsMaps[componentId].Add(filterIndex);
            }
            return newFilter;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void UpdateFilters(int componentId) {
            if (!_componentForUpdate.Has(componentId)) {
                _componentForUpdate.Add(new ComponentId(componentId));
            }
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void UpdateFilters() {
            if (_componentForUpdate.Count == 0) {
                return;
            }
            
            foreach (var component in _componentForUpdate) {
                if (_filterComponentsMaps.TryGetValue(component.Id, out var filterIndexes)) {
                    foreach (var index in filterIndexes) {
                        _filters[index].NeedUpdate();
                    }
                }
            }
            
            _componentForUpdate.Clear();
        }

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