using System;

namespace LuaMinifier.Models
{
	public class SyntaxError : Exception
	{
		public SyntaxError(int position, string message)
			:base($"Syntax error at column {position}, error: {message}")
		{

		}
	}
}
