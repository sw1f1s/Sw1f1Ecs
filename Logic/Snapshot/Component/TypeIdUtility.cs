using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sw1f1.Ecs {
    public static class TypeIdUtility {
        private static readonly Dictionary<Type, ulong> _cache = new Dictionary<Type, ulong>();
        private const ulong FnvOffset = 14695981039346656037UL;
        private const ulong FnvPrime  = 1099511628211UL;

        public static ulong GetTypeId<T>() => GetTypeId(typeof(T));
        
        public static ulong GetTypeId(Type t) {
            if (!_cache.TryGetValue(t, out ulong value)) {
                string canonical = BuildCanonicalName(t);
                value = Fnv1a64(canonical);
                _cache.Add(t, value);
            }
            
            return value;
        }
        
        private static string BuildCanonicalName(Type t) {
            string asm = t.Assembly.GetName().Name ?? "";
            string name = GetQualifiedTypeName(t);
            if (t.IsGenericType) {
                var args = t.GetGenericArguments();
                var argNames = args.Select(BuildCanonicalName);
                name += "[" + string.Join(",", argNames) + "]";
            }
            return asm + "::" + name;
        }
        
        
        private static string GetQualifiedTypeName(Type t) {
            if (t.IsNested && t.DeclaringType != null) {
                return GetQualifiedTypeName(t.DeclaringType) + "+" + BaseName(t);
            }
            
            var ns = t.Namespace;
            return (string.IsNullOrEmpty(ns) ? "" : ns + ".") + BaseName(t);
        }
        
        private static string BaseName(Type t) {
            string n = t.Name;
            int idx = n.IndexOf('`');
            return idx >= 0 ? n[..idx] : n;
        }
        
        private static ulong Fnv1a64(string s) {
            ulong hash = FnvOffset;
            var bytes = Encoding.UTF8.GetBytes(s.ToLowerInvariant());
            foreach (byte b in bytes) {
                hash ^= b;
                hash *= FnvPrime;
            }
            return hash;
        }
    }
}