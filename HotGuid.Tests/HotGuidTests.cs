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
		private void TestHotGuidNewGuid(ushort shardCode)
		{
			//Arrange
			var generator = HotGuidGenerator.Instance;
			var items = Enumerable.Range(0, 25).Select(i => new { Id = generator.NewGuid(shardCode), Sort = i });
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
		private void TestHotGuidNewSqlGuid(ushort shardCode)
		{
			//Arrange
			var generator = HotSqlGuidGenerator.Instance;
			var items = Enumerable.Range(0, 25).Select(i => new { Id = new SqlGuid(generator.NewGuid(shardCode)), Sort = i });
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
		private void TestLocalDateIsUtcInGuid(ushort shardCode)
		{
			var localNow = DateTime.Now;
			TestLocalDateIsUtcInGuidImpl(localNow,
				HotGuidGenerator.Instance.NewGuid(localNow, shardCode));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestLocalDateIsUtcInSqlGuid(ushort shardCode)
		{
			var localNow = DateTime.Now;
			TestLocalDateIsUtcInGuidImpl(localNow,
				HotGuidGenerator.Instance.NewGuid(localNow, shardCode));
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
		private void TestGuidToDateTimeIsUtc(ushort shardCode)
		{
			//Arrange
			var expectedDateTime = DateTime.UtcNow;
			//Act
			var dateTime = HotGuidGenerator.Instance
				.NewGuid(expectedDateTime, shardCode)
				.ToDateTime()
				.GetValueOrDefault();
			//Assert
			Assert.Equal(expectedDateTime.Ticks, dateTime.Ticks);
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
		private void TestUtcNowDoesNotThrowException(ushort shardCode)
		{
			HotGuidGenerator.Instance.NewGuid(DateTime.UtcNow, shardCode);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestLocalNowDoesNotThrowException(ushort shardCode)
		{
			HotGuidGenerator.Instance.NewGuid(DateTime.Now, shardCode);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestUnixEpochDoesNotThrowException(ushort shardCode)
		{
			HotGuidGenerator.Instance.NewGuid(EpochTicks, shardCode);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestBetweenUnixEpochAndNowDoesNotThrowException(ushort shardCode)
		{
			HotGuidGenerator.Instance.NewGuid(
				new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), shardCode);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestDateTimeKindUnspecifiedThrowsArgumentException(ushort shardCode)
		{
			TestThrowsArgumentException(new DateTime(2000, 1, 1), shardCode);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestAfterNowThrowsArgumentException(ushort shardCode)
		{
			TestThrowsArgumentException(DateTime.UtcNow.AddSeconds(1), shardCode);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestAfterNowReturnsNullDateTime(ushort shardCode)
		{
			TestReturnsNullDateTime(DateTime.UtcNow.AddSeconds(1).Ticks, shardCode);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestBeforeUnixEpochThrowsArgumentException(ushort shardCode)
		{
			TestThrowsArgumentException(
				new DateTime(EpochTicks - 1), shardCode);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestBeforeUnixEpochReturnsNullDateTime(ushort shardCode)
		{
			TestReturnsNullDateTime(EpochTicks - 1, shardCode);
		}

		// Test the internal mechanism that bypasses date validation
		private static void TestReturnsNullDateTime(long ticks, ushort shardCode)
		{
			//Arrange
			var guid = HotGuidGenerator.Instance.NewGuid(ticks, shardCode);
			var sqlGuid = HotGuidGenerator.Instance.NewGuid(ticks, shardCode);
			//Act & Assert
			Assert.Null(guid.ToDateTime());
			Assert.Null(sqlGuid.ToDateTime());
		}

		private static void TestThrowsArgumentException(DateTime timestamp, ushort shardCode)
		{
			Assert.Throws<ArgumentException>(() =>
				HotGuidGenerator.Instance.NewGuid(timestamp, shardCode));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestSqlGuidToDateTime(ushort shardCode)
		{
			//Arrange
			var expectedDateTime = DateTime.UtcNow;
			//Act
			var dateTime = HotSqlGuidGenerator.Instance.NewGuid(expectedDateTime, shardCode).ToDateTime()
				.GetValueOrDefault();
			//Assert
			Assert.Equal(expectedDateTime.Ticks, dateTime.Ticks);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestGuidBigDateRange(ushort shardCode)
		{
			//Arrange
			var generator = HotGuidGenerator.Instance;
			var items = new List<Guid>();
			//Act
			for (var i = 1970+1; i < DateTime.Today.Year; i++)	// testing for 1970 is unreliable, because it depends on local timezone
			{
				items.Add(generator.NewGuid(new DateTime(i, 1, 1, 0, 0, 0,
					DateTimeKind.Local), shardCode));
			}
			//Assert
			Assert.True(items.SequenceEqual(items.OrderBy(x => x)));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestSqlGuidBigDateRange(ushort shardCode)
		{
			//Arrange
			var generator = HotSqlGuidGenerator.Instance;
			var items = new List<SqlGuid>();
			//Act
			for (var i = 1970; i < DateTime.Today.Year; i++)
			{
				items.Add(generator.NewGuid(new DateTime(i, 1, 1, 0, 0, 0,
					DateTimeKind.Utc), shardCode));
			}

			//Assert
			Assert.True(items.SequenceEqual(items.OrderBy(x => x)));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestSqlGuidGenerator(ushort shardCode)
		{
			// Arrange
			var now = DateTime.UtcNow;
			// Act
			var stamp = HotSqlGuidGenerator.Instance.NewSqlGuid(now, shardCode).ToDateTime();
			// Assert
			Assert.Equal(now, stamp);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestExtractShardCodeFromGuid(ushort shardCode)
		{
			//Arrange
			var generator = HotGuidGenerator.Instance;
			//Act
			var item = generator.NewGuid(shardCode);
			//Assert
			Assert.Equal(shardCode, item.ExtractShardCode());
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(208)]
		[InlineData(65535)]
		private void TestExtractShardCodeFromSqlGuid(ushort shardCode)
		{
			//Arrange
			var generator = HotSqlGuidGenerator.Instance;
			//Act
			var item = generator.NewSqlGuid(shardCode);
			//Assert
			Assert.Equal(shardCode, item.ExtractShardCode());
		}
	}
}