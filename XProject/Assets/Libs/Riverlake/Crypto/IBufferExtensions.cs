#if UNITY_METRO && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace AL.WinRT
{
	static class IBufferExtensions
	{
		public static byte[] ToBytes(this IBuffer buffer)
		{
			if (buffer == null)
				return null;
			var ret = new byte[buffer.Length];
			CryptographicBuffer.CopyToByteArray(buffer, out ret);
			return ret;
		}

		public static IBuffer ToIBuffer(this byte[] data)
		{
			if (data == null)
				return null;
			return CryptographicBuffer.CreateFromByteArray(data);
		}
	}
}
#endif