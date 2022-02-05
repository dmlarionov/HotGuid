using System.Data.SqlTypes;

namespace HotGuid
{
    public sealed class HotSqlGuidGenerator : HotGuidGeneratorBase<HotSqlGuidGenerator>
    {
		private HotSqlGuidGenerator() { }

		public override Guid NewGuid(long timestamp, uint shardKey) =>
		base.NewGuid(timestamp, shardKey).ToSqlGuid().Value;

		/// <summary>
		/// Returns a guid for the value of UtcNow
		/// </summary>
		/// <returns>Sequential SQL guid</returns>
		public SqlGuid NewSqlGuid(uint shardKey) =>
			new(NewGuid(shardKey));

		/// <summary>
		/// Takes a date time parameter to encode in a sequential SQL guid
		/// </summary>
		/// <param name="timestamp">
		/// Timestamp that must not be in unspecified kind and must be between the unix epoch and now to be considered valid
		/// </param>
		/// <returns>Sequential SQL guid</returns>
		public SqlGuid NewSqlGuid(DateTime timestamp, uint shardKey) =>
			new(NewGuid(timestamp, shardKey));
	}
}
