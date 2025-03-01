namespace Sw1f1.Ecs {
    internal struct DelayedOperation<T> : ISparseItem  where T : ISparseItem {
        private readonly bool _isAdd;
        private readonly int _index;
        private T _value;
            
        public int Id => _index;
        public T Value => _value;
        public bool IsAdd => _isAdd;

        public DelayedOperation(T value) {
            _isAdd = true;
            _value = value;
            _index = value.Id;
        }
            
        public DelayedOperation(int index) {
            _isAdd = false;
            _index = index;
            _value = default(T);
        }

        public ref T GetRefValue(ref DelayedOperation<T> self) {
            return ref self._value;
        }
    }
}