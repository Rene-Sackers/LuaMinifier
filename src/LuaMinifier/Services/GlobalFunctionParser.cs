using System;
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

		public IEnumerable<LuaFunction> ParseGlobalFunctions(string lua)
		{
			var cursorIndex = 0;
			return ParseInnerFunction(lua, null, ref cursorIndex);
		}

		private IEnumerable<LuaFunction> ParseInnerFunction(string lua, LuaFunction parentFunction, ref int cursorIndex)
		{
			var parsedFunctions = new List<LuaFunction>();

			LuaFunction function = null;

			for (; cursorIndex < lua.Length; cursorIndex++)
			{
				var substring = lua.Substring(cursorIndex);
				var functionKeywordMatch = FunctionRegex.Match(substring);

				if (functionKeywordMatch.Success)
				{
					function = FunctionMatchToLuaFunction(cursorIndex, functionKeywordMatch);

					cursorIndex += functionKeywordMatch.Groups[0].Value.Length;

					function.ParentFunction = parentFunction;

					parsedFunctions.Add(function);
					parsedFunctions.AddRange(ParseInnerFunction(lua, function, ref cursorIndex));
				}

				var endKeywordMatch = EndRegex.Match(substring);

				if (!endKeywordMatch.Success) continue;

				if (function == null)
				{
					cursorIndex--;
					return parsedFunctions;
				}

				cursorIndex += endKeywordMatch.Groups[0].Value.Length - 1;

				function.LuaString = lua.Substring(function.StartIndex, cursorIndex - function.StartIndex + 1).Trim();

				// Step out of current function
				function = null;
			}

			cursorIndex--;
			return parsedFunctions;
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
