using AutoIoTEdge.Models;

namespace SampleModule.Models;
public class ModuleTwin : ModuleTwinBase
{
	public static string TestVariable { get; set; } = "DefaultString";
	public static Position Position { get; set; } = new Position();
}
