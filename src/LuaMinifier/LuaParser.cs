using Autofac;
using LuaMinifier.Models;
using LuaMinifier.Services;

namespace LuaMinifier
{
	public class LuaParser
	{
		public LuaDocument Parse(string lua)
		{
			var builder = new ContainerBuilder();

			builder.RegisterAssemblyTypes(GetType().Assembly)
				.InNamespaceOf<LuaParserService>()
				.AsImplementedInterfaces()
				.AsSelf();

			var container = builder.Build();

			var parser = container.Resolve<LuaParserService>();

			return parser.Parse(lua);
		}
	}
}
