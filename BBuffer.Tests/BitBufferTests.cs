using BBuffer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace BBufferTests {
	[TestFixture]
	public class BitBufferTests {
		[Test]
		public void Test() {
			// https://www.h-schmidt.net/FloatConverter/IEEE754.html

			byte[] tmp = new byte[400];

			byte[] expectedMessage = new byte[] {
				0x56, 0x47, 0x68, 0x70, 0x63, 0x79, 0x42, 0x70,
				0x63, 0x79, 0x42, 0x68, 0x49, 0x48, 0x52, 0x6c,
				0x63, 0x33, 0x51, 0x67, 0x62, 0x57, 0x56, 0x7a,
				0x63, 0x32, 0x46, 0x6e, 0x5a, 0x53, 0x45, 0x3d
			};

			for (int off = 0; off < 30; off++) {
				for (int len = 300; len < 330; len++) {
					BitBuffer b = new BitBuffer(tmp, off, len);
					{
						Assert.AreEqual(off, b.absPosition);
						Assert.AreEqual(0, b.Position);
						Assert.AreEqual(off + len, b.absLength);
						Assert.AreEqual(len, b.Length);
						Assert.AreEqual(off, b.absOffset);
					}

					b.Put((ushort) 0x4756);
					b.Put((byte) 0x11);
					b.Put((byte) 0x70);
					b.PutAt(1 * 8, (ushort) 0x6847);
					b.Put(0x70427963);
					b.Position += 64;
					b.Put((ulong) 0x7a56576267513363);
					b.Position = 8 * 8;
					b.Put(3.67351315E+24f);
					b.Put(1.01686312E+27f);
					b.Position += 64;
					b.Put(1.5152749180821361E-13d);

					Assert.IsTrue(b.FromStartToPosition().BufferEquals(new BitBuffer(expectedMessage)), "off=" + off + ", len=" + len);

					Assert.AreEqual(off + expectedMessage.Length * 8, b.absPosition);
					Assert.AreEqual(off, b.absOffset);
					Assert.AreEqual(expectedMessage.Length * 8, b.Position);
					Assert.AreEqual(len, b.Length);
					Assert.AreEqual(off + len, b.absLength);

					Assert.IsTrue(b.BufferEquals(b));
				}
			}
		}

		[Test]
		public void TestVariableLength() {
			// https://www.h-schmidt.net/FloatConverter/IEEE754.html

			byte[] tmp = new byte[2000];

			for (int off = 0; off < 30; off++) {
				BitBuffer b = new BitBuffer(tmp, off);

				b.PutVariableLength(0);
				b.PutVariableLength(1);
				b.PutVariableLength(-1);
				b.PutVariableLength((uint) 0x7f);
				b.PutVariableLength((uint) 0x80);
				b.PutVariableLength((uint) 0x81);
				b.PutVariableLength((uint) 0xff);
				b.PutVariableLength(0x7f);
				b.PutVariableLength(0x80);
				b.PutVariableLength(0xff);
				b.PutVariableLength((uint) 0x7f);
				b.PutVariableLength((uint) 0x7fff);
				b.PutVariableLength((uint) 0xffff);
				b.PutVariableLength((uint) 0xffffff);
				b.PutVariableLength((uint) 0xffffffff);
				b.PutVariableLength((ulong) 0xffffffffff);
				b.PutVariableLength((ulong) 0xffffffffffff);
				b.PutVariableLength((ulong) 0xffffffffffffff);
				b.PutVariableLength((ulong) 0xffffffffffffffff);
				b.PutVariableLength(0xffffffffff);
				b.PutVariableLength(0xffffffffffff);


				b.Position = 0;


				Assert.AreEqual(0, b.GetIntVariableLength());
				Assert.AreEqual(1, b.GetIntVariableLength());
				Assert.AreEqual(-1, b.GetIntVariableLength());
				Assert.AreEqual((uint) 0x7f, b.GetUIntVariableLength());
				Assert.AreEqual((uint) 0x80, b.GetUIntVariableLength());
				Assert.AreEqual((uint) 0x81, b.GetUIntVariableLength());
				Assert.AreEqual((uint) 0xff, b.GetUIntVariableLength());
				Assert.AreEqual(0x7f, b.GetIntVariableLength());
				Assert.AreEqual(0x80, b.GetIntVariableLength());
				Assert.AreEqual(0xff, b.GetIntVariableLength());
				Assert.AreEqual((uint) 0x7f, b.GetUIntVariableLength());
				Assert.AreEqual((uint) 0x7fff, b.GetUIntVariableLength());
				Assert.AreEqual((uint) 0xffff, b.GetUIntVariableLength());
				Assert.AreEqual((uint) 0xffffff, b.GetUIntVariableLength());
				Assert.AreEqual((uint) 0xffffffff, b.GetUIntVariableLength());
				Assert.AreEqual((ulong) 0xffffffffff, b.GetULongVariableLength());
				Assert.AreEqual((ulong) 0xffffffffffff, b.GetULongVariableLength());
				Assert.AreEqual((ulong) 0xffffffffffffff, b.GetULongVariableLength());
				Assert.AreEqual((ulong) 0xffffffffffffffff, b.GetULongVariableLength());
				Assert.AreEqual(0xffffffffff, b.GetLongVariableLength());
				Assert.AreEqual(0xffffffffffff, b.GetLongVariableLength());
			}
		}

		[Test]
		public void TestBitBufferPut() {
			BitBuffer b = new BitBuffer(new byte[2000]);
			b.Put(new BitBuffer(b.data, 0, 1));
			b.Put(new BitBuffer(b.data, 1, 1));
			b.Put(new BitBuffer(b.data, 2, 2));
			b.Put(new BitBuffer(b.data, 2, 7));
			b.Put(new BitBuffer(b.data, 2, 20));
			Assert.AreEqual(31, b.Position);

			b.PutAt(31, new BitBuffer(b.data, 0, 10));
			b.PutAt(31, new BitBuffer(b.data, 3, 10));
			Assert.AreEqual(31, b.Position);
		}

		[Test]
		public void TestBitBufferPut2() {
			BitBuffer b = new BitBuffer(new byte[1200]);
			b.Position = 132;

			BitBuffer bToPut = new BitBuffer(new byte[1200]);
			bToPut.Put((byte) 0);
			bToPut.Put((byte) 1);
			bToPut.Put((byte) 0);
			bToPut.Length = bToPut.absPosition;

			CollectionAssert.AreEqual(new byte[3] { 0, 1, 0 }, bToPut.ToArray());

			b.PutAt(132, bToPut);

			CollectionAssert.AreEqual(new byte[3] { 0, 1, 0 }, b.GetBitsAt(132, 24).ToArray());
		}

		[Test]
		public void BitsOccuplied() {
			Assert.AreEqual(0, BitBuffer.BitsOccupied(0));
			for (int i = 0; i < 32; i++) {
				Assert.AreEqual(i + 1, BitBuffer.BitsOccupied(1u << i));
			}

			Assert.AreEqual(0, BitBuffer.BitsOccupied(0ul));
			for (int i = 0; i < 64; i++) {
				Assert.AreEqual(i + 1, BitBuffer.BitsOccupied(1ul << i));
			}
		}

		[Test]
		public void TestRangedFloat() {
			BitBuffer b = new BitBuffer(new byte[1200]);

			int MIN = -5;
			int MAX = 5;

			int minBits = BitBuffer.BitsOccupied((uint) (MAX - MIN));

			for (int absOffset = 0; absOffset < 8; absOffset++) {
				b.absOffset = absOffset;
				for (int bits = 32; bits >= minBits; bits--) {
					for (float min = MIN; min < MAX; min++) {
						for (float max = min; max < MAX; max++) {
							for (float f = min; f <= max; f++) {
								for (int pos = 0; pos < 8; pos++) {
									b.Position = pos;
									var bWrite = b;
									var bRead = b;
									bWrite.PutRanged(f, min, max, bits);
									float expected = bRead.GetRangedFloat(min, max, bits);

									Assert.AreEqual(f, Math.Round(expected), "expected=" + f + " actual=" + expected + "\n" +
										"bits=" + bits + "\n" +
										"absOffset=" + absOffset + "\n" +
										"pos=" + pos + "\n" +
										"min=" + min + "\n" +
										"max=" + max);
								}
							}
						}
					}
				}
			}
		}

		[Test]
		public void TestVariableLength2() {
			var b = new BitBuffer(new byte[200000]);
			Random r = new Random(0);
			for (int j = 0; j < 4; j++) {
				var bWrite = b;
				var bRead = b;
				for (int i = 0; i < 1000; i++) {
					int target = new FastByte.Int((byte) r.Next(), (byte) r.Next(), (byte) r.Next(), (byte) r.Next()).value;
					target >>= j * 8;
					bWrite.PutVariableLength(target);
					Assert.AreEqual(target, bRead.GetIntVariableLengthAt(bRead.Position));
					Assert.AreEqual(target, bRead.GetIntVariableLength());
					bWrite.PutVariableLength((uint) target);
					Assert.AreEqual((uint) target, bRead.GetUIntVariableLengthAt(bRead.Position));
					Assert.AreEqual((uint) target, bRead.GetUIntVariableLength());
				}
			}

			for (int j = 0; j < 8; j++) {
				var bWrite = b;
				var bRead = b;
				for (int i = 0; i < 1000; i++) {
					long target = new FastByte.Long((byte) r.Next(), (byte) r.Next(), (byte) r.Next(), (byte) r.Next(), (byte) r.Next(), (byte) r.Next(), (byte) r.Next(), (byte) r.Next()).value;
					target >>= j * 8;
					bWrite.PutVariableLength(target);
					Assert.AreEqual(target, bRead.GetLongVariableLengthAt(bRead.Position));
					Assert.AreEqual(target, bRead.GetLongVariableLength());
					bWrite.PutVariableLength((ulong) target);
					Assert.AreEqual((ulong) target, bRead.GetULongVariableLengthAt(bRead.Position));
					Assert.AreEqual((ulong) target, bRead.GetULongVariableLength());
				}
			}
		}

		[Test]
		public void TestVariableLength3() {
			var b = new BitBuffer(new byte[20000]);
			var bWrite = b;
			var bRead = b;
			bWrite.PutVariableLength(-5);
			bWrite.PutVariableLength(-4);
			bWrite.PutVariableLength(-3);
			bWrite.PutVariableLength(-2);
			bWrite.PutVariableLength(-1);

			bWrite.PutVariableLength(0);

			bWrite.PutVariableLength(1);
			bWrite.PutVariableLength(2);
			bWrite.PutVariableLength(3);
			bWrite.PutVariableLength(4);
			bWrite.PutVariableLength(5);

			Assert.AreEqual(9, bRead.GetUIntVariableLength());
			Assert.AreEqual(7, bRead.GetUIntVariableLength());
			Assert.AreEqual(5, bRead.GetUIntVariableLength());
			Assert.AreEqual(3, bRead.GetUIntVariableLength());
			Assert.AreEqual(1, bRead.GetUIntVariableLength());

			Assert.AreEqual(0, bRead.GetUIntVariableLength());

			Assert.AreEqual(2, bRead.GetUIntVariableLength());
			Assert.AreEqual(4, bRead.GetUIntVariableLength());
			Assert.AreEqual(6, bRead.GetUIntVariableLength());
			Assert.AreEqual(8, bRead.GetUIntVariableLength());
			Assert.AreEqual(10, bRead.GetUIntVariableLength());
		}

		[Test]
		public void TestVariableLength4() {
			var b = new BitBuffer(new byte[20000]);
			var bWrite = b;
			var bRead = b;
			bWrite.PutVariableLength(-5L);
			bWrite.PutVariableLength(-4L);
			bWrite.PutVariableLength(-3L);
			bWrite.PutVariableLength(-2L);
			bWrite.PutVariableLength(-1L);

			bWrite.PutVariableLength(0L);

			bWrite.PutVariableLength(1L);
			bWrite.PutVariableLength(2L);
			bWrite.PutVariableLength(3L);
			bWrite.PutVariableLength(4L);
			bWrite.PutVariableLength(5L);

			Assert.AreEqual(9, bRead.GetULongVariableLength());
			Assert.AreEqual(7, bRead.GetULongVariableLength());
			Assert.AreEqual(5, bRead.GetULongVariableLength());
			Assert.AreEqual(3, bRead.GetULongVariableLength());
			Assert.AreEqual(1, bRead.GetULongVariableLength());

			Assert.AreEqual(0, bRead.GetULongVariableLength());

			Assert.AreEqual(2, bRead.GetULongVariableLength());
			Assert.AreEqual(4, bRead.GetULongVariableLength());
			Assert.AreEqual(6, bRead.GetULongVariableLength());
			Assert.AreEqual(8, bRead.GetULongVariableLength());
			Assert.AreEqual(10, bRead.GetULongVariableLength());
		}

		[Test]
		public void Ranged() {

		}

		[Test]
		public void SimulateWrites() {
			var b = new BitBuffer(new byte[10000]) {
				simulateWrites = true
			};
			b.Put(new BitBuffer(new byte[] { 0xff, 0xff, 0xff }, 0, 3));
			Assert.AreEqual(3, b.Position);
			b.Put(true);
			Assert.AreEqual(3 + 1, b.Position);
			b.Put(new byte[] { 0xff, 0xff, 0xff });
			Assert.AreEqual(3 + 1 + 24, b.Position);
			b.Put(1d);
			Assert.AreEqual(3 + 1 + 24 + 64, b.Position);
			b.Put(1f);
			Assert.AreEqual(3 + 1 + 24 + 64 + 32, b.Position);
			b.Put((byte) 1);
			Assert.AreEqual(3 + 1 + 24 + 64 + 32 + 8, b.Position);
			b.Put(1);
			Assert.AreEqual(3 + 1 + 24 + 64 + 32 + 8 + 32, b.Position);
			b.Put(1L);
			Assert.AreEqual(3 + 1 + 24 + 64 + 32 + 8 + 32 + 64, b.Position);
			b.Put((short) 1);
			Assert.AreEqual(3 + 1 + 24 + 64 + 32 + 8 + 32 + 64 + 16, b.Position);
			b.Put(1u);
			Assert.AreEqual(3 + 1 + 24 + 64 + 32 + 8 + 32 + 64 + 16 + 32, b.Position);
			b.Put(1uL);
			Assert.AreEqual(3 + 1 + 24 + 64 + 32 + 8 + 32 + 64 + 16 + 32 + 64, b.Position);
			b.Put((ushort) 1);
			Assert.AreEqual(3 + 1 + 24 + 64 + 32 + 8 + 32 + 64 + 16 + 32 + 64 + 16, b.Position);
			b.Put(new byte[] { 0xff, 0xff, 0xff }, 0, 3 * 8);
			Assert.AreEqual(3 + 1 + 24 + 64 + 32 + 8 + 32 + 64 + 16 + 32 + 64 + 16 + 24, b.Position);

			for (int i = 0; i < b.Position; i++) {
				Assert.AreEqual(false, b.GetBoolAt(i));
			}
		}

		[Test]
		public void TestPutGetBitBuffer() {
			var b = new BitBuffer(new byte[1024]);
			var r = new Random(0);
			for (int i = 0; i < b.data.Length / 4; i++) {
				b.Put(r.Next());
			}

			for (int i = 0; i < 16; i++) {
				for (int length = 0; length < 32; length++) {
					for (int offset = 0; offset < 16; offset++) {
						var c = new BitBuffer(new byte[(int) Math.Ceiling((offset + length) / 8f)], offset);
						c.Put(b.GetBitsAt(i, length));
						c = c.FromStartToPosition();
						CollectionAssert.AreEqual(b.GetBitsAt(i, length).ToArray(), c.ToArray(), "i=" + i + ", length=" + length + ", offset=" + offset);
						Assert.IsTrue(b.GetBitsAt(i, length).BufferEquals(c), "i=" + i + ", length=" + length + ", offset=" + offset);
					}
				}
			}
		}

		[Test]
		public void TestBufferInequality() {
			var b = new BitBuffer(new byte[1024]);
			var r = new Random(0);
			for (int i = 0; i < b.data.Length / 4; i++) {
				b.Put(r.Next());
			}

			for (int offset = 0; offset < 16; offset++) {
				for (int i = 0; i < 16; i++) {
					for (int length = 0; length < 16; length++) {
						var c = new BitBuffer(new byte[(int) Math.Ceiling((offset + length) / 8f)], offset);
						for (int p = 0; p < length; p++) {
							var b2 = c;
							try {
								var tmp = b.GetBitsAt(i, length);
								b2.Put(tmp);
								b2 = b2.FromStartToPosition();
								b2.PutAt(p, !b2.GetBoolAt(p));

								CollectionAssert.AreNotEqual(b.GetBitsAt(i, length).ToArray(), b2.ToArray());
								Assert.IsFalse(b.GetBitsAt(i, length).BufferEquals(b2));
							}
							catch (Exception e) {
								throw new Exception("i=" + i + ", length=" + length + ", p=" + p + ", offset=" + offset, e);
							}
						}
					}
				}
			}
		}

		[Test]
		public void ToArrayTest() {
			Random r = new Random(0);
			for (int offset = 0; offset < 16; offset++) {
				byte[] data = new byte[40];
				r.NextBytes(data);
				var b = new BitBuffer(new byte[60], offset);
				b.Put(data);
				var newArr = b.FromStartToPosition().ToArray();
				CollectionAssert.AreEqual(data, newArr, "offset=" + offset);
			}
		}

		[Test]
		public void BufferPutBuffer() {
			Random r = new Random(0);
			for (int offset1 = 0; offset1 < 16; offset1++) {
				byte[] data = new byte[40];
				r.NextBytes(data);
				for (int offset2 = 0; offset2 < 16; offset2++) {
					var dBitBuffer = new BitBuffer(data, offset2, 4 * 8);
					var b = new BitBuffer(new byte[60], offset1);
					b.Put(dBitBuffer);
					var oldArray = dBitBuffer.ToArray();
					b = b.FromStartToPosition();
					var newArr = b.ToArray();
					CollectionAssert.AreEqual(oldArray, newArr, "offset1=" + offset1 + ", offset2 = " + offset2);
					Assert.IsTrue(dBitBuffer.BufferEquals(b), "offset1=" + offset1 + ", offset2 = " + offset2);
				}
			}
		}

		[Test]
		public void BufferPutBufferTestBounds() {
			for (int offset1 = 0; offset1 < 16; offset1++) {
				for (int offset2 = 0; offset2 < 16; offset2++) {
					for (int length = 2; length < 14; length++) {
						byte[] data = new byte[40];
						for (int i = 0; i < data.Length; i++) data[i] = 0xff;

						var dBitBuffer = new BitBuffer(data, offset2, length);

						var bWrite = dBitBuffer;
						bWrite.Put(false);
						bWrite.Position += length - 2;
						bWrite.Put(false);

						var b = new BitBuffer(new byte[60], offset1);
						b.Put(dBitBuffer);

						var oldArray = dBitBuffer.ToArray();
						b = b.FromStartToPosition();
						var newArr = b.ToArray();
						CollectionAssert.AreEqual(oldArray, newArr, "offset1=" + offset1 + ", offset2 = " + offset2 + ", length=" + length);
						Assert.IsTrue(dBitBuffer.BufferEquals(b), "offset1=" + offset1 + ", offset2 = " + offset2 + ", length=" + length);
					}
				}
			}
		}

		[Test]
		public void SerializingModeTest() {
			var reference = new BitBuffer(new byte[1000]);

			reference.serializerWriteMode = true;
			TestBothSerializingModes(reference);

			reference.serializerWriteMode = false;
			TestBothSerializingModes(reference);
		}

		private void TestBothSerializingModes(BitBuffer b) {
			BitBuffer example = new BitBuffer(new byte[] { 1, 2, 3 });
			if (b.serializerWriteMode) {
				b.Serialize(ref example);
			}
			else {
				var buff = new BitBuffer(new byte[3]);
				b.Serialize(ref buff);
				Assert.IsTrue(example.BufferEquals(buff));
			}

			for (int i = 0; i < 2; i++) {
				bool bTest = false;
				if (b.serializerWriteMode) {
					bTest = i % 2 == 0;
					b.Serialize(ref bTest);
				}
				else {
					b.Serialize(ref bTest);
					Assert.AreEqual(i % 2 == 0, bTest);
				}
			}

			for (int i = 0; i < 10; i++) {
				if (b.serializerWriteMode) {
					b.Serialize(ref i);
				}
				else {
					int j = 0;
					b.Serialize(ref j);
					Assert.AreEqual(i, j);
				}
			}
		}

		[Test]
		public void StringTest() {
			BitBuffer b = new BitBuffer(new byte[10000]);
			List<string> strings = new List<string>() {
				"ABCDEFGHIJKLMNOPQRSTUVWXYZ /0123456789" +
				"abcdefghijklmnopqrstuvwxyz £©µÀÆÖÞßéöÿ" +
				"–—‘“”„†•…‰™œŠŸž€ ΑΒΓΔΩαβγδω АБВГДабвгд" +
				"∀∂∈ℝ∧∪≡∞ ↑↗↨↻⇣ ┐┼╔╘░►☺♀ ﬁ�⑀₂ἠḂӥẄɐː⍎אԱა",
				"\r\n",
				"",
				"ᚻᛖ ᚳᚹᚫᚦ ᚦᚫᛏ ᚻᛖ ᛒᚢᛞᛖ ᚩᚾ ᚦᚫᛗ ᛚᚪᚾᛞᛖ ᚾᚩᚱᚦᚹᛖᚪᚱᛞᚢᛗ ᚹᛁᚦ ᚦᚪ ᚹᛖᛥᚫ"
			};

			Random r = new Random(0);
			for (int i = 0; i < 128; i++) {
				var s = new StringBuilder();
				for (int j = 0; j < i; j++) s.Append((char) r.Next());
				strings.Add(r.ToString());
			}

			List<int> endPositions = new List<int>();
			var b2 = b;
			b2.serializerWriteMode = true;
			foreach (var str in strings) {
				b2.Put(str);
				endPositions.Add(b2.Position);
			}

			for (int offset = 0; offset < 16; offset++) {
				var bCopy = new BitBuffer(b.data, offset);
				var bWrite = bCopy;
				bWrite.serializerWriteMode = true;
				var bRead = bCopy;
				bRead.serializerWriteMode = false;

				for (int i = 0; i < strings.Count; i++) {
					bWrite.Put(strings[i]);
					Assert.AreEqual(endPositions[i], bWrite.Position, "The string is writting a different ammount of bits depending on the offset of the array. This is wrong. offset=" + offset);
				}
				foreach (var str in strings) {
					Assert.AreEqual(str, bRead.GetString());
				}
			}
		}

		[Test]
		public void StringLengthTest() {
			Random r = new Random(0);
			BitBuffer b = new BitBuffer(new byte[10000]);
			List<string> strings = new List<string>();
			for (int i = 0; i < 128; i++) {
				var s = new StringBuilder();
				for (int j = 0; j < i; j++) s.Append((char) r.Next());
				strings.Add(r.ToString());
			}

			for (int offset = 0; offset < 16; offset++) {
				var bCopy = b;
				bCopy.Position += offset;
				bCopy.serializerWriteMode = true;
				var bWriteReal = bCopy;
				var bWriteFalse = bCopy;
				bWriteFalse.simulateWrites = true;

				foreach (var str in strings) {
					bWriteReal.Put(str);
					bWriteFalse.Put(str);
				}
				Assert.AreEqual(bWriteReal.Position, bWriteFalse.Position);
			}
		}
	}
}
