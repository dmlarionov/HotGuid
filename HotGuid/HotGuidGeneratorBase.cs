using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace HotGuid
{
    /// <summary>
    /// Generates time-ordered ID in Guid (UUID) format based on ushort shard code (region code)
    /// </summary>
    public class HotGuidGeneratorBase<T> where T : HotGuidGeneratorBase<T>
    {
		// We use singleton to avoid having 2 instances of generator with the same machine & process PID.
		private static readonly Lazy<T> Lazy =
			new(() => (Activator.CreateInstance(typeof(T), true) as T)!);

		/// <summary>
		/// 4-byte random value generated once per process. This random value is unique to the machine and process.
		/// </summary>
		private readonly byte[] _machinePid;

		/// <summary>
		/// Incrementing counter, initialized to a random value (we use low order 2 bytes)
		/// </summary>
		private uint _increment;

		/// <summary>
		///     Protected constructor that seeds the necessary values from the machine name hash &amp; process id as well as seed
		///     the increment
		/// </summary>
		protected HotGuidGeneratorBase()
		{
			_increment = (uint)new Random().Next(0, 500000);
			_machinePid = new byte[4];
			using (var algorithm = MD5.Create())
			{
				var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(Environment.MachineName));
				// use first 2 bytes of hash
				for (var i = 0; i < 2; i++)
				{
					_machinePid[i] = hash[i];
				}
			}

			try
			{
				var pid = Environment.ProcessId;
				// use low order two bytes only
				_machinePid[2] = (byte)(pid >> 8);
				_machinePid[3] = (byte)pid;
			}
			catch (SecurityException)
			{
			}
		}

		/// <summary>
		///     Singleton instance of the generator
		/// </summary>
		public static T Instance => Lazy.Value;

		/// <summary>
		///     Returns a guid for the value of UtcNow
		/// </summary>
		/// <param name="shardCode">The shard code to keep in ID value</param>
		/// <returns>Sequential guid</returns>
		public Guid NewGuid(ushort shardCode) =>
			NewGuid(DateTime.UtcNow.Ticks, shardCode);

		/// <summary>
		///     Takes a date time parameter to encode in a sequential guid
		/// </summary>
		/// <param name="timestamp">
		///     Timestamp that must not be in unspecified kind and must be between the unix epoch and now to be
		///     considered valid
		/// </param>
		/// <param name="shardCode">The shard code to keep in ID value</param>
		/// <returns>Sequential guid</returns>
		public Guid NewGuid(DateTime timestamp, ushort shardCode)
		{
			var ticks = timestamp.Kind switch
			{
				DateTimeKind.Utc => timestamp.Ticks, // use ticks as is
				DateTimeKind.Local => timestamp.ToUniversalTime().Ticks, // convert to UTC
				_ => throw new ArgumentException("DateTimeKind.Unspecified not supported", nameof(timestamp))
			};

			// run validation after tick conversion
			if (!ticks.IsDateTime())
			{
				throw new ArgumentException("Timestamp must be between January 1st, 1970 UTC and now",
					nameof(timestamp));
			}

			// Once we've gotten here we have a valid UTC tick count so yield the Guid
			return NewGuid(ticks, shardCode);
		}

		/// <summary>
		///     Implementation that increments the counter and shreds the timestamp &amp; increment and constructs the guid
		/// </summary>
		/// <param name="timestamp">Timestamp in terms of Ticks</param>
		/// <param name="shardCode">The shard code to keep in ID value</param>
		/// <returns></returns>
		public virtual Guid NewGuid(long timestamp, ushort shardCode)
		{
			// only use low order 2 bytes of increment
			var increment = Interlocked.Increment(ref _increment) & 0x0000ffff;
			return new Guid(
				(int)(timestamp >> 32),
				(short)(timestamp >> 16),
				(short)timestamp,
				_machinePid
				.Concat(BitConverter.GetBytes(shardCode))
				.Concat(BitConverter.GetBytes((ushort)increment)).ToArray()
			);
		}
	}
}