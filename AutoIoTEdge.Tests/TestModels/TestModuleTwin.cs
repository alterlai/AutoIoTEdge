using AutoIoTEdge.Models;

namespace AutoIoTEdge.Tests.TestModels;

public class TestModuleTwin : ModuleTwinBase
{
    public string StringProperty { get; set; } = "DefaultString";
    public int IntProperty { get; set; } = 42;
    public double DoubleProperty { get; set; } = 3.14;
    public bool BoolProperty { get; set; } = true;
    public DateTime DateTimeProperty { get; set; } = DateTime.MinValue;
    
    // Read-only property to test that it's not updated
    public string ReadOnlyProperty { get; } = "ReadOnly";
    
    // Property without setter to test filtering
    public string PropertyWithoutSetter => "NoSetter";
}