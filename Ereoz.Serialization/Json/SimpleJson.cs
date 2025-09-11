using Ereoz.Abstractions.Serialization;
using System;

namespace Ereoz.Serialization.Json
{
    /// <summary>
    /// Provides JSON serialization and deserialization capabilities.
    /// This service implements the <see cref="IStringSerializer"/> interface.
    /// </summary>
    public class SimpleJson : IStringSerializer
    {

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="formattingIndented">A value indicating whether to format the JSON string with indentation.</param>
        /// <returns>A JSON string representing the serialized object.</returns>
        public string Serialize<T>(T obj, bool formattingIndented = false) =>
            JsonSerializer.Serialize(obj, 1, formattingIndented);

        /// <summary>
        /// Deserializes a JSON string into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="jsonString">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public T Deserialize<T>(string jsonString) =>
            JsonDeserializer.Deserialize<T>(jsonString);

        /// <summary>
        /// Deserializes a JSON string into an object of the specified target type.
        /// </summary>
        /// <param name="jsonString">The JSON string to deserialize.</param>
        /// <param name="targetType">The type of the object to deserialize to.</param>
        /// <returns></returns>
        public object Deserialize(string jsonString, Type targetType) =>
            JsonDeserializer.Deserialize(targetType, jsonString);
    }
}
