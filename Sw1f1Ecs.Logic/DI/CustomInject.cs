namespace Sw1f1.Ecs.DI {
    public struct CustomInject<T> : ICustomDataInject where T : class {
        public T Value { get; private set; }
        void ICustomDataInject.Fill(object[] injects) {
            if (injects.Length > 0) {
                var vType = typeof (T);
                foreach (var inject in injects) {
                    if (vType.IsInstanceOfType(inject)) {
                        Value = (T)inject;
                        break;
                    }
                }
            }
        }
    }
}