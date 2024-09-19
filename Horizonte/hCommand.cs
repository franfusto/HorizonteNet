using System.Reflection;

namespace Horizonte
{

	public class HCommand
	{
		public string CommandKey { get; set; } = string.Empty;
		public MethodInfo? CommandAction { get; set; }
		public object Instance { get; set; } = new();
		public string Description { get; set; } = string.Empty;
		public Type? OutType { get; set; }
		public List<Type>? InTypes { get; set; } = new List<Type>();
		public List<string>? InNames { get; set; } = new List<string>();
		public List<string> Roles { get; set; } = new List<string>();
	}




}