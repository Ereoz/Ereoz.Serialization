using Ereoz.Abstractions.Serialization;
using Ereoz.Serialization.Json;
using Ereoz.Serialization.Tests.Sources;
using System.Collections.ObjectModel;

namespace Ereoz.Serialization.Tests
{
    public class SimpleJsonSerializationTests
    {
        IStringSerializer _simpleJson = new SimpleJson();

        [Fact]
        public void Serialization_SimpleTypes_ShouldCorrectString()
        {
            Assert.Equal("-128", _simpleJson.Serialize((sbyte)-128));
            Assert.Equal("255", _simpleJson.Serialize((byte)255));

            Assert.Equal("-32768", _simpleJson.Serialize((short)-32768));
            Assert.Equal("65535", _simpleJson.Serialize((ushort)65535));

            Assert.Equal("-2147483648", _simpleJson.Serialize((int)-2147483648));
            Assert.Equal("4294967295", _simpleJson.Serialize((uint)4294967295));

            Assert.Equal("-9223372036854775808", _simpleJson.Serialize((long)-9223372036854775808));
            Assert.Equal("18446744073709551615", _simpleJson.Serialize((ulong)18446744073709551615));

            Assert.Equal("-3.14", _simpleJson.Serialize((float)-3.14));
            Assert.Equal("3.14", _simpleJson.Serialize((double)3.14));
            Assert.Equal("3.14", _simpleJson.Serialize((decimal)3.14));

            Assert.Equal("\"This is \\\"Special\\\" string.\"", _simpleJson.Serialize("This is \"Special\" string."));
            Assert.Equal("\"A\"", _simpleJson.Serialize('A'));
            Assert.Equal("true", _simpleJson.Serialize(true));
            Assert.Equal("2", _simpleJson.Serialize(SomeEnum.Two));
        }

        [Fact]
        public void Serialization_Structures_ShouldCorrectString()
        {
            Assert.Equal("\"2025-03-05T11:18:32.512\"", _simpleJson.Serialize(new DateTime(2025, 03, 05, 11, 18, 32, 512)));
            Assert.Equal("\"5.11:18:32.5120000\"", _simpleJson.Serialize(new TimeSpan(5, 11, 18, 32, 512)));

            var someStruct = new SomeStruct()
            {
                Name = "John",
                Age = 100,
                NotSerializeProperty = "Some value"
            };

            var json = _simpleJson.Serialize(someStruct);

            Assert.Equal("{\"Name\":\"John\",\"Age\":100}", json);
        }

        [Fact]
        public void Serialization_Classes_ShouldCorrectString()
        {
            var someClass = new SomeClass()
            {
                Name = "John",
                Age = 100,
                NotSerializeProperty = "Some value"
            };

            var json = _simpleJson.Serialize(someClass);

            Assert.Equal("{\"Name\":\"John\",\"Age\":100}", json);
        }

        [Fact]
        public void Serialization_Sequences_ShouldCorrectString()
        {
            Assert.Equal("[0,64,128,255]", _simpleJson.Serialize(new byte[] { 0, 64, 128, 255 }));
            Assert.Equal("[0,64,128,255]", _simpleJson.Serialize(new List<byte> { 0, 64, 128, 255 }));
            Assert.Equal("[0,64,128,255]", _simpleJson.Serialize(new LinkedList<byte>(new List<byte> { 0, 64, 128, 255 })));
            Assert.Equal("[0,64,128,255]", _simpleJson.Serialize(new Collection<byte> { 0, 64, 128, 255 }));
            Assert.Equal("[0,64,128,255]", _simpleJson.Serialize(new ObservableCollection<byte> { 0, 64, 128, 255 }));
            Assert.Equal("[0,64,128,255]", _simpleJson.Serialize(new ReadOnlyCollection<byte>(new Collection<byte> { 0, 64, 128, 255 })));
            Assert.Equal("[0,64,128,255]", _simpleJson.Serialize(new HashSet<byte> { 0, 64, 128, 255 }));

            var queue = new Queue<byte>();
            var stack = new Stack<byte>();

            queue.Enqueue(0);
            queue.Enqueue(64);
            queue.Enqueue(128);
            queue.Enqueue(255);

            stack.Push(0);
            stack.Push(64);
            stack.Push(128);
            stack.Push(255);

            Assert.Equal("[0,64,128,255]", _simpleJson.Serialize(queue));
            Assert.Equal("[255,128,64,0]", _simpleJson.Serialize(stack));
        }

        [Fact]
        public void Serialization_KeyValuePairs_ShouldCorrectString()
        {
            
            var dictionary = new Dictionary<string, int> { { "One", 1 }, { "Two", 2 }, { "Three", 3 } };

            Assert.Equal("{\"One\":1,\"Two\":2,\"Three\":3}", _simpleJson.Serialize(dictionary));
        }

        [Fact]
        public void Serialization_Tuple_ShouldCorrectString()
        {
            var tuple = new Tuple<int, string, float>(1, "Two", 3.14f);

            Assert.Equal("{\"Item1\":1,\"Item2\":\"Two\",\"Item3\":3.14}", _simpleJson.Serialize(tuple));
        }
    }
}
