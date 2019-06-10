using System.Collections.Generic;

namespace LuaMinifier.Models
{
	public class LuaFunction
	{
		private LuaFunction _parentFunction;

		public string Name { get; set; }

		public ICollection<LuaArgument> Arguments { get; set; } = new List<LuaArgument>();

		public string LuaString { get; set; }

		public int StartIndex { get; set; }

		public bool IsLocal { get; set; }

		public LuaFunction ParentFunction
		{
			get => _parentFunction;
			set
			{
				_parentFunction = value;
				Depth = CalculateDepth();
			}
		}

		public ICollection<LuaFunction> ChildFunctions { get; set; } = new List<LuaFunction>();

		public int Depth { get; private set; } = 0;

		public LuaFunction(string name)
		{
			Name = name;
		}

		private int CalculateDepth()
		{
			var parent = ParentFunction;
			var depth = -1;

			do
			{
				depth++;
				parent = parent?.ParentFunction;
			} while (parent != null);

			return depth;
		}
	}
}