using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Memory;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Core.Tests.Memory
{
	public class MemoryListenerTests
	{
		private readonly ITestOutputHelper _output;

		public MemoryListenerTests(ITestOutputHelper output)
		{
			_output = output;
		}

		[Theory]
		[InlineData("Some.Matching.Pattern", "Some.Matching.Pattern", true)]
		[InlineData("Some.Matching.Pattern", "Some.NonMatching.Key", false)]
		[InlineData("Some.*.Pattern", "Some.Matching.Pattern", true)]
		[InlineData("Some.*.Pattern", "Some.NonMatching.Key", false)]
		[InlineData("Some.*.Pattern", "Some.NonMatching.Pattern.Key", false)]
		[InlineData("Some.#.Pattern", "Some.NonMatching.Pattern.Key", false)]
		[InlineData("NYSE.TECH.MSFT", "NYSE.TECH.MSFT", true)]
		[InlineData("#", "NYSE.TECH.MSFT", true)]
		[InlineData("NYSE.#", "NYSE.TECH.MSFT", true)]
		[InlineData("*.*", "NYSE.TECH.MSFT", false)]
		[InlineData("NYSE.*", "NYSE.TECH.MSFT", false)]
		[InlineData("NYSE.TECH.*", "NYSE.TECH.MSFT", true)]
		[InlineData("NYSE.*.MSFT", "NYSE.TECH.MSFT", true)]
		public void TheoryMethodName(string pattern, string key, bool matches)
		{
			//nyse.tech.msft taken from:
			//http://spring.io/blog/2010/06/14/understanding-amqp-the-protocol-used-by-rabbitmq/

			MemoryListener.Matches(pattern, key).ShouldBe(matches);
		}
	}
}
