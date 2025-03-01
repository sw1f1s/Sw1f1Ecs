using System.Reflection;

namespace Sw1f1.Ecs.DI {
    public static class SystemsExtensions {
        public static ISystems Inject(this ISystems systems, params object[] injects) {
            if (injects == null) {
                injects = Array.Empty<object> ();
            }

            foreach (var system in systems.AllSystems) {
                InjectToSystem(systems, system, injects);
            }
            
            return systems;
        }

        private static void InjectToSystem(ISystems systems, ISystem system, params object[] injects) {
            foreach (var f in system.GetType ().GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                if (f.IsStatic) {
                    continue;
                }
                
                if (InjectBuiltIns(f, systems, system)) {
                    continue;
                }

                InjectCustoms(f, system, injects);
            }
        }
        
        private static bool InjectBuiltIns (FieldInfo fieldInfo, ISystems systems, ISystem system) {
            if (typeof (IDataInject).IsAssignableFrom (fieldInfo.FieldType)) {
                var instance = (IDataInject)fieldInfo.GetValue(system);
                instance.Fill(systems);
                fieldInfo.SetValue(system, instance);
                return true;
            }
            return false;
        }

        private static void InjectCustoms(FieldInfo fieldInfo, ISystem system, params object[] injects) {
            if (typeof (ICustomDataInject).IsAssignableFrom (fieldInfo.FieldType)) {
                var instance = (ICustomDataInject)fieldInfo.GetValue(system);
                instance.Fill(injects);
                fieldInfo.SetValue(system, instance);
            }
        }
    }   
}