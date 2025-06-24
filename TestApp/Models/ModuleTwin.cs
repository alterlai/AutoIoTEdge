using AutoIoTEdge.Models;

namespace ExampleApp.Models;
public class ModuleTwin : ModuleTwinBase
{
	public string TestVariable { get; set; } = "DefaultString";
}
