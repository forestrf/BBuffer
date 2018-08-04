using BBuffer;
using NUnit.Framework;
using System;
using System.Threading;

namespace BBufferTests {
	[TestFixture]
	public class BitBufferPoolingTests {
		[Test]
		public void TestRecyclingThreadSafety() {
			ManualResetEvent[] mres = new ManualResetEvent[Environment.ProcessorCount * 4];
			for (int i = 0; i < mres.Length; i++) {
				mres[i] = new ManualResetEvent(false);
				int index = i;
				new Thread(() => {
					try {
						Random randomInitializer = new Random(index);
						for (int j = 0; j < 50; j++) {
							byte powerOfTwoLength = (byte) (randomInitializer.Next() % 17);
							var randomInit = randomInitializer.Next();

							Random r = new Random(randomInit);
							var b = BitBuffer.GetPooled(powerOfTwoLength);
							Assert.IsTrue(b.IsValid());
							var bRead = b;
							var bWrite = b;

							for (int k = 0; k < b.data.Length / sizeof(int); k++) {
								bWrite.Put(r.Next());
							}

							r = new Random(randomInit);
							for (int k = 0; k < b.data.Length / sizeof(int); k++) {
								Assert.AreEqual(r.Next(), bRead.GetInt());
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
	}
}
