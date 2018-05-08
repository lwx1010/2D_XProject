using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace Riverlake.Crypto
{
	#region SecretBytes
	public class SecretBytes
	{
		/// <summary>
		/// 字节异或密钥
		/// </summary>
		private readonly byte seed1 = (byte)UnityEngine.Random.Range(byte.MinValue, byte.MaxValue);
		private readonly byte seed2 = (byte)UnityEngine.Random.Range(byte.MinValue, byte.MaxValue);
		private byte[] bytes1;
		private byte[] bytes2;

		public byte[] Bytes
		{
			get
			{
				if (bytes1 == null) return null;
				var buf = (from b in bytes1 select (byte)(b ^ seed1)).ToArray();
				if (buf.SequenceEqual(from b in bytes2 select (byte)(b ^ seed2)))
					return buf;
				else
				{
					Application.Quit();
					throw new InvalidOperationException();
				}
			}
			set
			{
				if (value == null)
				{
					bytes1 = null;
					bytes2 = null;
				}
				else
				{
					bytes1 = (from b in value select (byte)(b ^ seed1)).ToArray();
					bytes2 = (from b in value select (byte)(b ^ seed2)).ToArray();
				}
			}
		}
	}
	#endregion

	#region SecretBoolean
	public class SecretBoolean : SecretBytes
	{
		public bool Value
		{
			get { return BitConverter.ToBoolean(Bytes, 0); }
			set { Bytes = BitConverter.GetBytes(value); }
		}

		public SecretBoolean(bool value = default(bool))
		{
			Value = value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public static implicit operator bool(SecretBoolean s)
		{
			return s.Value;
		}

		public static implicit operator SecretBoolean(bool value)
		{
			return new SecretBoolean(value);
		}
	}
	#endregion

	#region SecretInt32
	public class SecretInt32 : SecretBytes
	{
		public int Value
		{
			get { return BitConverter.ToInt32(Bytes, 0); }
			set { Bytes = BitConverter.GetBytes(value); }
		}

		public SecretInt32(int value = default(int))
		{
			Value = value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public static implicit operator int(SecretInt32 s)
		{
			return s.Value;
		}

		public static implicit operator SecretInt32(int value)
		{
			return new SecretInt32(value);
		}
	}
	#endregion

	#region SecretDouble
	public class SecretDouble : SecretBytes
	{
		public double Value
		{
			get { return BitConverter.ToDouble(Bytes, 0); }
			set { Bytes = BitConverter.GetBytes(value); }
		}

		public SecretDouble(double value = default(double))
		{
			Value = value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public static implicit operator double(SecretDouble s)
		{
			return s.Value;
		}

		public static implicit operator SecretDouble(double value)
		{
			return new SecretDouble(value);
		}
	}
	#endregion

}