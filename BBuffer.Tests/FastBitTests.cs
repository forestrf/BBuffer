using BBuffer;
using NUnit.Framework;
using System;

namespace BBufferTests {
	[TestFixture]
	public class FastBitTests {
		static FastBit.UInt ff = new FastBit.UInt(0xffffffff);

		[Test]
		public void TestByte() {
			byte[] valuesToTest = new byte[] { 0x00, 0x01, 0x55, 0xAA, 0x77, 0xF0, 0x0F, 0x7F, 0xFF };

			byte[] testArr = new byte[sizeof(byte) + 2];
			int arrayLength = testArr.Length - sizeof(byte);

			foreach (var a in valuesToTest) {
				for (int offset = 0; offset < 8 * arrayLength; offset++) {
					for (int bits = 0; bits < sizeof(byte) * 8; bits++) {
						var mask = Mask(bits);
						ResetArray(testArr, 0x00);
						FastBit.Byte.Write(a, testArr, offset, bits);
						Assert.AreEqual(a & mask, FastBit.Byte.Read(testArr, offset, bits), "\noffset:<" + offset + ">.\nbits:<" + bits + ">.");
					}
				}

				for (int offset = 0; offset < 8 * arrayLength; offset++) {
					for (int bits = 0; bits < sizeof(byte) * 8; bits++) {
						var mask = Mask(bits);
						ResetArray(testArr, 0xff);
						FastBit.Byte.Write((byte) a, testArr, offset, bits);
						Assert.AreEqual(a & mask, FastBit.Byte.Read(testArr, offset, bits), "\noffset:<" + offset + ">.\nbits:<" + bits + ">.");
					}
				}

				for (int bits = 0; bits < sizeof(byte) * 8; bits++) {
					var mask = Mask(bits);
					for (int offset = bits; offset < 8 * arrayLength; offset++) {
						ResetArray(testArr, 0xff);
						FastBit.Byte.Write((byte) a, testArr, offset, bits);
						ff.Write(testArr, offset - bits, bits); // write behind, trying to override
						Assert.AreEqual(a & mask, FastBit.Byte.Read(testArr, offset, bits), "\noffset:<" + offset + ">.\nbits:<" + bits + ">.");
					}
				}
			}
		}

		[Test]
		public void TestUShort() {
			ushort[] valuesToTest = new ushort[] { 0x00, 0x01, 0x55, 0xAA, 0x77, 0xF0, 0x0F, 0x7F, 0xFF,
				0x5555, 0x1155, 0x5511, 0x5151, 0x1515, 0xffff, 0x7fff };

			byte[] testArr = new byte[sizeof(ushort) + 2];
			int arrayLength = testArr.Length - sizeof(ushort);

			foreach (var a in valuesToTest) {
				var sb = new FastBit.UShort(a);
				Assert.AreEqual(a, sb.value);
				for (int offset = 0; offset < 8 * arrayLength; offset++) {
					for (int bits = 0; bits < sizeof(ushort) * 8; bits++) {
						var mask = Mask(bits);
						ResetArray(testArr, 0x00);
						sb.Write(testArr, offset, bits);
						Assert.AreEqual(a & mask, new FastBit.UShort().Read(testArr, offset, bits), "\noffset:<" + offset + ">.\nbits:<" + bits + ">.");
					}
				}

				for (int offset = 0; offset < 8 * arrayLength; offset++) {
					for (int bits = 0; bits < sizeof(ushort) * 8; bits++) {
						var mask = Mask(bits);
						ResetArray(testArr, 0xff);
						sb.Write(testArr, offset, bits);
						Assert.AreEqual(a & mask, new FastBit.UShort().Read(testArr, offset, bits), "\noffset:<" + offset + ">.\nbits:<" + bits + ">.");
					}
				}

				for (int bits = 0; bits < sizeof(ushort) * 8; bits++) {
					var mask = Mask(bits);
					for (int offset = bits; offset < 8 * arrayLength; offset++) {
						ResetArray(testArr, 0xff);
						sb.Write(testArr, offset, bits);
						ff.Write(testArr, offset - bits, bits); // write behind, trying to override
						Assert.AreEqual(a & mask, new FastBit.UShort().Read(testArr, offset, bits), "\noffset:<" + offset + ">.\nbits:<" + bits + ">.");
					}
				}
			}
		}

		[Test]
		public void TestUInt() {
			uint[] valuesToTest = new uint[] { 0x00, 0x01, 0x55, 0xAA, 0x77, 0xF0, 0x0F, 0x7F, 0xFF,
				0x5555, 0x1155, 0x5511, 0x5151, 0x1515, 0xffff, 0x7fff,
				0x55555555, 0x11224488, 0xFFDDAA99, 0xFFFFFFFF, 0X7FFFFFFF };

			byte[] testArr = new byte[sizeof(uint) + 2];
			int arrayLength = testArr.Length - sizeof(uint);

			foreach (var a in valuesToTest) {
				var sb = new FastBit.UInt(a);
				Assert.AreEqual(a, sb.value);
				for (int offset = 0; offset < 8 * arrayLength; offset++) {
					for (int bits = 0; bits < sizeof(uint) * 8; bits++) {
						var mask = Mask(bits);
						ResetArray(testArr, 0x00);
						sb.Write(testArr, offset, bits);
						Assert.AreEqual(a & mask, new FastBit.UInt().Read(testArr, offset, bits), "\noffset:<" + offset + ">.\nbits:<" + bits + ">.");
					}
				}

				for (int offset = 0; offset < 8 * arrayLength; offset++) {
					for (int bits = 0; bits < sizeof(uint) * 8; bits++) {
						var mask = Mask(bits);
						ResetArray(testArr, 0xff);
						sb.Write(testArr, offset, bits);
						Assert.AreEqual(a & mask, new FastBit.UInt().Read(testArr, offset, bits), "\noffset:<" + offset + ">.\nbits:<" + bits + ">.");
					}
				}

				for (int bits = 0; bits < sizeof(uint) * 8; bits++) {
					var mask = Mask(bits);
					for (int offset = bits; offset < 8 * arrayLength; offset++) {
						ResetArray(testArr, 0xff);
						sb.Write(testArr, offset, bits);
						ff.Write(testArr, offset - bits, bits); // write behind, trying to override
						Assert.AreEqual(a & mask, new FastBit.UInt().Read(testArr, offset, bits), "\noffset:<" + offset + ">.\nbits:<" + bits + ">.");
					}
				}
			}
		}

		[Test]
		public void TestULong() {
			ulong[] valuesToTest = new ulong[] { 0x00, 0x01, 0x55, 0xAA, 0x77, 0xF0, 0x0F, 0x7F, 0xFF,
				0x5555, 0x1155, 0x5511, 0x5151, 0x1515, 0xffff, 0x7fff,
				0x55555555, 0x11224488, 0xFFDDAA99, 0xFFFFFFFF, 0X7FFFFFFF,
				0x5555555555555555, 0x1122448899AADDFF, 0xFFDDAA9988442211, 0xFFFFFFFFFFFFFFFF, 0X7FFFFFFFFFFFFFFF };

			byte[] testArr = new byte[sizeof(ulong) + 2];
			int arrayLength = testArr.Length - sizeof(ulong);

			foreach (var a in valuesToTest) {
				var sb = new FastBit.ULong(a);
				Assert.AreEqual(a, sb.value);
				for (int offset = 0; offset < 8 * arrayLength; offset++) {
					for (int bits = 0; bits < sizeof(ulong) * 8; bits++) {
						var mask = Mask(bits);
						ResetArray(testArr, 0x00);
						sb.Write(testArr, offset, bits);
						Assert.AreEqual(a & mask, new FastBit.ULong().Read(testArr, offset, bits), "\noffset:<" + offset + ">.\nbits:<" + bits + ">.");
					}
				}

				for (int offset = 0; offset < 8 * arrayLength; offset++) {
					for (int bits = 0; bits < sizeof(ulong) * 8; bits++) {
						var mask = Mask(bits);
						ResetArray(testArr, 0xff);
						sb.Write(testArr, offset, bits);
						Assert.AreEqual(a & mask, new FastBit.ULong().Read(testArr, offset, bits), "\noffset:<" + offset + ">.\nbits:<" + bits + ">.");
					}
				}

				for (int bits = 0; bits < sizeof(ulong) * 8; bits++) {
					var mask = Mask(bits);
					for (int offset = bits; offset < 8 * arrayLength; offset++) {
						ResetArray(testArr, 0xff);
						sb.Write(testArr, offset, bits);
						ff.Write(testArr, offset - bits, bits); // write behind, trying to override
						Assert.AreEqual(a & mask, new FastBit.ULong().Read(testArr, offset, bits), "\noffset:<" + offset + ">.\nbits:<" + bits + ">.");
					}
				}
			}
		}

		[Test]
		public void TestGetBytes() {
			for (int i = 0; i < 20; i++) {
				BitBuffer b = new BitBuffer(new byte[256], i);
				Random r = new Random(0);
				for (int j = 0; j < 15; j++) {
					int expected = r.Next();
					b.Put(expected);
					Assert.AreEqual(expected, b.GetIntAt(32 * j));
				}

				for (int k = 0; k < 20; k++) {
					byte[] clon = new byte[256];
					b.GetBits(clon, k, 700);
					BitBuffer b2 = new BitBuffer(clon);

					r = new Random(0);
					for (int j = 0; j < 15; j++) {
						int expected = r.Next();
						int read = b2.GetInt();
						Assert.AreEqual(expected, read, "offset source=" + i + ", ofset destination=" + k + ", number=" + j);
					}
				}
			}
		}

		[Test]
		public void TestByteWithOffset() {
			for (int i = 0; i < 16; i++) {
				var b = new BitBuffer(new byte[3], i);
				for (int j = 0; j < 255; j++) {
					for (int k = 0; k < 8; k++) {
						var bWrite = b;
						var bRead = b;
						bWrite.Put((byte) j, k);
						Assert.AreEqual(j & ((1 << k) - 1), bRead.GetByte(k), "bits=" + k + ", offset=" + i);
					}
				}
			}
		}

		[Test]
		public void TestBitInLastByte() {
			var b = new BitBuffer(new byte[1], 7);
			b.GetBool();
			for (int i = 0; i < 8; i++) {
				b = new BitBuffer(new byte[1], i);
				b.GetByte(8 - i);
			}
		}

		static ulong Mask(int bits) {
			return bits == 64 ? 0xffffffffffffffff : ~(0xffffffffffffffff << bits);
		}
		static void ResetArray(byte[] arr, byte value) {
			for (int i = 0; i < arr.Length; i++) arr[i] = value;
		}
	}
}
