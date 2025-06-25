using AutoIoTEdge.Models;

namespace ExampleApp.Models;
public class ModuleTwin : ModuleTwinBase
{
	public static string TestVariable { get; set; } = "DefaultString";
}
