using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PubSubDataConstructor
{
    internal class TypeHelper
    {
        private class LinqMethods
        {
            public MethodInfo Cast { get; set; }
            public MethodInfo Distinct { get; set; }
            public MethodInfo ToArray { get; set; }
        }

        private static readonly Dictionary<Type, LinqMethods> cache = new Dictionary<Type, LinqMethods>();

        public static bool IsDefaultValue(object obj)
        {
            if (obj == null || (obj is string && string.IsNullOrEmpty((string)obj)))
                return true;

            var type = obj.GetType();
            return type.IsValueType ? object.Equals(Activator.CreateInstance(type), obj) : false;
        }

        public static object Default(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static object InvokeLinqMethod(string method, Type type, object sequence)
        {
            var methods = GetLinqMethods(type);

            switch (method)
            {
                case "Cast":
                    return methods.Cast.Invoke(null, new[] { sequence });
                case "Distinct":
                    return methods.Distinct.Invoke(null, new[] { sequence });
                case "ToArray":
                    return methods.ToArray.Invoke(null, new[] { sequence });
                default:
                    throw new ArgumentOutOfRangeException("Unknown method");
            }
        }

        public static object CombineArray(Type type, object sequence)
        {
            var methods = GetLinqMethods(type);

            sequence = methods.Cast.Invoke(null, new[] { sequence });
            sequence = methods.Distinct.Invoke(null, new[] { sequence });
            return methods.ToArray.Invoke(null, new[] { sequence });
        }

        private static LinqMethods GetLinqMethods(Type type)
        {
            LinqMethods methods;
            if (!cache.TryGetValue(type, out methods))
            {
                var enumerable = typeof(System.Linq.Enumerable);
                methods = new LinqMethods
                {
                    Cast = enumerable.GetMethod("Cast").MakeGenericMethod(type),
                    Distinct = enumerable.GetMethods().First(x => x.Name == "Distinct").MakeGenericMethod(type),
                    ToArray = enumerable.GetMethod("ToArray").MakeGenericMethod(type)
                };

                cache.Add(type, methods);
            }
            return methods;
        }
    }
}
