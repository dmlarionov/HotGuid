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
			//Act
			var sortedList = SortedGuidList.OrderBy(x => x);
			//Assert
			Assert.True(SortedGuidList.SequenceEqual(sortedList));
		}

		[Fact]
		private void TestSqlGuidSorting()
		{
			//Act
			var sortedList = SortedSqlGuidList.OrderBy(x => x);
			//Assert
			Assert.True(SortedSqlGuidList.SequenceEqual(sortedList));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestHotGuidNewGuid(uint shardKey)
		{
			//Arrange
			var generator = HotGuidGenerator.Instance;
			var items = Enumerable.Range(0, 25).Select(i => new { Id = generator.NewGuid(shardKey), Sort = i });
			//Act
			var sortedItems = items.OrderBy(x => x.Id).ToList();
			//Assert
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
		[InlineData(4294967295)]
		private void TestHotGuidNewSqlGuid(uint shardKey)
		{
			//Arrange
			var generator = HotSqlGuidGenerator.Instance;
			var items = Enumerable.Range(0, 25).Select(i => new { Id = new SqlGuid(generator.NewGuid(shardKey)), Sort = i });
			//Act
			var sortedItems = items.OrderBy(x => x.Id).ToList();
			//Assert
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
		[InlineData(4294967295)]
		private void TestLocalDateIsUtcInGuid(uint shardKey)
		{
			var localNow = DateTime.Now;
			TestLocalDateIsUtcInGuidImpl(localNow,
				HotGuidGenerator.Instance.NewGuid(localNow, shardKey));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestLocalDateIsUtcInSqlGuid(uint shardKey)
		{
			var localNow = DateTime.Now;
			TestLocalDateIsUtcInGuidImpl(localNow,
				HotGuidGenerator.Instance.NewGuid(localNow, shardKey));
		}

		// ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
		private static void TestLocalDateIsUtcInGuidImpl(DateTime localNow,
			Guid id)
		{
			// Act
			var utcDate = id.ToDateTime().GetValueOrDefault();
			// Assert
			Assert.Equal(DateTimeKind.Utc, utcDate.Kind);
			Assert.Equal(localNow, utcDate.ToLocalTime());
		}

		[Fact]
		private void TestSqlGuidToGuid()
		{
			// Act
			var sqlList = SortedSqlGuidList.Select(g => g.ToGuid());
			// Assert
			Assert.True(SortedGuidList.SequenceEqual(sqlList));
		}

		[Fact]
		private void TestGuidToSqlGuid()
		{
			// Act
			var guidList = SortedGuidList.Select(g => g.ToSqlGuid());
			// Assert
			Assert.True(SortedSqlGuidList.SequenceEqual(guidList));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestGuidToDateTimeIsUtc(uint shardKey)
		{
			//Arrange
			var expectedDateTime = DateTime.UtcNow;
			//Act
			var dateTime = HotGuidGenerator.Instance
				.NewGuid(expectedDateTime, shardKey)
				.ToDateTime()
				.GetValueOrDefault();
			//Assert
			Assert.Equal(TicksToSeconds(expectedDateTime.Ticks), TicksToSeconds(dateTime.Ticks));
			Assert.Equal(expectedDateTime.Kind, dateTime.Kind);
		}

		[Fact]
		private void TestGuidToDateTimeForNonSequentialGuidReturnsNull()
		{
			//Arrange
			var guid = Guid.NewGuid();
			//Act
			var actual = guid.ToDateTime();
			//Assert
			Assert.Null(actual);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestUtcNowDoesNotThrowException(uint shardKey)
		{
			HotGuidGenerator.Instance.NewGuid(DateTime.UtcNow, shardKey);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestLocalNowDoesNotThrowException(uint shardKey)
		{
			HotGuidGenerator.Instance.NewGuid(DateTime.Now, shardKey);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestUnixEpochDoesNotThrowException(uint shardKey)
		{
			HotGuidGenerator.Instance.NewGuid(EpochTicks, shardKey);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestBetweenUnixEpochAndNowDoesNotThrowException(uint shardKey)
		{
			HotGuidGenerator.Instance.NewGuid(
				new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), shardKey);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestDateTimeKindUnspecifiedThrowsArgumentException(uint shardKey)
		{
			TestThrowsArgumentException(new DateTime(2000, 1, 1), shardKey);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestAfterNowThrowsArgumentException(uint shardKey)
		{
			TestThrowsArgumentException(DateTime.UtcNow.AddSeconds(1), shardKey);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestAfterNowReturnsNullDateTime(uint shardKey)
		{
			TestReturnsNullDateTime(DateTime.UtcNow.AddSeconds(1).Ticks, shardKey);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestBeforeUnixEpochThrowsArgumentException(uint shardKey)
		{
			TestThrowsArgumentException(
				new DateTime(EpochTicks - 1), shardKey);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestBeforeUnixEpochReturnsNullDateTime(uint shardKey)
		{
			TestReturnsNullDateTime(EpochTicks - 1, shardKey);
		}

		// Test the internal mechanism that bypasses date validation
		private static void TestReturnsNullDateTime(long ticks, uint shardKey)
		{
			//Arrange
			var guid = HotGuidGenerator.Instance.NewGuid(ticks, shardKey);
			var sqlGuid = HotGuidGenerator.Instance.NewGuid(ticks, shardKey);
			//Act & Assert
			Assert.Null(guid.ToDateTime());
			Assert.Null(sqlGuid.ToDateTime());
		}

		private static void TestThrowsArgumentException(DateTime timestamp, uint shardKey)
		{
			Assert.Throws<ArgumentException>(() =>
				HotGuidGenerator.Instance.NewGuid(timestamp, shardKey));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestSqlGuidToDateTime(uint shardKey)
		{
			//Arrange
			var expectedDateTime = DateTime.UtcNow;
			//Act
			var dateTime = HotSqlGuidGenerator.Instance.NewGuid(expectedDateTime, shardKey).ToDateTime()
				.GetValueOrDefault();
			//Assert
			Assert.Equal(expectedDateTime.Ticks, dateTime.Ticks);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestGuidBigDateRange(uint shardKey)
		{
			//Arrange
			var generator = HotGuidGenerator.Instance;
			var items = new List<Guid>();
			//Act
			for (var i = 1970 + 1; i < DateTime.Today.Year; i++)    // testing for 1970 is unreliable, because it depends on local timezone
			{
				items.Add(generator.NewGuid(new DateTime(i, 1, 1, 0, 0, 0,
					DateTimeKind.Local), shardKey));
			}
			//Assert
			Assert.True(items.SequenceEqual(items.OrderBy(x => x)));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestSqlGuidBigDateRange(uint shardKey)
		{
			//Arrange
			var generator = HotSqlGuidGenerator.Instance;
			var items = new List<SqlGuid>();
			//Act
			for (var i = 1970; i < DateTime.Today.Year; i++)
			{
				items.Add(generator.NewGuid(new DateTime(i, 1, 1, 0, 0, 0,
					DateTimeKind.Utc), shardKey));
			}

			//Assert
			Assert.True(items.SequenceEqual(items.OrderBy(x => x)));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestSqlGuidGenerator(uint shardKey)
		{
			// Arrange
			var now = DateTime.UtcNow;
			// Act
			var stamp = HotSqlGuidGenerator.Instance.NewSqlGuid(now, shardKey).ToDateTime();
			// Assert
			Assert.Equal(now, stamp);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestExtractShardKeyFromGuid(uint shardKey)
		{
			//Arrange
			var generator = HotGuidGenerator.Instance;
			//Act
			var item = generator.NewGuid(shardKey);
			//Assert
			Assert.Equal(shardKey, item.ExtractShardKey());
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		[InlineData(4294967295)]
		private void TestExtractShardKeyFromSqlGuid(uint shardKey)
		{
			//Arrange
			var generator = HotSqlGuidGenerator.Instance;
			//Act
			var item = generator.NewSqlGuid(shardKey);
			//Assert
			Assert.Equal(shardKey, item.ExtractShardKey());
		}

		/// <summary>
		/// Covert ticks to seconds since Unix Epoch
		/// </summary>
		/// <param name="ticks">Ticks</param>
		/// <returns>Seconds</returns>
		private static int TicksToSeconds(long ticks) => (int)(ticks >> 32);
	}
}