﻿using System;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Core.Tests
{
	public class Scratchpad
	{
		private readonly ITestOutputHelper _output;

		public Scratchpad(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void When_testing_something()
		{
		}
	}
}
