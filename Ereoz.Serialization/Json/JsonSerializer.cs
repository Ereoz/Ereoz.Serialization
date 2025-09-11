using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ereoz.Serialization.Json
{
    internal static class JsonSerializer
    {
        // Сериализует объект. Параметр level нужен добавления отступов согласно уровню вложенности.
        internal static string Serialize(object obj, int level, bool formattingIndented)
        {
            if (obj == null)
                return "null";

            Type type = obj.GetType();

            if (obj is char)
                return $"\"{obj}\"";

            // TODO: Реализовать не только int, но и другие возможные варианты.
            if (obj is Enum)
                return ((int)obj).ToString();

            if (obj is DateTime)
                return $"\"{((DateTime)obj).ToString("yyyy-MM-ddTHH:mm:ss.fff")}\"";

            if (obj is TimeSpan)
                return $"\"{obj}\"";

            if (type.IsPrimitive || obj is decimal)
                return SerializeBasic(obj);

            if (obj is string)
                return SerializeString(obj as string);

            if (obj is IEnumerable)
            {
                if (!obj.GetType().Name.StartsWith("Dictionary"))
                    return SerializeEnumerable(obj, level, formattingIndented);
                else
                    return SerializeDictionary(obj, level, formattingIndented);
            }

            return SerializeObject(obj, level, formattingIndented);
        }

        // Сериализует byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal, bool, char.
        private static string SerializeBasic(object obj)
        {
            // добавить или проверить: NaN, Infiniti и т.д.

            if (obj.GetType() == typeof(float))
            {
                if((float)obj is float.NaN)
                    return "\"NaN\"";

                if ((float)obj is float.PositiveInfinity)
                    return "\"Infinity\"";

                if ((float)obj is float.NegativeInfinity)
                    return "\"-Infinity\"";

                return ((float)obj).ToString("R", CultureInfo.InvariantCulture);
            }

            if (obj.GetType() == typeof(double))
            {
                if ((double)obj is double.NaN)
                    return "\"NaN\"";

                if ((double)obj is double.PositiveInfinity)
                    return "\"Infinity\"";

                if ((double)obj is double.NegativeInfinity)
                    return "\"-Infinity\"";

                return ((double)obj).ToString("R", CultureInfo.InvariantCulture);
            }

            if (obj.GetType() == typeof(decimal))
                return ((decimal)obj).ToString(null, CultureInfo.InvariantCulture);

            return Convert.ToString(obj, CultureInfo.InvariantCulture).ToLower();
        }

        // Сериализует строку с экранированием специальных символов
        // и обработкой управляющих символов для соответствия формату JSON.
        private static string SerializeString(string obj)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in obj)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ')
                            sb.AppendFormat("\\u{0:x4}", (int)c);
                        else
                            sb.Append(c);
                        break;
                }
            }
            return $"\"{sb.ToString()}\"";
        }

        // Сериализует объекты.
        private static string SerializeObject(object obj, int level, bool formattingIndented)
        {
            StringBuilder builder = new StringBuilder();
            var tab = string.Empty;

            if (formattingIndented)
            {
                for (int i = 0; i < level; i++)
                    tab += "  ";

                builder.Append("{\r\n" + tab);
            }
            else
            {
                builder.Append("{");
            }

            var properties = obj.GetType()
                                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.CanRead && !Attribute.IsDefined(p, typeof(NonSerializedAttribute)));

            bool isFirst = true;
            foreach (PropertyInfo prop in properties)
            {
                object value = prop.GetValue(obj, null);

                if (!isFirst)
                {
                    if (formattingIndented)
                        builder.Append(",\r\n" + tab);
                    else
                        builder.Append(",");
                }

                if (formattingIndented)
                    builder.Append($"\"{prop.Name}\": {Serialize(value, level + 1, formattingIndented)}");
                else
                    builder.Append($"\"{prop.Name}\":{Serialize(value, level + 1, formattingIndented)}");

                isFirst = false;
            }

            if (formattingIndented)
                builder.Append("\r\n" + tab.Substring(0, tab.Length - 2) + "}");
            else
                builder.Append("}");

            return builder.ToString();
        }

        // Сериализует IEnumerable (кроме Dictionary).
        private static string SerializeEnumerable(object obj, int level, bool formattingIndented)
        {
            StringBuilder builder = new StringBuilder();
            var tab = string.Empty;

            if (formattingIndented)
            {
                for (int i = 0; i < level; i++)
                    tab += "  ";

                builder.Append("[\r\n" + tab);
            }
            else
            {
                builder.Append("[");
            }

            bool isFirst = true;
            foreach (var item in obj as IEnumerable)
            {
                if (!isFirst)
                {
                    if (formattingIndented)
                        builder.Append(",\r\n" + tab);
                    else
                        builder.Append(",");
                }

                builder.Append($"{Serialize(item, level + 1, formattingIndented)}");
                isFirst = false;
            }

            if (formattingIndented)
                builder.Append("\r\n" + tab.Substring(0, tab.Length - 2) + "]");
            else
                builder.Append("]");

            return builder.ToString();
        }

        // Сериализует Dictionary.
        private static string SerializeDictionary(object obj, int level, bool formattingIndented)
        {
            StringBuilder builder = new StringBuilder();
            var tab = string.Empty;

            if (formattingIndented)
            {
                for (int i = 0; i < level; i++)
                    tab += "  ";

                builder.Append("{\r\n" + tab);
            }
            else
            {
                builder.Append("{");
            }

            bool isFirst = true;
            foreach (DictionaryEntry item in obj as IDictionary)
            {
                if (!isFirst)
                {
                    if (formattingIndented)
                        builder.Append(",\r\n" + tab);
                    else
                        builder.Append(",");
                }

                if (formattingIndented)
                    builder.Append($"{Serialize(item.Key is string ? item.Key : item.Key.ToString(), level + 1, formattingIndented)}: {Serialize(item.Value, level + 1, formattingIndented)}");
                else
                    builder.Append($"{Serialize(item.Key is string ? item.Key : item.Key.ToString(), level + 1, formattingIndented)}:{Serialize(item.Value, level + 1, formattingIndented)}");

                isFirst = false;
            }

            if (formattingIndented)
                builder.Append("\r\n" + tab.Substring(0, tab.Length - 2) + "}");
            else
                builder.Append("}");

            return builder.ToString();
        }
    }
}
