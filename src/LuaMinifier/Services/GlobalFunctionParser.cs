using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LuaMinifier.Models;

namespace LuaMinifier.Services
{
	public class GlobalFunctionParser
	{
		private static readonly Regex FunctionRegex = new Regex(@"(?:^|-)(?<local>local )?(function\s*(?<name>\w+)?\s*|(?<name2>\w+)\s*=\s*function)\s*\((?<arguments>[^\)]*)\)(?:\s|-|$)",  RegexOptions.Compiled);
		private static readonly Regex EndRegex = new Regex(@"(?:^|-)end(?:\s|-|$)",  RegexOptions.Compiled);

		public IEnumerable<LuaFunction> ParseFunctions(string lua)
		{
			var cursorIndex = 0;
			var allFunctions = ParseInnerFunction(lua, null, ref cursorIndex);
			return allFunctions;
		}

		public ICollection<LuaFunction> GlobalFunctions(IEnumerable<LuaFunction> parsedRootFunctions)
		{
			return GetGlobalFunctions(parsedRootFunctions);
		}

		private static ICollection<LuaFunction> GetGlobalFunctions(IEnumerable<LuaFunction> functions)
		{
			var globalFunctions = new List<LuaFunction>();

			foreach (var function in functions)
			{
				// 1. Functions in root are always global, even if defined as local
				// 2. Nested functions *NOT* defined as local are global
				if (function.ParentFunction == null || !function.IsLocal)
				{
					globalFunctions.Add(function);
				}

				globalFunctions.AddRange(GetGlobalFunctions(function.ChildFunctions));
			}

			return globalFunctions;
		}

		private static IEnumerable<LuaFunction> ParseInnerFunction(string lua, LuaFunction parentFunction, ref int cursorIndex)
		{
			var functions = new List<LuaFunction>();

			LuaFunction function = null;

			for (; cursorIndex < lua.Length; cursorIndex++)
			{
				var substring = lua.Substring(cursorIndex);
				var functionKeywordMatch = FunctionRegex.Match(substring);

				if (functionKeywordMatch.Success)
				{
					function = FunctionMatchToLuaFunction(cursorIndex, functionKeywordMatch);
					function.ParentFunction = parentFunction;

					cursorIndex += functionKeywordMatch.Groups[0].Value.Length;

					// Parse nested functions
					function.ChildFunctions = ParseInnerFunction(lua, function, ref cursorIndex).ToList();
				}

				var endKeywordMatch = EndRegex.Match(substring);

				if (!endKeywordMatch.Success) continue;

				if (function == null)
				{
					cursorIndex--;
					return functions;
				}

				cursorIndex += endKeywordMatch.Groups[0].Value.Length - 1;

				function.LuaString = lua.Substring(function.StartIndex, cursorIndex - function.StartIndex + 1).Trim();

				// Step out of current function
				functions.Add(function);
				function = null;
			}

			return functions;
		}

		private static LuaFunction FunctionMatchToLuaFunction(int startIndex, Match functionKeywordMatch)
		{
			var isLocalFunction = functionKeywordMatch.Groups["local"].Value == "local ";
			var functionName = !string.IsNullOrWhiteSpace(functionKeywordMatch.Groups["name"].Value) ? functionKeywordMatch.Groups["name"].Value : functionKeywordMatch.Groups["name2"].Value;

			var currentFunction = new LuaFunction(functionName)
			{
				StartIndex = startIndex,
				IsLocal = isLocalFunction
			};

			var arguments = functionKeywordMatch.Groups["arguments"].Value
				.Split(',')
				.Select(a => a.Trim(' '))
				.Where(a => !string.IsNullOrWhiteSpace(a))
				.Select(a => new LuaArgument(a))
				.ToList();

			currentFunction.Arguments = arguments;

			return currentFunction;
		}
	}
}
