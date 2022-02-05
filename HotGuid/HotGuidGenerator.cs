using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

[assembly: InternalsVisibleTo("HotGuid.Tests")]

namespace HotGuid
{
	/// <summary>
	/// Generates time-ordered ID in Guid / UUID format (Hot Guid) based on 4-byte shard key
	/// </summary>
	public sealed class HotGuidGenerator
    {
		// We use singleton to avoid having 2 instances of generator with the same machine & process PID.
		private static readonly Lazy<HotGuidGenerator> Lazy =
			new(() => (Activator.CreateInstance(typeof(HotGuidGenerator), true) as HotGuidGenerator)!);

		/// <summary>
		/// 5-byte random value generated once per process. This random value is unique to the machine and process.
		/// </summary>
		private readonly byte[] _machinePid;

		/// <summary>
		/// Incrementing counter, initialized to a random value (we use low order 3 bytes)
		/// </summary>
		private int _increment;

		/// <summary>
		/// Protected constructor that seeds the necessary values from the machine name hash &amp; process id as well as seed the increment
		/// </summary>
		private HotGuidGenerator()
		{
			_increment = new Random().Next(500000);
			_machinePid = new byte[5];
			using (var algorithm = MD5.Create())
			{
				var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(Environment.MachineName));
				// use first 3 bytes of hash
				for (var i = 0; i < 3; i++)
				{
					_machinePid[i] = hash[i];
				}
			}

			try
			{
				var pid = Environment.ProcessId;
				// use low order two bytes only
				_machinePid[3] = (byte)(pid >> 8);
				_machinePid[4] = (byte)pid;
			}
			catch (SecurityException)
			{
			}
		}

		/// <summary>
		/// Singleton instance of the generator
		/// </summary>
		public static HotGuidGenerator Instance => Lazy.Value;

		/// <summary>
		/// Returns a Hot Guid generated for a given shard key
		/// </summary>
		/// <param name="shardKey">The shard code to keep in ID value</param>
		/// <returns>Hot Guid</returns>
		public Guid NewGuid(uint shardKey) => NewGuid(shardKey, DateTime.UtcNow);

		/// <summary>
		/// Returns a Hot Guid generated for a given shard key and DateTime
		/// </summary>
		/// <param name="shardKey">The shard code to keep in ID value</param>
		/// <param name="dateTime">DateTime</param>
		/// <returns>Hot Guid</returns>
		public Guid NewGuid(uint shardKey, DateTime dateTime)
        {
			var unixTimestamp = (uint)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			// only use low order 3 bytes of increment
			var increment = Interlocked.Increment(ref _increment) & 0x00ffffff;

			return new Guid(
				BitConverter.GetBytes(unixTimestamp).Concat(
					_machinePid.Concat(
						new[] { (byte)(increment >> 16), (byte)(increment >> 8), (byte)increment }).Concat(
							BitConverter.GetBytes(shardKey)))
				.ToArray()
			);
		}
	}
}