using System;

namespace Ereoz.Serialization.Json
{
    internal static class JsonDeserializer
    {
        internal static T Deserialize<T>(string jsonString)
        {
            var virtualObject = JsonParser.Deserialize(jsonString);

            return TargetConverter.Convert<T>(virtualObject);
        }

        internal static object Deserialize(Type targetType, string jsonString)
        {
            var virtualObject = JsonParser.Deserialize(jsonString);

            return TargetConverter.Convert(targetType, virtualObject);
        }
    }
}
