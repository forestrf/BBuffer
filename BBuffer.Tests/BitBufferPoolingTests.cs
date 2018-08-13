using BBuffer;
using NUnit.Framework;
using System;
using System.Threading;

namespace BBufferTests {
	[TestFixture]
	public class BitBufferPoolingTests {
		[Test]
		public void TestRecyclingThreadSafety_LocalPool() {
			TestRecyclingThreadSafetyInternal(false);
		}

		[Test]
		public void TestRecyclingThreadSafety_GobalPool() {
			TestRecyclingThreadSafetyInternal(true);
		}

		private void TestRecyclingThreadSafetyInternal(bool useGlobalPool) {
			ManualResetEvent[] mres = new ManualResetEvent[Environment.ProcessorCount * 4];
			for (int i = 0; i < mres.Length; i++) {
				mres[i] = new ManualResetEvent(false);
				int index = i;
				new Thread(() => {
					try {
						Random randomInitializer = new Random(index);
						for (int j = 0; j < 1000; j++) {
							ushort size = (ushort) randomInitializer.Next();
							var b = BitBuffer.GetPooled(size, useGlobalPool);
							Assert.IsTrue(b.IsValid());
							b.Recycle();
							Assert.IsFalse(b.IsValid());
						}

						for (int j = 0; j < 150; j++) {
							ushort size = (byte) randomInitializer.Next();
							var randomInit = randomInitializer.Next();

							Random r = new Random(randomInit);
							var b = BitBuffer.GetPooled(size, useGlobalPool);
							Assert.IsTrue(b.IsValid());
							var bRead = b;
							var bWrite = b;

							int start = r.Next();
							for (int k = 0; k < b.data.Length / sizeof(int); k++) {
								bWrite.Put(start + k);
							}

							for (int k = 0; k < b.data.Length / sizeof(int); k++) {
								Assert.AreEqual(start + k, bRead.GetInt());
							}
							b.Recycle();
							Assert.IsFalse(b.IsValid());
						}


						mres[index].Set();
					}
					catch (Exception e) {
						for (int j = 0; j < mres.Length; j++) {
							mres[j].Set();
						}
						Assert.Fail(e.ToString());
					}

				}).Start();
			}

			for (int j = 0; j < mres.Length; j++) {
				mres[j].WaitOne();
			}
		}

		[Test]
		public void Log2BitPositionTest() {
			for (ushort i = 0; i < 1 << 14; i++) {
				Console.WriteLine(i + " = 1 << " + GetCeilPowerOfTwo(i) + " = " + (1 << GetCeilPowerOfTwo(i)));
				int reference = 1 << GetCeilPowerOfTwo(i);
				int test = 1 << BitBuffer.Log2BitPosition(i);

				Assert.AreEqual(reference, test, "Number=" + i);
			}


			Random r = new Random(0);
			for (uint i = 0; i < 1024; i++) {
				ushort n = (ushort) r.Next();
				int reference = GetCeilPowerOfTwo(n);
				int test = BitBuffer.Log2BitPosition(n);

				Assert.AreEqual(reference, test, "Number=" + n);
			}
		}
		
		private int GetCeilPowerOfTwo(uint number) {
			for (int i = 0; i < 32; i++) {
				if (number <= 1 << i) {
					return i;
				}
			}
			return 31;
		}

		[Test]
		public void CloneBufferWithPoolTest() {
			Random r = new Random(0);
			for (int offset = 0; offset < 16; offset++) {
				var b = new BitBuffer(new byte[32], offset);
				for (int k = 0; k < (b.data.Length - (int) Math.Ceiling(offset / 8f)) / sizeof(int); k++) {
					b.Put(r.Next());
				}
				var clone = b.CloneUsingPool();

				Assert.IsTrue(b.BufferEquals(clone), "offset=" + offset);

				clone.Recycle();
			}
		}

		[Test]
		public void CloneBufferOffset() {
			BitBuffer b = new BitBuffer(new byte[1 << 16]);
			b.CloneUsingPool();
			for (int i = 0; i < 16; i++) {
				b.absOffset += 1;
				b.CloneUsingPool();
			}
		}
	}
}
