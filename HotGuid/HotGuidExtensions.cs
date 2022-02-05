﻿using System.Collections.ObjectModel;
using System.Data.SqlTypes;

namespace HotGuid
{
    public static class HotGuidExtensions
    {
		private static readonly IReadOnlyDictionary<byte, byte> ToSqlGuidMap;
		private static readonly IReadOnlyDictionary<byte, byte> ToGuidMap;

		/// <summary>
		/// Constructor initializes the guid sequence mappings
		/// </summary>
		static HotGuidExtensions()
		{
			//See: http://sqlblog.com/blogs/alberto_ferrari/archive/2007/08/31/how-are-guids-sorted-by-sql-server.aspx
			ToGuidMap = new ReadOnlyDictionary<byte, byte>(
				new Dictionary<byte, byte>
				{
				{0, 13},
				{1, 12},
				{2, 11},
				{3, 10},
				{4, 15},
				{5, 14},
				{6, 9},
				{7, 8},
				{8, 6},
				{9, 7},
				{10, 4},
				{11, 5},
				{12, 0},
				{13, 1},
				{14, 2},
				{15, 3}
				});
			//Invert map
			ToSqlGuidMap =
				new ReadOnlyDictionary<byte, byte>(
					ToGuidMap.ToDictionary(d => d.Value, d => d.Key));
		}

		private static DateTime ToDateTime(this long ticks) =>
		new(ticks, DateTimeKind.Utc);

		/// <summary>
		/// Will return the value of DateTime.UtcNow at the time of the generation of the Guid
		/// </summary>
		/// <param name="guid">A Hot Guid with the first 4 bytes containing the seconds of unix time</param>
		/// <returns>DateTime?</returns>
		public static DateTime? ToDateTime(this Guid guid)
		{
			var ticks = ((long)guid.ToSeconds() << 32);
			if (ticks.IsDateTime())
			{
				return ticks.ToDateTime();
			}

			//Try conversion through sql guid
			ticks = ((long)new SqlGuid(guid).ToGuid().ToSeconds() << 32);
			return ticks.IsDateTime()
				? ticks.ToDateTime()
				: default(DateTime?);
		}

		/// <summary>
		/// Will return the value of DateTime.UtcNow at the time of the generation of the Guid
		/// </summary>
		/// <param name="sqlGuid">
		/// A hot SqlGuid with the first sorted 4 bytes containing the seconds of unix time
		/// </param>
		/// <returns>DateTime?</returns>
		public static DateTime? ToDateTime(this SqlGuid sqlGuid) =>
			sqlGuid.ToGuid().ToDateTime();

		/// <summary>
		/// Will take a SqlGuid and re-sequence to a Guid that will sort in the same order
		/// </summary>
		/// <param name="sqlGuid">Any SqlGuid</param>
		/// <returns>Guid</returns>
		public static Guid ToGuid(this SqlGuid sqlGuid)
		{
			var bytes = sqlGuid.ToByteArray();
			return new(Enumerable.Range(0, 16)
				.Select(e => bytes![ToGuidMap[(byte)e]])
				.ToArray());
		}

		/// <summary>
		/// Will take a Guid and will re-sequence it so that it will sort properly in SQL Server without fragmenting your tables
		/// </summary>
		/// <param name="guid">Any Guid</param>
		/// <returns>SqlGuid</returns>
		public static SqlGuid ToSqlGuid(this Guid guid)
		{
			var bytes = guid.ToByteArray();
			return new(Enumerable.Range(0, 16)
				.Select(e => bytes[ToSqlGuidMap[(byte)e]])
				.ToArray());
		}

		internal static bool IsDateTime(this long ticks) =>
			ticks <= DateTime.UtcNow.Ticks &&
			ticks >= DateTime.UnixEpoch.Ticks;

		/// <summary>
		/// Take seconds since Unix Epoch (1970-01-01)
		/// </summary>
		/// <param name="guid">Hot Guid</param>
		/// <returns>Seconds since Unix Epoch (1970-01-01)</returns>
		private static int ToSeconds(this Guid guid)
		{
			var bytes = guid.ToByteArray();
			return
				((int)bytes[3] << 56) +
				((int)bytes[2] << 48) +
				((int)bytes[1] << 40) +
				((int)bytes[0] << 32);
		}

		/// <summary>
		/// Take shard key out of Hot Guid value
		/// </summary>
		/// <param name="guid">Guid generaged by HotGuidGenerator</param>
		/// <returns>Shard key</returns>
		public static uint ExtractShardKey(this Guid guid)
        {
			var bytes = guid.ToByteArray();
			return BitConverter.ToUInt32(bytes, 12);
        }

		/// <summary>
		/// Take shard key out of SqlGuid value
		/// </summary>
		/// <param name="guid">SqlGuid generated by HotSqlGuidGenerator</param>
		/// <returns>Shard key</returns>
		public static uint ExtractShardKey(this SqlGuid guid)
		{
			var bytes = guid.ToGuid().ToByteArray();
			return BitConverter.ToUInt32(bytes, 12);
		}
	}
}
