using System.Collections.Generic;

namespace System.Reflection
{
    public static class TypeExtender
    {
        public static Type[] EmptyTypes = { };

        public static Assembly Assembly(this Type type)
        {
            return type.GetTypeInfo().Assembly;
        }

        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }

        public static bool IsAbstract(this Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static Type BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static Type[] FindInterfaces(this Type type, Func<Type, object, bool> filter, object criteria)
        {
            List<Type> results = new List<Type>();
            foreach (Type walk in type.GetInterfaces())
            {
                if (filter(type, criteria))
                    results.Add(walk);
            }

            return results.ToArray();
        }
    }
}

