using System.Collections.Generic;

namespace LuaMinifier.Models
{
	public class LuaDocument
	{
		public ICollection<LuaFunction> GlobalFunctions { get; set; }
	}
}
