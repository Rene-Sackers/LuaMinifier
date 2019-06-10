using System.Linq;
using LuaMinifier.Models;

namespace LuaMinifier.Services
{
	internal class LuaParserService
	{
		private readonly GlobalFunctionParser _globalFunctionParser;

		public LuaParserService(GlobalFunctionParser globalFunctionParser)
		{
			_globalFunctionParser = globalFunctionParser;
		}

		public LuaDocument Parse(string lua)
		{
			return new LuaDocument
			{
				GlobalFunctions = _globalFunctionParser.ParseGlobalFunctions(lua).ToList()
			};
		}
	}
}
