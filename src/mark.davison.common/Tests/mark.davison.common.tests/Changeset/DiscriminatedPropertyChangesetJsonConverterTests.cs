using mark.davison.common.Changeset;
using System.Text.Json;

namespace mark.davison.common.tests.Changeset;

[TestClass]
public class DiscriminatedPropertyChangesetJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public DiscriminatedPropertyChangesetJsonConverterTests()
    {
        _options = SerializationHelpers.CreateStandardSerializationOptions();
    }

    private class TestEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public DateOnly Date { get; set; }
    }

    private void ValdiateOriginalAgainstDeserialised(
        DiscriminatedPropertyChangeset original,
        DiscriminatedPropertyChangeset? deserialised
        )
    {
        Assert.IsNotNull(deserialised);

        Assert.AreEqual(original.Name, deserialised.Name);
        Assert.AreEqual(original.PropertyType, deserialised.PropertyType);
        Assert.AreEqual(original.Value, deserialised.Value);
    }

    [TestMethod]
    public void SerialiseAndDeserialise_WorksForBooleanProperty()
    {
        var entity = new TestEntity
        {
            IsAdmin = true
        };

        DiscriminatedPropertyChangeset cs = new()
        {
            PropertyType = typeof(bool).FullName!,
            Name = nameof(TestEntity.IsAdmin),
            Value = entity.IsAdmin
        };

        var serialised = JsonSerializer.Serialize(cs, _options);

        Assert.IsFalse(string.IsNullOrEmpty(serialised));

        var deserialized = JsonSerializer.Deserialize<DiscriminatedPropertyChangeset>(serialised, _options);

        ValdiateOriginalAgainstDeserialised(cs, deserialized);
    }

    [TestMethod]
    public void SerialiseAndDeserialise_WorksForGuidProperty()
    {
        var entity = new TestEntity
        {
            Id = Guid.NewGuid()
        };

        DiscriminatedPropertyChangeset cs = new()
        {
            PropertyType = typeof(Guid).FullName!,
            Name = nameof(TestEntity.Id),
            Value = entity.Id
        };

        var serialised = JsonSerializer.Serialize(cs, _options);

        Assert.IsFalse(string.IsNullOrEmpty(serialised));

        var deserialized = JsonSerializer.Deserialize<DiscriminatedPropertyChangeset>(serialised, _options);

        ValdiateOriginalAgainstDeserialised(cs, deserialized);
    }

    [TestMethod]
    public void SerialiseAndDeserialise_WorksForStringProperty()
    {
        var entity = new TestEntity
        {
            Name = Guid.NewGuid().ToString()
        };

        DiscriminatedPropertyChangeset cs = new()
        {
            PropertyType = typeof(string).FullName!,
            Name = nameof(TestEntity.Name),
            Value = entity.Name
        };

        var serialised = JsonSerializer.Serialize(cs, _options);

        Assert.IsFalse(string.IsNullOrEmpty(serialised));

        var deserialized = JsonSerializer.Deserialize<DiscriminatedPropertyChangeset>(serialised, _options);

        ValdiateOriginalAgainstDeserialised(cs, deserialized);
    }

    [TestMethod]
    public void SerialiseAndDeserialise_WorksForDateOnlyProperty()
    {
        var entity = new TestEntity
        {
            Date = DateOnly.FromDateTime(DateTime.Now)
        };

        DiscriminatedPropertyChangeset cs = new()
        {
            PropertyType = typeof(DateOnly).FullName!,
            Name = nameof(TestEntity.Date),
            Value = entity.Date
        };

        var serialised = JsonSerializer.Serialize(cs, _options);

        Assert.IsFalse(string.IsNullOrEmpty(serialised));

        var deserialized = JsonSerializer.Deserialize<DiscriminatedPropertyChangeset>(serialised, _options);

        ValdiateOriginalAgainstDeserialised(cs, deserialized);
    }
}
