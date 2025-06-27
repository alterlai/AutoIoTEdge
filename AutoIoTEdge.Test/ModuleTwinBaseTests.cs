using AutoIoTEdge.Test.TestModels;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AutoIoTEdge.Test;

[TestFixture]
public class ModuleTwinBaseTests
{
    private TestModuleTwin _testTwin;

    [SetUp]
    public void Setup()
    {
        _testTwin = new TestModuleTwin();
        // Reset static property to default value before each test
        TestModuleTwin.StaticProperty = "Static";
    }

    #region UpdateFromTwin Tests

    [Test]
    public void UpdateFromTwin_WithValidTwinCollection_UpdatesProperties()
    {
        // Arrange
        var twinCollection = new TwinCollection();
        twinCollection["StringProperty"] = "UpdatedString";
        twinCollection["IntProperty"] = 100;
        twinCollection["DoubleProperty"] = 6.28;
        twinCollection["BoolProperty"] = false;
        twinCollection["DateTimeProperty"] = new DateTime(2024, 1, 1);

        // Act
        _testTwin.UpdateFromTwin(twinCollection);

        // Assert
        Assert.That(_testTwin.StringProperty, Is.EqualTo("UpdatedString"));
        Assert.That(_testTwin.IntProperty, Is.EqualTo(100));
        Assert.That(_testTwin.DoubleProperty, Is.EqualTo(6.28));
        Assert.That(_testTwin.BoolProperty, Is.EqualTo(false));
        Assert.That(_testTwin.DateTimeProperty, Is.EqualTo(new DateTime(2024, 1, 1)));
    }

    [Test]
    public void UpdateFromTwin_WithStaticProperty_UpdatesStaticProperty()
    {
        // Arrange
        var originalStaticValue = TestModuleTwin.StaticProperty;
        var twinCollection = new TwinCollection();
        twinCollection["StaticProperty"] = "UpdatedStatic";
        twinCollection["StringProperty"] = "UpdatedString";

        // Act
        _testTwin.UpdateFromTwin(twinCollection);

        // Assert
        Assert.That(TestModuleTwin.StaticProperty, Is.EqualTo("UpdatedStatic"));
        Assert.That(_testTwin.StringProperty, Is.EqualTo("UpdatedString"));
    }

    [Test]
    public void UpdateFromTwin_WithNullValues_SkipsNullProperties()
    {
        // Arrange
        var originalStringValue = _testTwin.StringProperty;
        var originalStaticValue = TestModuleTwin.StaticProperty;
        var twinCollection = new TwinCollection();
        twinCollection["StringProperty"] = null;
        twinCollection["StaticProperty"] = null;
        twinCollection["IntProperty"] = 200;

        // Act
        _testTwin.UpdateFromTwin(twinCollection);

        // Assert
        Assert.That(_testTwin.StringProperty, Is.EqualTo(originalStringValue)); // Should remain unchanged
        Assert.That(TestModuleTwin.StaticProperty, Is.EqualTo(originalStaticValue)); // Should remain unchanged
        Assert.That(_testTwin.IntProperty, Is.EqualTo(200)); // Should be updated
    }

    [Test]
    public void UpdateFromTwin_WithNonExistentProperty_IgnoresProperty()
    {
        // Arrange
        var twinCollection = new TwinCollection();
        twinCollection["NonExistentProperty"] = "SomeValue";
        twinCollection["StringProperty"] = "UpdatedString";

        // Act & Assert - Should not throw exception
        Assert.DoesNotThrow(() => _testTwin.UpdateFromTwin(twinCollection));
        Assert.That(_testTwin.StringProperty, Is.EqualTo("UpdatedString"));
    }

    [Test]
    public void UpdateFromTwin_WithReadOnlyProperty_DoesNotUpdate()
    {
        // Arrange
        var originalReadOnlyValue = _testTwin.ReadOnlyProperty;
        var twinCollection = new TwinCollection();
        twinCollection["ReadOnlyProperty"] = "NewValue";

        // Act
        _testTwin.UpdateFromTwin(twinCollection);

        // Assert
        Assert.That(_testTwin.ReadOnlyProperty, Is.EqualTo(originalReadOnlyValue));
    }

    [Test]
    public void UpdateFromTwin_WithEmptyTwinCollection_DoesNotChangeProperties()
    {
        // Arrange
        var originalStringValue = _testTwin.StringProperty;
        var originalIntValue = _testTwin.IntProperty;
        var originalStaticValue = TestModuleTwin.StaticProperty;
        var twinCollection = new TwinCollection();

        // Act
        _testTwin.UpdateFromTwin(twinCollection);

        // Assert
        Assert.That(_testTwin.StringProperty, Is.EqualTo(originalStringValue));
        Assert.That(_testTwin.IntProperty, Is.EqualTo(originalIntValue));
        Assert.That(TestModuleTwin.StaticProperty, Is.EqualTo(originalStaticValue));
    }

    [Test]
    public void UpdateFromTwin_WithTypeConversion_ConvertsCorrectly()
    {
        // Arrange
        var twinCollection = new TwinCollection();
        twinCollection["IntProperty"] = "123"; // String that can be converted to int
        twinCollection["BoolProperty"] = "true"; // String that can be converted to bool

        // Act
        _testTwin.UpdateFromTwin(twinCollection);

        // Assert
        Assert.That(_testTwin.IntProperty, Is.EqualTo(123));
        Assert.That(_testTwin.BoolProperty, Is.EqualTo(true));
    }

    [Test]
    public void UpdateFromTwin_WithInvalidTypeConversion_ThrowsException()
    {
        // Arrange
        var twinCollection = new TwinCollection();
        twinCollection["IntProperty"] = "NotANumber";

        // Act & Assert
        Assert.Throws<FormatException>(() => _testTwin.UpdateFromTwin(twinCollection));
    }

    #endregion

    #region UpdateFromConfiguration Tests

    [Test]
    public void UpdateFromConfiguration_WithValidConfiguration_UpdatesProperties()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            {"StringProperty", "ConfigString"},
            {"IntProperty", "999"},
            {"DoubleProperty", "9,99"},
            {"BoolProperty", "false"},
            {"DateTimeProperty", "2023-12-25T00:00:00"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        _testTwin.UpdateFromConfiguration(config);

        // Assert
        Assert.That(_testTwin.StringProperty, Is.EqualTo("ConfigString"));
        Assert.That(_testTwin.IntProperty, Is.EqualTo(999));
        Assert.That(_testTwin.DoubleProperty, Is.EqualTo(9.99));
        Assert.That(_testTwin.BoolProperty, Is.EqualTo(false));
        Assert.That(_testTwin.DateTimeProperty, Is.EqualTo(new DateTime(2023, 12, 25)));
    }

    [Test]
    public void UpdateFromConfiguration_WithStaticProperty_UpdatesStaticProperty()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            {"StaticProperty", "ConfigStatic"},
            {"StringProperty", "ConfigString"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        _testTwin.UpdateFromConfiguration(config);

        // Assert
        Assert.That(TestModuleTwin.StaticProperty, Is.EqualTo("ConfigStatic"));
        Assert.That(_testTwin.StringProperty, Is.EqualTo("ConfigString"));
    }

    [Test]
    public void UpdateFromConfiguration_WithNullValues_SkipsNullProperties()
    {
        // Arrange
        var originalStringValue = _testTwin.StringProperty;
        var originalStaticValue = TestModuleTwin.StaticProperty;
        var configData = new Dictionary<string, string>
        {
            {"IntProperty", "500"}
            // StringProperty and StaticProperty are not included (null)
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        _testTwin.UpdateFromConfiguration(config);

        // Assert
        Assert.That(_testTwin.StringProperty, Is.EqualTo(originalStringValue)); // Should remain unchanged
        Assert.That(TestModuleTwin.StaticProperty, Is.EqualTo(originalStaticValue)); // Should remain unchanged
        Assert.That(_testTwin.IntProperty, Is.EqualTo(500)); // Should be updated
    }

    [Test]
    public void UpdateFromConfiguration_WithReadOnlyProperty_DoesNotUpdate()
    {
        // Arrange
        var originalReadOnlyValue = _testTwin.ReadOnlyProperty;
        var configData = new Dictionary<string, string>
        {
            {"ReadOnlyProperty", "NewValue"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        _testTwin.UpdateFromConfiguration(config);

        // Assert
        Assert.That(_testTwin.ReadOnlyProperty, Is.EqualTo(originalReadOnlyValue));
    }

    [Test]
    public void UpdateFromConfiguration_WithEmptyConfiguration_DoesNotChangeProperties()
    {
        // Arrange
        var originalStringValue = _testTwin.StringProperty;
        var originalIntValue = _testTwin.IntProperty;
        var originalStaticValue = TestModuleTwin.StaticProperty;
        var config = new ConfigurationBuilder().Build();

        // Act
        _testTwin.UpdateFromConfiguration(config);

        // Assert
        Assert.That(_testTwin.StringProperty, Is.EqualTo(originalStringValue));
        Assert.That(_testTwin.IntProperty, Is.EqualTo(originalIntValue));
        Assert.That(TestModuleTwin.StaticProperty, Is.EqualTo(originalStaticValue));
    }

    [Test]
    public void UpdateFromConfiguration_WithInvalidTypeConversion_ThrowsException()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            {"IntProperty", "NotANumber"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act & Assert
        Assert.Throws<FormatException>(() => _testTwin.UpdateFromConfiguration(config));
    }

    #endregion

    #region ToTwinCollection Tests

    [Test]
    public void ToTwinCollection_ReturnsAllPublicInstanceAndStaticProperties()
    {
        // Arrange
        _testTwin.StringProperty = "TestString";
        _testTwin.IntProperty = 555;
        _testTwin.DoubleProperty = 5.55;
        _testTwin.BoolProperty = false;
        _testTwin.DateTimeProperty = new DateTime(2023, 6, 15);
        TestModuleTwin.StaticProperty = "TestStatic";

        // Act
        var twinCollection = _testTwin.ToTwinCollection();

        // Assert
        Assert.That(twinCollection["StringProperty"].Value, Is.EqualTo("TestString"));
        Assert.That(twinCollection["IntProperty"].Value, Is.EqualTo(555));
        Assert.That(twinCollection["DoubleProperty"].Value, Is.EqualTo(5.55));
        Assert.That(twinCollection["BoolProperty"].Value, Is.EqualTo(false));
        Assert.That(twinCollection["DateTimeProperty"].Value, Is.EqualTo(new DateTime(2023, 6, 15)));
        Assert.That(twinCollection["ReadOnlyProperty"].Value, Is.EqualTo("ReadOnly"));
        Assert.That(twinCollection["PropertyWithoutSetter"].Value, Is.EqualTo("NoSetter"));
        Assert.That(twinCollection["StaticProperty"].Value, Is.EqualTo("TestStatic"));
    }

    [Test]
    public void ToTwinCollection_IncludesStaticProperties()
    {
        // Arrange
        TestModuleTwin.StaticProperty = "IncludedStatic";

        // Act
        var twinCollection = _testTwin.ToTwinCollection();

        // Assert
        Assert.That(twinCollection.Contains("StaticProperty"), Is.True);
        Assert.That(twinCollection["StaticProperty"].Value, Is.EqualTo("IncludedStatic"));
    }

    [Test]
    public void ToTwinCollection_WithNullPropertyValues_IncludesNullValues()
    {
        // Arrange
        _testTwin.StringProperty = null;

        // Act
        var twinCollection = _testTwin.ToTwinCollection();

        // Assert
        Assert.That(twinCollection.Contains("StringProperty"), Is.True);
        Assert.That(twinCollection["StringProperty"].Value, Is.Null);
    }

    [Test]
    public void ToTwinCollection_ReturnsNewInstanceEachTime()
    {
        // Act
        var twinCollection1 = _testTwin.ToTwinCollection();
        var twinCollection2 = _testTwin.ToTwinCollection();

        // Assert
        Assert.That(twinCollection1, Is.Not.SameAs(twinCollection2));
        Assert.That(twinCollection1["StringProperty"], Is.EqualTo(twinCollection2["StringProperty"]));
    }

    #endregion

    #region Integration Tests

    [Test]
    public void UpdateFromTwinAndToTwinCollection_RoundTrip_PreservesData()
    {
        // Arrange
        var originalTwinCollection = new TwinCollection();
        originalTwinCollection["StringProperty"] = "RoundTripString";
        originalTwinCollection["IntProperty"] = 777;
        originalTwinCollection["DoubleProperty"] = 7.77;
        originalTwinCollection["BoolProperty"] = true;
        originalTwinCollection["DateTimeProperty"] = new DateTime(2024, 3, 15);
        originalTwinCollection["StaticProperty"] = "RoundTripStatic";

        // Act
        _testTwin.UpdateFromTwin(originalTwinCollection);
        var resultTwinCollection = _testTwin.ToTwinCollection();

        // Assert
        Assert.That(resultTwinCollection["StringProperty"], Is.EqualTo("RoundTripString"));
        Assert.That(resultTwinCollection["IntProperty"], Is.EqualTo(777));
        Assert.That(resultTwinCollection["DoubleProperty"], Is.EqualTo(7.77));
        Assert.That(resultTwinCollection["BoolProperty"], Is.EqualTo(true));
        Assert.That(resultTwinCollection["DateTimeProperty"], Is.EqualTo(new DateTime(2024, 3, 15)));
        Assert.That(resultTwinCollection["StaticProperty"], Is.EqualTo("RoundTripStatic"));
    }

    [Test]
    public void UpdateFromConfigurationAndToTwinCollection_RoundTrip_PreservesData()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            {"StringProperty", "ConfigRoundTrip"},
            {"IntProperty", "888"},
            {"DoubleProperty", "8,88"},
            {"BoolProperty", "false"},
            {"DateTimeProperty", "2024-04-20T12:30:45"},
            {"StaticProperty", "ConfigStaticRoundTrip"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        _testTwin.UpdateFromConfiguration(config);
        var twinCollection = _testTwin.ToTwinCollection();

        // Assert
        Assert.That(twinCollection["StringProperty"].Value, Is.EqualTo("ConfigRoundTrip"));
        Assert.That(twinCollection["IntProperty"].Value, Is.EqualTo(888));
        Assert.That(twinCollection["DoubleProperty"].Value, Is.EqualTo(8.88));
        Assert.That(twinCollection["BoolProperty"].Value, Is.EqualTo(false));
        Assert.That(twinCollection["DateTimeProperty"].Value, Is.EqualTo(new DateTime(2024, 4, 20, 12, 30, 45)));
        Assert.That(twinCollection["StaticProperty"].Value, Is.EqualTo("ConfigStaticRoundTrip"));
    }

    [Test]
    public void UpdateFromTwin_WithMixedInstanceAndStaticProperties_UpdatesBoth()
    {
        // Arrange
        var twinCollection = new TwinCollection();
        twinCollection["StringProperty"] = "UpdatedInstance";
        twinCollection["StaticProperty"] = "UpdatedStatic";

        // Act
        _testTwin.UpdateFromTwin(twinCollection);

        // Assert
        Assert.That(_testTwin.StringProperty, Is.EqualTo("UpdatedInstance"));
        Assert.That(TestModuleTwin.StaticProperty, Is.EqualTo("UpdatedStatic"));
    }

    [Test]
    public void UpdateFromConfiguration_WithMixedInstanceAndStaticProperties_UpdatesBoth()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            {"StringProperty", "ConfigInstance"},
            {"StaticProperty", "ConfigStatic"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        _testTwin.UpdateFromConfiguration(config);

        // Assert
        Assert.That(_testTwin.StringProperty, Is.EqualTo("ConfigInstance"));
        Assert.That(TestModuleTwin.StaticProperty, Is.EqualTo("ConfigStatic"));
    }

    #endregion
}