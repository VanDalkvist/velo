using System.Collections.Generic;
using AutoFixture.Xunit2;
using FluentAssertions;
using Newtonsoft.Json;
using Velo.Serialization;
using Xunit;

namespace Velo.Tests.Serialization.Converters
{
    public class ListConverterShould : DeserializationTests
    {
        private readonly IJsonConverter<List<int>> _converter;
        
        public ListConverterShould()
        {
            var converters = BuildConvertersCollection();
            _converter = converters.Get<List<int>>();
        }

        [Theory]
        [AutoData]
        public void DeserializeList(List<int> list)
        {
            var serialized = JsonConvert.SerializeObject(list);
            var deserialized = _converter.Deserialize(serialized);

            for (var i = 0; i < list.Count; i++)
            {
                deserialized[i].Should().Be(list[i]);
            }
        }

        [Theory]
        [AutoData]
        public void DeserializeIList(IList<int> list)
        {
            var serialized = JsonConvert.SerializeObject(list);
            var deserialized = _converter.Deserialize(serialized);

            for (var i = 0; i < list.Count; i++)
            {
                deserialized[i].Should().Be(list[i]);
            }
        }
    }
}