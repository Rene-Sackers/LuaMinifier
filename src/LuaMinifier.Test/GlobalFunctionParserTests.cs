using System.Linq;
using Autofac;
using FluentAssertions;
using FluffySpoon.Testing.Autofake;
using LuaMinifier.Models;
using LuaMinifier.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuaMinifier.Test
{
	[TestClass]
	public class GlobalFunctionParserTests
	{
		private GlobalFunctionParser _service;
		private IContainer _container;

		[TestInitialize]
		public void Initialize()
		{
			_service = new GlobalFunctionParser();

			var builder = new ContainerBuilder();

			var faker = new Autofaker();
			faker.UseAutofac(builder);
			faker.UseNSubstitute();

			faker.RegisterFakesForConstructorParameterTypesOf<GlobalFunctionParserTests>();

			_container = builder.Build();
		}

		[TestCleanup]
		public void Cleanup()
		{
			_container.Dispose();
		}

		[TestMethod]
		public void ParsesGlobalFunction()
		{
			const string inputLua = "" +
				"function myFunction()\n" +
				"end";

			var result = _service.ParseFunctions(inputLua).ToList();

			var expected = new LuaFunction("myFunction")
			{
				LuaString = "function myFunction()\n" +
					"end",
				StartIndex = 0,
				Name = "myFunction",
				IsLocal = false,
				ParentFunction = null,
				Arguments = new LuaArgument[0]
			};

			result.Should().BeEquivalentTo(expected);
		}

		[TestMethod]
		public void ParsesLocalGlobalFunction()
		{
			const string inputLua = "" +
				"local function myFunction()\n" +
				"end";

			var result = _service.ParseFunctions(inputLua).ToList();

			var expected = new LuaFunction("myFunction")
			{
				LuaString = "local function myFunction()\n" +
					"end",
				StartIndex = 0,
				Name = "myFunction",
				IsLocal = true,
				ParentFunction = null,
				Arguments = new LuaArgument[0]
			};

			result.Should().BeEquivalentTo(expected);
		}

		[TestMethod]
		public void ParsesVariableStyleGlobalFunction()
		{
			const string inputLua = "" +
				"myFunction = function ()\n" +
				"end";

			var result = _service.ParseFunctions(inputLua).ToList();

			var expected = new LuaFunction("myFunction")
			{
				LuaString = "myFunction = function ()\n" +
					"end",
				StartIndex = 0,
				Name = "myFunction",
				IsLocal = false,
				ParentFunction = null,
				Arguments = new LuaArgument[0]
			};

			result.Should().BeEquivalentTo(expected);
		}

		[TestMethod]
		public void ParsesGlobalFunctionWithArguments()
		{
			const string inputLua = "" +
				"function myFunction(argument1, argument2)\n" +
				"end";

			var result = _service.ParseFunctions(inputLua).ToList();

			var expectedArguments = new[]
			{
				new LuaArgument("argument1"),
				new LuaArgument("argument2"),
			};

			var expected = new LuaFunction("myFunction")
			{
				Arguments = expectedArguments,
				LuaString = "function myFunction(argument1, argument2)\n" +
					"end",
				StartIndex = 0,
				Name = "myFunction",
				IsLocal = false,
				ParentFunction = null,
			};

			result.Should().BeEquivalentTo(expected);
		}

		[TestMethod]
		public void ParsesMultipleGlobalFunction()
		{
			const string inputLua = "" +
				"function myFunction1()\n" +
				"end\n" +
				"function myFunction2()\n" +
				"end";

			var result = _service.ParseFunctions(inputLua).ToList();

			var expectedFunction1 = new LuaFunction("myFunction1")
			{
				LuaString = "function myFunction1()\n" +
					"end",
				StartIndex = 0,
				Name = "myFunction1",
				IsLocal = false,
				ParentFunction = null,
			};

			var expectedFunction2 = new LuaFunction("myFunction2")
			{
				LuaString = "function myFunction2()\n" +
					"end",
				StartIndex = 27,
				Name = "myFunction2",
				IsLocal = false,
				ParentFunction = null,
			};

			result.Should().BeEquivalentTo(expectedFunction1, expectedFunction2);
		}

		[TestMethod]
		public void ParsesNestedFunctions()
		{
			const string inputLua = "" +
				"function myFunction1()\n" +
				"	function myFunction2() end\n" +
				"end";

			var result = _service.ParseFunctions(inputLua).ToList();

			var expectedFunction1 = new LuaFunction("myFunction1")
			{
				LuaString = "" +
					"function myFunction1()\n" +
					"	function myFunction2() end\n" +
					"end",
				StartIndex = 0,
				Name = "myFunction1",
				IsLocal = false,
				ParentFunction = null,
			};

			var expectedFunction2 = new LuaFunction("myFunction2")
			{
				LuaString = "" +
					"function myFunction2() end",
				StartIndex = 24,
				Name = "myFunction2",
				IsLocal = false,
				ParentFunction = expectedFunction1,
			};

			expectedFunction1.ChildFunctions.Add(expectedFunction2);

			result.Should().BeEquivalentTo(new [] { expectedFunction1 }, options => options.IgnoringCyclicReferences());
		}

		[TestMethod]
		public void ParsesVariableStyleNestedFunctions()
		{
			const string inputLua = "" +
				"myFunction1 = function()\n" +
				"	myFunction2 = function() end\n" +
				"end";

			var result = _service.ParseFunctions(inputLua).ToList();

			var expectedFunction1 = new LuaFunction("myFunction1")
			{
				LuaString = "" +
					"myFunction1 = function()\n" +
					"	myFunction2 = function() end\n" +
					"end",
				StartIndex = 0,
				Name = "myFunction1",
				IsLocal = false,
				ParentFunction = null,
			};

			var expectedFunction2 = new LuaFunction("myFunction2")
			{
				LuaString = "" +
					"myFunction2 = function() end",
				StartIndex = 26,
				Name = "myFunction2",
				IsLocal = false,
				ParentFunction = expectedFunction1,
			};

			expectedFunction1.ChildFunctions.Add(expectedFunction2);

			result.Should().BeEquivalentTo(new[] { expectedFunction1 }, options => options.IgnoringCyclicReferences());
		}

		[TestMethod]
		public void NestedLocalFunctionNotParsedAsGlobal()
		{
			const string inputLua = "" +
				"function myFunction1()\n" +
				"	local function myFunction2() end\n" +
				"end";

			var result = _service.ParseFunctions(inputLua).ToList();

			var expectedFunction1 = new LuaFunction("myFunction1")
			{
				LuaString = "" +
					"function myFunction1()\n" +
					"	local function myFunction2() end\n" +
					"end",
				StartIndex = 0,
				Name = "myFunction1",
				IsLocal = false,
				ParentFunction = null,
			};

			var expectedFunction2 = new LuaFunction("myFunction2")
			{
				LuaString = "" +
					"local function myFunction2() end",
				StartIndex = 24,
				Name = "myFunction2",
				IsLocal = true,
				ParentFunction = expectedFunction1,
			};

			expectedFunction1.ChildFunctions.Add(expectedFunction2);

			result.Should().BeEquivalentTo(new [] { expectedFunction1 }, options => options.IgnoringCyclicReferences());
		}

		[TestMethod]
		public void NestedGlobalFunctionParsedAsGlobal()
		{
			const string inputLua = "" +
				"function myFunction1()\n" +
				"	function myFunction2() end\n" +
				"end";

			var parsedFunctions = _service.ParseFunctions(inputLua).ToList();
			var globalFunctionsResult = _service.GlobalFunctions(parsedFunctions);

			var expectedFunction1 = new LuaFunction("myFunction1")
			{
				LuaString = "" +
					"function myFunction1()\n" +
					"	function myFunction2() end\n" +
					"end",
				StartIndex = 0,
				Name = "myFunction1",
				IsLocal = false,
				ParentFunction = null,
			};

			var expectedFunction2 = new LuaFunction("myFunction2")
			{
				LuaString = "" +
					"function myFunction2() end",
				StartIndex = 24,
				Name = "myFunction2",
				IsLocal = false,
				ParentFunction = expectedFunction1,
			};

			expectedFunction1.ChildFunctions.Add(expectedFunction2);

			globalFunctionsResult.Should().BeEquivalentTo(new [] { expectedFunction1, expectedFunction2 }, options => options.IgnoringCyclicReferences());
		}
	}
}
