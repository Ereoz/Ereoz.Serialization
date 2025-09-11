using System;

namespace Ereoz.Serialization.Json
{
    /// <summary>
    /// Indicates that a property should not be serialized.
    /// This attribute is used to exclude specific properties from the serialization process,
    /// for example, if the property contains temporary data, sensitive information,
    /// or if its serialization does not make sense for your particular serializer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NonSerializedAttribute : Attribute
    {
    }
}
