using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;
using Xunit;

namespace HotGuid.Tests
{
	public class HotGuidTests
	{
		private const long EpochTicks = 621355968000000000;

		/// <summary>
		///     Properly sequenced Guid array
		/// </summary>
		private IReadOnlyList<Guid> SortedGuidList { get; } =
			new ReadOnlyCollection<Guid>(new List<Guid>
			{
			new("00000000-0000-0000-0000-000000000001"),
			new("00000000-0000-0000-0000-000000000100"),
			new("00000000-0000-0000-0000-000000010000"),
			new("00000000-0000-0000-0000-000001000000"),
			new("00000000-0000-0000-0000-000100000000"),
			new("00000000-0000-0000-0000-010000000000"),
			new("00000000-0000-0000-0001-000000000000"),
			new("00000000-0000-0000-0100-000000000000"),
			new("00000000-0000-0001-0000-000000000000"),
			new("00000000-0000-0100-0000-000000000000"),
			new("00000000-0001-0000-0000-000000000000"),
			new("00000000-0100-0000-0000-000000000000"),
			new("00000001-0000-0000-0000-000000000000"),
			new("00000100-0000-0000-0000-000000000000"),
			new("00010000-0000-0000-0000-000000000000"),
			new("01000000-0000-0000-0000-000000000000")
			});

		/// <summary>
		///     Properly sequenced SqlGuid array
		/// </summary>
		private IReadOnlyList<SqlGuid> SortedSqlGuidList { get; } =
			new ReadOnlyCollection<SqlGuid>(new List<SqlGuid>
			{
			new("01000000-0000-0000-0000-000000000000"),
			new("00010000-0000-0000-0000-000000000000"),
			new("00000100-0000-0000-0000-000000000000"),
			new("00000001-0000-0000-0000-000000000000"),
			new("00000000-0100-0000-0000-000000000000"),
			new("00000000-0001-0000-0000-000000000000"),
			new("00000000-0000-0100-0000-000000000000"),
			new("00000000-0000-0001-0000-000000000000"),
			new("00000000-0000-0000-0001-000000000000"),
			new("00000000-0000-0000-0100-000000000000"),
			new("00000000-0000-0000-0000-000000000001"),
			new("00000000-0000-0000-0000-000000000100"),
			new("00000000-0000-0000-0000-000000010000"),
			new("00000000-0000-0000-0000-000001000000"),
			new("00000000-0000-0000-0000-000100000000"),
			new("00000000-0000-0000-0000-010000000000")
			});

		[Fact]
		private void TestGuidSorting()
		{
			// Act
			var sortedList = SortedGuidList.OrderBy(x => x);

			// Assert
			Assert.True(SortedGuidList.SequenceEqual(sortedList));
		}

		[Fact]
		private void TestSqlGuidSorting()
		{
			// Act
			var sortedList = SortedSqlGuidList.OrderBy(x => x);

			// Assert
			Assert.True(SortedSqlGuidList.SequenceEqual(sortedList));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(32465535)]
		[InlineData(4294967295)]
		private void TestHotGuidSorting(uint shardKey)
		{
			// Arrange
			var generator = HotGuidGenerator.Instance;
			var items = Enumerable.Range(0, 25).Select(i => new { Id = generator.NewGuid(shardKey), Sort = i });

			// Act
			var sortedItems = items.OrderBy(x => x.Id).ToList();

			// Assert
			for (var i = 0; i < sortedItems.Count; i++)
			{
				Assert.Equal(i, sortedItems[i].Sort);
			}
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(32465535)]
		[InlineData(4294967295)]
		private void TestHotGuidSqlSorting(uint shardKey)
		{
			// Arrange
			var generator = HotGuidGenerator.Instance;
			var items = Enumerable.Range(0, 25).Select(i => new { Id = generator.NewGuid(shardKey).ToSqlGuid(), Sort = i });

			// Act
			var sortedItems = items.OrderBy(x => x.Id).ToList();

			// Assert
			for (var i = 0; i < sortedItems.Count; i++)
			{
				Assert.Equal(i, sortedItems[i].Sort);
			}
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(32465535)]
		[InlineData(4294967295)]
		private void TestHotGuidTwiceConvertedSorting(uint shardKey)
		{
			// Arrange
			var generator = HotGuidGenerator.Instance;
			var items = Enumerable.Range(0, 25).Select(i => new { Id = generator.NewGuid(shardKey).ToSqlGuid().ToGuid(), Sort = i });

			// Act
			var sortedItems = items.OrderBy(x => x.Id).ToList();

			// Assert
			for (var i = 0; i < sortedItems.Count; i++)
			{
				Assert.Equal(i, sortedItems[i].Sort);
			}
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(32465535)]
		[InlineData(4294967295)]
		private void TestHotGuidUnixTimestamp(uint shardKey)
		{
			// Arrange
			var generator = HotGuidGenerator.Instance;

			// Act
			var before = DateTime.UtcNow;
			var item = generator.NewGuid(shardKey);
			var after = DateTime.UtcNow;
			var beforeTimestamp = before.Ticks / TimeSpan.TicksPerSecond - (new DateTime(1970, 1, 1)).Ticks / TimeSpan.TicksPerSecond;
			var itemTimestamp = item.ExtractUnixTimestamp();
			var afterTimestamp = after.Ticks / TimeSpan.TicksPerSecond - (new DateTime(1970, 1, 1)).Ticks / TimeSpan.TicksPerSecond;

			// Assert
			Assert.True(itemTimestamp >= beforeTimestamp);
			Assert.True(itemTimestamp <= afterTimestamp);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(32465535)]
		[InlineData(4294967295)]
		private void TestHotGuidExtractShardKey(uint shardKey)
		{
			// Arrange
			var generator = HotGuidGenerator.Instance;

			// Act
			var item = generator.NewGuid(shardKey);

			// Assert
			Assert.Equal(shardKey, item.ExtractShardKey());
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(32465535)]
		[InlineData(4294967295)]
		private void TestStepOver20380119031407(uint shardKey)
		{
			// Arrange
			var generator = HotGuidGenerator.Instance;

			// Act
			var item0 = generator.NewGuid(shardKey, new DateTime(2038, 1, 19, 3, 14, 6));
			var item1 = generator.NewGuid(shardKey, new DateTime(2038, 1, 19, 3, 14, 7));
			var item2 = generator.NewGuid(shardKey, new DateTime(2038, 1, 19, 3, 14, 8));
			var item3 = generator.NewGuid(shardKey, new DateTime(2038, 1, 19, 3, 14, 9));

			// Assert 
			Assert.Equal((uint)2147483646, item0.ExtractUnixTimestamp());
			Assert.Equal((uint)2147483647, item1.ExtractUnixTimestamp());	// int.MaxValue
			Assert.Equal((uint)2147483648, item2.ExtractUnixTimestamp());
			Assert.Equal((uint)2147483649, item3.ExtractUnixTimestamp());
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(32465535)]
		[InlineData(4294967295)]
		private void TestStepOver21060207062815(uint shardKey)
		{
			// Arrange
			var generator = HotGuidGenerator.Instance;

			// Act
			var item0 = generator.NewGuid(shardKey, new DateTime(2106, 2, 7, 6, 28, 14));
			var item1 = generator.NewGuid(shardKey, new DateTime(2106, 2, 7, 6, 28, 15));
			var item2 = generator.NewGuid(shardKey, new DateTime(2106, 2, 7, 6, 28, 16));
			var item3 = generator.NewGuid(shardKey, new DateTime(2106, 2, 7, 6, 28, 17));

			// Assert 
			Assert.Equal((uint)4294967294, item0.ExtractUnixTimestamp());
			Assert.Equal((uint)4294967295, item1.ExtractUnixTimestamp());	// uint.MaxValue
			Assert.Equal((uint)0, item2.ExtractUnixTimestamp());
			Assert.Equal((uint)1, item3.ExtractUnixTimestamp());
		}
	}
}