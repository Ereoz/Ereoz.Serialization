namespace Ereoz.Serialization.Tests.Sources
{
    public class SomeClass
    {
        public string? Name { get; set; }
        public int Age { get; set; }

        [Ereoz.Serialization.Json.NonSerialized]
        public string? NotSerializeProperty { get; set; }
    }
}
