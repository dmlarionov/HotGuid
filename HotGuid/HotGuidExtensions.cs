using System.Collections.ObjectModel;
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

		// <summary>
		/// Take a SqlGuid and re-sequence to a Guid that will sort in the same order
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
		/// Take a Guid and re-sequence it so that it will sort properly in SQL Server without fragmenting your tables
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

		/// <summary>
		/// The unix timestamp (seconds since 1970-01-01 in UTC) of Hot Guid
		/// </summary>
		/// <param name="guid">Hot Guid</param>
		/// <returns>Unix timestamp</returns>
		public static int ExtractUnixTimestamp(this Guid guid)
			=> BitConverter.ToInt32(guid.ToByteArray());

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
	}
}
