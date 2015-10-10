using Core.Memory;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Core.Tests.Memory
{
	public class MemoryListenerTests
	{
		[Theory]
		[InlineData("", "", true)]
		[InlineData("", "testing", false)]
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
		public void When_matching_a_routing_key(string pattern, string key, bool matches)
		{
			//nyse.tech.msft taken from:
			//http://spring.io/blog/2010/06/14/understanding-amqp-the-protocol-used-by-rabbitmq/

			var tree = MemoryListener.CreateExpressionTree(pattern);

			MemoryListener.Matches(tree, key).ShouldBe(matches);
		}
	}
}
