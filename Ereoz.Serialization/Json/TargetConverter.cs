using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Ereoz.Serialization.Json
{
    internal static class TargetConverter
    {
        public static T Convert<T>(Dictionary<string, object> virtualObject) =>
            (T)Convert(typeof(T), virtualObject);

        public static object Convert(Type targetType, Dictionary<string, object> virtualObject)
        {
            if (targetType.Name.StartsWith("Tuple"))
            {
                Type[] args = targetType.GetGenericArguments();
                object[] values = new object[args.Length];

                int index = 0;
                foreach (var kvp in virtualObject)
                {
                    object value = null;
                    try
                    {
                        value = ParseSimple(args[index], virtualObject[$"Item{index + 1}"]);
                    }
                    catch
                    {
                        if (kvp.Value is Array array)
                        {
                            GetFromJsonArray(args[index], ref array, ref value);
                        }
                        else
                        {
                            value = Convert(args[index], kvp.Value as Dictionary<string, object>);
                        }
                    }
                    values[index] = value;
                    index++;
                }

                ConstructorInfo constructor = targetType.GetConstructors().First();

                return constructor.Invoke(values);
            }

            if (targetType.Name.StartsWith("Dictionary"))
            {
                var dic = Activator.CreateInstance(targetType);

                var keyType = targetType.GetGenericArguments()[0];
                var valueType = targetType.GetGenericArguments()[1];

                foreach (var kvp in virtualObject)
                {
                    object key = null;
                    object value = null;

                    try
                    {
                        key = ParseSimple(keyType, kvp.Key);
                    }
                    catch
                    {
                        throw new FormatException($"{keyType} - not supported type. Dictionary key must be simple type");
                    }

                    try
                    {
                        value = ParseSimple(valueType, kvp.Value);
                    }
                    catch
                    {
                        if (kvp.Value is Array array)
                        {
                            GetFromJsonArray(valueType, ref array, ref value);
                        }
                        else
                        {
                            value = Convert(valueType, kvp.Value as Dictionary<string, object>);
                        }
                    }

                    targetType.GetMethod("Add")
                              .Invoke(dic, new object[] { key, value });
                }

                return dic;
            }


            if (virtualObject.TryGetValue("0SimpleValue", out object simpleValue))
            {
                return ParseSimple(targetType, simpleValue);
            }
            else if (virtualObject.TryGetValue("0Sequence", out object sequence))
            {
                var array = sequence as Array;
                object originalArray = null;

                GetFromJsonArray(targetType, ref array, ref originalArray);

                return originalArray;
            }

            var originalProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                   .Where(p => p.CanRead);

            object originalObject = FormatterServices.GetUninitializedObject(targetType);

            foreach (var kvp in virtualObject)
            {
                PropertyInfo propertyInfo = originalProperties.Single(prop => prop.Name == kvp.Key);
                Type type = propertyInfo.PropertyType;
                object value = null;

                if (kvp.Value is Dictionary<string, object> nestedObject)
                {
                    Type propertyType = originalProperties.Single(prop => prop.Name == kvp.Key).PropertyType;
                    value = Convert(propertyType, nestedObject);
                    propertyInfo.SetValue(originalObject, value, null);
                }
                else if (kvp.Value is Array array)
                {
                    GetFromJsonArray(type, ref array, ref value);
                    propertyInfo.SetValue(originalObject, value, null);
                }
                else
                {
                    value = ParseSimple(propertyInfo.PropertyType, kvp.Value);
                    propertyInfo.SetValue(originalObject, value, null);
                }
            }

            return originalObject;
        }

        private static void GetFromJsonArray(Type type, ref Array array, ref object value)
        {
            if (type.Name.EndsWith("[]"))
            {
                value = CreateArray(type, array);
            }
            else if (type.Name.StartsWith("IList") || type.Name.StartsWith("List"))
            {
                value = CreateIList(type.GetGenericArguments()[0], typeof(List<>), array);
            }
            else if (type.Name.StartsWith("ICollection") || type.Name.StartsWith("Collection"))
            {
                value = CreateIList(type.GetGenericArguments()[0], typeof(Collection<>), array);
            }
            else if (type.Name.StartsWith("ObservableCollection"))
            {
                value = CreateIList(type.GetGenericArguments()[0], typeof(ObservableCollection<>), array);
            }
            else if (type.Name.StartsWith("ReadOnlyCollection"))
            {
                value = CreateSpecialWrapper(type.GetGenericArguments()[0], typeof(ReadOnlyCollection<>), array);
            }
            else if (type.Name.StartsWith("LinkedList"))
            {
                value = CreateSpecialWrapper(type.GetGenericArguments()[0], typeof(LinkedList<>), array);
            }
            else if (type.Name.StartsWith("HashSet"))
            {
                value = CreateSet(type.GetGenericArguments()[0], typeof(HashSet<>), array);
            }
            else if (type.Name.StartsWith("Queue"))
            {
                value = CreateSpecialWrapper(type.GetGenericArguments()[0], typeof(Queue<>), array);
            }
            else if (type.Name.StartsWith("Stack"))
            {
                value = CreateSpecialWrapper(type.GetGenericArguments()[0], typeof(Stack<>), array);
            }
            else if (type.GetInterface("IEnumerable") != null)
            {
                value = CreateIList(type.GetGenericArguments()[0], typeof(List<>), array);
            }
        }

        private static object CreateArray(Type propertyType, Array array)
        {
            Type elementType = propertyType.GetElementType();

            var newArray = Array.CreateInstance(elementType, array.Length);
            int index = 0;
            foreach (var item in array)
            {
                object value;

                try
                {
                    value = ParseSimple(elementType, item);
                }
                catch
                {
                    value = Convert(elementType, item as Dictionary<string, object>);
                }

                newArray.SetValue(value, index);
                index++;
            }

            return newArray;
        }

        private static object CreateIList(Type elementType, Type genericType, Array array)
        {
            var type = genericType.MakeGenericType(elementType);
            var list = (IList)Activator.CreateInstance(type);

            foreach (var item in array)
            {
                object value;

                try
                {
                    value = ParseSimple(elementType, item);
                }
                catch
                {
                    value = Convert(elementType, item as Dictionary<string, object>);
                }

                list.Add(value);
            }

            return list;
        }

        private static object CreateSet(Type elementType, Type genericType, Array array)
        {
            var type = genericType.MakeGenericType(elementType);
            var set = Activator.CreateInstance(type);

            foreach (var item in array)
            {
                object value;

                try
                {
                    value = ParseSimple(elementType, item);
                }
                catch
                {
                    value = Convert(elementType, item as Dictionary<string, object>);
                }

                type.GetMethod("Add")
                    .Invoke(set, new object[] { value });
            }

            return set;
        }

        private static object CreateSpecialWrapper(Type elementType, Type genericType, Array array)
        {
            var type = genericType.MakeGenericType(elementType);
            var tempType = typeof(List<>).MakeGenericType(elementType);
            var templist = (IList)Activator.CreateInstance(tempType);

            foreach (var item in array)
            {
                object value;

                try
                {
                    value = ParseSimple(elementType, item);
                }
                catch
                {
                    value = Convert(elementType, item as Dictionary<string, object>);
                }

                templist.Add(value);
            }

            var collection = FormatterServices.GetUninitializedObject(type);
            var constructor = type.GetConstructors().Last(it => it.GetParameters().Length > 0);
            constructor.Invoke(collection, new object[] { templist });

            return collection;
        }

        private static object ParseSimple(Type type, object obj)
        {
            if (type == typeof(sbyte))
                return sbyte.Parse(obj.ToString());
            else if (type == typeof(byte))
                return byte.Parse(obj.ToString());
            else if (type == typeof(short))
                return short.Parse(obj.ToString());
            else if (type == typeof(ushort))
                return ushort.Parse(obj.ToString());
            else if (type == typeof(int))
                return int.Parse(obj.ToString());
            else if (type == typeof(uint))
                return uint.Parse(obj.ToString());
            else if (type == typeof(long))
                return long.Parse(obj.ToString());
            else if (type == typeof(ulong))
                return ulong.Parse(obj.ToString());
            else if (type == typeof(float))
            {
                if (obj.ToString().Equals("NaN", StringComparison.Ordinal))
                    return float.NaN;

                if (obj.ToString().Equals("Infinity", StringComparison.Ordinal))
                    return float.PositiveInfinity;

                if (obj.ToString().Equals("-Infinity", StringComparison.Ordinal))
                    return float.NegativeInfinity;

                return float.Parse(obj.ToString());
            }
            else if (type == typeof(double))
            {
                if (obj.ToString().Equals("NaN", StringComparison.Ordinal))
                    return double.NaN;

                if (obj.ToString().Equals("Infinity", StringComparison.Ordinal))
                    return double.PositiveInfinity;

                if (obj.ToString().Equals("-Infinity", StringComparison.Ordinal))
                    return double.NegativeInfinity;

                try
                {
                    return double.Parse(obj.ToString());
                }
                catch
                {
                    string[] parts = obj.ToString().Split('E', 'e');

                    if (parts.Length != 2)
                        throw new FormatException($"{obj}: Некорректный формат числа.");

                    double mantissa = double.Parse(parts[0], CultureInfo.InvariantCulture);
                    int exponent = int.Parse(parts[1].Replace("+", ""), CultureInfo.InvariantCulture);

                    return mantissa * Math.Pow(10, exponent);
                }
            }
            else if (type == typeof(decimal))
                return decimal.Parse(obj.ToString());
            else if (type == typeof(bool))
                return bool.Parse(obj.ToString());
            else if (type == typeof(DateTime))
                return DateTime.Parse(obj.ToString());
            else if (type == typeof(TimeSpan))
                return TimeSpan.Parse(obj.ToString());
            else if (type == typeof(char))
                return char.Parse(obj.ToString());
            else if (type == typeof(string) || type.BaseType == typeof(Enum))
                return obj;

            throw new Exception($"ParseSimple: '{type.Name}' uknown type");
        }
    }
}
