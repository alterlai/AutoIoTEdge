using AutoIoTEdge.Models;

namespace AutoIoTEdge.Test.TestModels;

public class TestModuleTwin : ModuleTwinBase
{
    public string StringProperty { get; set; } = "DefaultString";
    public int IntProperty { get; set; } = 42;
    public double DoubleProperty { get; set; } = 3.14;
    public bool BoolProperty { get; set; } = true;
    public DateTime DateTimeProperty { get; set; } = DateTime.MinValue;
    
    // List of strings property for testing list parsing
    public List<string> StringListProperty { get; set; } = new List<string> { "Default1", "Default2" };
    
    // Read-only property to test that it's not updated
    public string ReadOnlyProperty { get; } = "ReadOnly";
    
    // Property without setter to test filtering
    public string PropertyWithoutSetter => "NoSetter";
    
    // Static property to test that it's not included
    public static string StaticProperty { get; set; } = "Static";
}