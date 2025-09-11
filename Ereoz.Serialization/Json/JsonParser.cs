using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Ereoz.Serialization.Json
{
    internal static class JsonParser
    {
        public static Dictionary<string, object> Deserialize(string json)
        {
            int index = 0;
            SkipWhitespace(json, ref index);
            return ParseObject(json, ref index);
        }

        private static Dictionary<string, object> ParseObject(string json, ref int index)
        {
            var obj = new Dictionary<string, object>();

            if (json[index] == '{')
            {
                index++; // Пропустить '{'
                SkipWhitespace(json, ref index);

                while (json[index] != '}')
                {
                    var key = ParseString(json, ref index);
                    SkipWhitespace(json, ref index);
                    index++; // Пропустить ':'
                    SkipWhitespace(json, ref index);
                    var value = ParseValue(json, ref index);
                    obj[key] = value;
                    SkipWhitespace(json, ref index);

                    if (json[index] == ',')
                    {
                        index++;
                        SkipWhitespace(json, ref index);
                    }
                }

                index++; // Пропустить '}'

            }
            else if (json[index] != '[')
            {
                var value = ParseValue(json, ref index);
                obj["0SimpleValue"] = value;
            }
            else
            {
                var value = ParseValue(json, ref index);
                obj["0Sequence"] = value;
            }

            return obj;
        }

        private static object[] ParseArray(string json, ref int index)
        {
            var array = new List<object>();
            index++; // Пропустить '['
            SkipWhitespace(json, ref index);

            while (json[index] != ']')
            {
                var value = ParseValue(json, ref index);
                array.Add(value);
                SkipWhitespace(json, ref index);

                if (json[index] == ',')
                {
                    index++;
                    SkipWhitespace(json, ref index);
                }
            }

            index++; // Пропустить ']'
            return array.ToArray();
        }

        private static object ParseValue(string json, ref int index)
        {
            SkipWhitespace(json, ref index);
            switch (json[index])
            {
                case '"': return ParseString(json, ref index);
                case '{': return ParseObject(json, ref index);
                case '[': return ParseArray(json, ref index);
                case 't': return ParseTrue(json, ref index);
                case 'f': return ParseFalse(json, ref index);
                case 'n': return ParseNull(json, ref index);
                default: return ParseNumber(json, ref index);
            }
        }

        private static string ParseString(string json, ref int index)
        {
            index++; // Пропустить открывающую "
            var sb = new StringBuilder();

            while (json[index] != '"')
            {
                if (json[index] == '\\')
                {
                    index++;
                    switch (json[index])
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            var hex = json.Substring(index + 1, 4);
                            sb.Append((char)Convert.ToInt32(hex, 16));
                            index += 4;
                            break;
                    }
                }
                else
                {
                    sb.Append(json[index]);
                }
                index++;
            }

            index++; // Пропустить закрывающую "
            return sb.ToString();
        }

        private static object ParseNumber(string json, ref int index)
        {
            var start = index;
            while (index < json.Length && "+-.0123456789eE".Contains(json[index].ToString()))
                index++;

            var number = json.Substring(start, index - start);

            if (int.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out int intResult))
                return intResult;

            if (uint.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out uint uintResult))
                return uintResult;

            if (long.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out long longResult))
                return longResult;

            if (ulong.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out ulong ulongResult))
                return ulongResult;

            if (decimal.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalResult))
                return decimalResult;

            if (float.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatResult))
                return floatResult;

            if (double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleResult))
                return doubleResult;

            throw new FormatException($"Invalid number format: {number}");
        }

        private static bool ParseTrue(string json, ref int index)
        {
            index += 4;
            return true;
        }

        private static bool ParseFalse(string json, ref int index)
        {
            index += 5;
            return false;
        }

        private static object ParseNull(string json, ref int index)
        {
            index += 4;
            return null;
        }

        private static void SkipWhitespace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
                index++;
        }
    }
}
