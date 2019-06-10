using System;
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

		[TestMethod]
		public void ParsesGlobalFunction()
		{
			var inputLua = "" +
				"function myFunction()\n" +
				"end";

			var result = _service.ParseGlobalFunctions(inputLua).ToList();

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
		public void ParsesVariableStyleGlobalFunction()
		{
			var inputLua = "" +
				"myFunction = function ()\n" +
				"end";

			var result = _service.ParseGlobalFunctions(inputLua).ToList();

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
			var inputLua = "" +
				"function myFunction(argument1, argument2)\n" +
				"end";

			var result = _service.ParseGlobalFunctions(inputLua).ToList();

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
			var inputLua = "" +
				"function myFunction1()\n" +
				"end\n" +
				"function myFunction2()\n" +
				"end";

			var result = _service.ParseGlobalFunctions(inputLua).ToList();

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
			var inputLua = "" +
				"function myFunction1()\n" +
				"	function myFunction2() end\n" +
				"end";

			var result = _service.ParseGlobalFunctions(inputLua).ToList();

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

			result.Should().BeEquivalentTo(expectedFunction1, expectedFunction2);
		}
	}
}
