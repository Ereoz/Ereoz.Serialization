using Ereoz.Abstractions.Serialization;
using Ereoz.Serialization.Json;
using Ereoz.Serialization.Tests.Sources;
using System.Collections.ObjectModel;

namespace Ereoz.Serialization.Tests
{
    public class SimpleJsonDeserializationTests
    {
        IStringSerializer _simpleJson = new SimpleJson();

        [Fact]
        public void Deserialization_SimpleTypes_ShouldCorrectString()
        {
            var sbyteJson = _simpleJson.Serialize((sbyte)-128);
            var byteJson = _simpleJson.Serialize((byte)255);

            var shortJson = _simpleJson.Serialize((short)-32768);
            var ushortJson = _simpleJson.Serialize((ushort)65535);

            var intJson = _simpleJson.Serialize((int)-2147483648);
            var uintJson = _simpleJson.Serialize((uint)4294967295);

            var longJson = _simpleJson.Serialize((long)-9223372036854775808);
            var ulongJson = _simpleJson.Serialize((ulong)18446744073709551615);

            var floatJson = _simpleJson.Serialize((float)-3.14);
            var doubleJson = _simpleJson.Serialize((double)3.14);
            var decimalJson = _simpleJson.Serialize((decimal)3.14);

            var stringJson = _simpleJson.Serialize("This is \"Special\" string.");
            var charJson = _simpleJson.Serialize('A');
            var boolJson = _simpleJson.Serialize(true);
            var enumJson = _simpleJson.Serialize(SomeEnum.Two);

            Assert.Equal((sbyte)-128, _simpleJson.Deserialize<sbyte>(sbyteJson));
            Assert.Equal((byte)255, _simpleJson.Deserialize<byte>(byteJson));

            Assert.Equal((short)-32768, _simpleJson.Deserialize<short>(shortJson));
            Assert.Equal((ushort)65535, _simpleJson.Deserialize<ushort>(ushortJson));

            Assert.Equal((int)-2147483648, _simpleJson.Deserialize<int>(intJson));
            Assert.Equal((uint)4294967295, _simpleJson.Deserialize<uint>(uintJson));

            Assert.Equal((long)-9223372036854775808, _simpleJson.Deserialize<long>(longJson));
            Assert.Equal((ulong)18446744073709551615, _simpleJson.Deserialize<ulong>(ulongJson));

            Assert.Equal((float)-3.14, _simpleJson.Deserialize<float>(floatJson));
            Assert.Equal((double)3.14, _simpleJson.Deserialize<double>(doubleJson));
            Assert.Equal((decimal)3.14, _simpleJson.Deserialize<decimal>(decimalJson));

            Assert.Equal("This is \"Special\" string.", _simpleJson.Deserialize<string>(stringJson));
            Assert.Equal('A', _simpleJson.Deserialize<char>(charJson));
            Assert.True(_simpleJson.Deserialize<bool>(boolJson));
            Assert.Equal(SomeEnum.Two, _simpleJson.Deserialize<SomeEnum>(enumJson));
        }

        [Fact]
        public void Deserialization_Structures_ShouldCorrectString()
        {
            var someStruct = new SomeStruct()
            {
                Name = "John",
                Age = 100,
                NotSerializeProperty = "Some value"
            };

            var dateTime = new DateTime(2025, 03, 05, 11, 18, 32, 512);
            var timeSpan = new TimeSpan(5, 11, 18, 32, 512);

            var someStructJson = _simpleJson.Serialize(someStruct);
            var dateTimeJson = _simpleJson.Serialize(dateTime);
            var timeSpanJson = _simpleJson.Serialize(timeSpan);

            var someStructDeserialize = _simpleJson.Deserialize<SomeStruct>(someStructJson);
            var dateTimeDeserialize = _simpleJson.Deserialize<DateTime>(dateTimeJson);
            var timeSpanDeserialize = _simpleJson.Deserialize<TimeSpan>(timeSpanJson);

            Assert.Equal(someStruct.Name, someStructDeserialize.Name);
            Assert.Equal(someStruct.Age, someStructDeserialize.Age);
            Assert.NotEqual(someStruct.NotSerializeProperty, someStructDeserialize.NotSerializeProperty);

            Assert.Equal(dateTime.Year, dateTimeDeserialize.Year);
            Assert.Equal(dateTime.Month, dateTimeDeserialize.Month);
            Assert.Equal(dateTime.Day, dateTimeDeserialize.Day);
            Assert.Equal(dateTime.Hour, dateTimeDeserialize.Hour);
            Assert.Equal(dateTime.Minute, dateTimeDeserialize.Minute);
            Assert.Equal(dateTime.Second, dateTimeDeserialize.Second);
            Assert.Equal(dateTime.Millisecond, dateTimeDeserialize.Millisecond);

            Assert.Equal(timeSpan.Days, timeSpanDeserialize.Days);
            Assert.Equal(timeSpan.Hours, timeSpanDeserialize.Hours);
            Assert.Equal(timeSpan.Minutes, timeSpanDeserialize.Minutes);
            Assert.Equal(timeSpan.Seconds, timeSpanDeserialize.Seconds);
            Assert.Equal(timeSpan.Milliseconds, timeSpanDeserialize.Milliseconds);
        }

        [Fact]
        public void Deserialization_Classes_ShouldCorrectString()
        {
            var someClass = new SomeClass()
            {
                Name = "John",
                Age = 100,
                NotSerializeProperty = "Some value"
            };

            var someClassJson = _simpleJson.Serialize(someClass);

            var someClassDeserialize = _simpleJson.Deserialize<SomeClass>(someClassJson);

            Assert.Equal(someClass.Name, someClassDeserialize.Name);
            Assert.Equal(someClass.Age, someClassDeserialize.Age);
            Assert.NotEqual(someClass.NotSerializeProperty, someClassDeserialize.NotSerializeProperty);
        }

        [Fact]
        public void Deserialization_Sequences_ShouldCorrectString()
        {
            var array = new byte[] { 0, 64, 128, 255 };
            var list = new List<byte> { 0, 64, 128, 255 };
            var linkedlist = new LinkedList<byte>(new List<byte> { 0, 64, 128, 255 });
            var collection = new Collection<byte> { 0, 64, 128, 255 };
            var observableCollection = new ObservableCollection<byte> { 0, 64, 128, 255 };
            var readOnlyCollection = new ReadOnlyCollection<byte>(new Collection<byte> { 0, 64, 128, 255 });
            var hashSet = new HashSet<byte> { 0, 64, 128, 255 };            
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

            var arrayJson = _simpleJson.Serialize(array);
            var listJson = _simpleJson.Serialize(list);
            var linkedListJson = _simpleJson.Serialize(linkedlist);
            var collectionJson = _simpleJson.Serialize(collection);
            var observableCollectionJson = _simpleJson.Serialize(observableCollection);
            var readOnlyCollectionJson = _simpleJson.Serialize(readOnlyCollection);
            var hashSetJson = _simpleJson.Serialize(hashSet);            
            var queueJson = _simpleJson.Serialize(queue);
            var stackJson = _simpleJson.Serialize(stack);

            Assert.True(array.SequenceEqual(_simpleJson.Deserialize<byte[]>(arrayJson)));
            Assert.True(list.SequenceEqual(_simpleJson.Deserialize<List<byte>>(listJson)));
            Assert.True(linkedlist.SequenceEqual(_simpleJson.Deserialize<LinkedList<byte>>(linkedListJson)));
            Assert.True(collection.SequenceEqual(_simpleJson.Deserialize<Collection<byte>>(collectionJson)));
            Assert.True(observableCollection.SequenceEqual(_simpleJson.Deserialize<ObservableCollection<byte>>(observableCollectionJson)));
            Assert.True(readOnlyCollection.SequenceEqual(_simpleJson.Deserialize<ReadOnlyCollection<byte>>(readOnlyCollectionJson)));
            Assert.True(hashSet.SequenceEqual(_simpleJson.Deserialize<HashSet<byte>>(hashSetJson)));
            Assert.True(queue.SequenceEqual(_simpleJson.Deserialize<Queue<byte>>(queueJson)));
            Assert.True(stack.SequenceEqual(_simpleJson.Deserialize<Stack<byte>>(stackJson).Reverse()));
        }

        [Fact]
        public void Deserialization_KeyValuePairs_ShouldCorrectString()
        {
            var dictionary = new Dictionary<string, int> { { "One", 1 }, { "Two", 2 }, { "Three", 3 } };

            var dictionaryJson = _simpleJson.Serialize(dictionary);

            Assert.True(dictionary.SequenceEqual(_simpleJson.Deserialize<Dictionary<string, int>>(dictionaryJson)));
        }

        [Fact]
        public void Deserialization_Tuple_ShouldCorrectString()
        {
            var tuple = new Tuple<int, string, float>(1, "Two", 3.14f);

            var tupleJson = _simpleJson.Serialize(tuple);

            var tupleDeserialize = _simpleJson.Deserialize<Tuple<int, string, float>>(tupleJson);

            Assert.Equal(tuple.Item1, tupleDeserialize.Item1);
            Assert.Equal(tuple.Item2, tupleDeserialize.Item2);
            Assert.Equal(tuple.Item3, tupleDeserialize.Item3);
        }
    }
}
