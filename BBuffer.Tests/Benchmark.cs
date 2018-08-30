using BBuffer;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace BBufferTests {
	[TestFixture]
	public class Benchmark {
		public delegate void BenchMethod(int iterations);
		public void Bench(string name, int iterations, BenchMethod method) {
			method(iterations); // Warm up
			var s = Stopwatch.StartNew();
			method(iterations);
			Console.WriteLine(name + ": " + (s.Elapsed.TotalMilliseconds / iterations) * 1000000 + " ns");
		}

		[Test]
		public void BitBufferAllInt() {
			var b = new BitBuffer(new byte[104857600]);

			b.Position = 0;
			Bench("PutInt aligned full", 1000000, (iterations) => {
				for (int i = 0; i < iterations; i++) {
					b.Put(i);
				}
			});

			b.Position = 0;
			Bench("PutInt aligned incomplete", 1000000, (iterations) => {
				for (int i = 0; i < iterations; i++) {
					b.Put(i, 31);
					b.Position++;
				}
			});

			b.Position += 4;
			Bench("PutInt misaligned", 1000000, (iterations) => {
				for (int i = 0; i < iterations; i++) {
					b.Put(i);
				}
			});

			b.Position = 0;
			Bench("GetInt aligned full", 1000000, (iterations) => {
				for (int i = 0; i < iterations; i++) {
					b.GetInt();
				}
			});

			b.Position = 0;
			Bench("GetInt aligned incomplete", 1000000, (iterations) => {
				for (int i = 0; i < iterations; i++) {
					b.GetInt(31);
					b.Position++;
				}
			});

			b.Position += 4;
			Bench("GetInt misaligned", 1000000, (iterations) => {
				for (int i = 0; i < iterations; i++) {
					b.GetInt();
				}
			});
		}

		[Test]
		public void ByteBufferAllInt() {
			var b = new ByteBuffer(new byte[104857600]);

			b.Position = 0;
			Bench("PutInt aligned", 1000000, (iterations) => {
				for (int i = 0; i < iterations; i++) {
					b.Put(i);
				}
			});

			b.Position = 0;
			Bench("GetInt aligned", 1000000, (iterations) => {
				for (int i = 0; i < iterations; i++) {
					b.GetInt();
				}
			});
		}

		[Test]
		public void BitBufferAllUInt() {
			var b = new BitBuffer(new byte[104857600]);

			b.Position = 0;
			Bench("PutUInt aligned", 1000000, (iterations) => {
				for (uint i = 0; i < iterations; i++) {
					b.Put(i);
				}
			});

			b.Position += 4;
			Bench("PutUInt misaligned", 1000000, (iterations) => {
				for (uint i = 0; i < iterations; i++) {
					b.Put(i);
				}
			});

			b.Position = 0;
			Bench("GetUInt aligned", 1000000, (iterations) => {
				for (uint i = 0; i < iterations; i++) {
					b.GetUInt();
				}
			});

			b.Position += 4;
			Bench("GetUInt misaligned", 1000000, (iterations) => {
				for (uint i = 0; i < iterations; i++) {
					b.GetUInt();
				}
			});
		}

		[Test]
		public void AllGetBufferAndRecycle() {
			Bench("GetPooled and Recycle", 1000000, (iterations) => {
				for (uint i = 0; i < iterations; i++) {
					BitBuffer.GetPooled(1024).Recycle();
				}
			});
		}

		[Test]
		public void If() {
			Bench("Baseline", 100000000, (iterations) => {
				for (uint i = 0; i < iterations; i++) {
				}
			});
			Bench("Ifs", 100000000, (iterations) => {
				bool someCondition = false;
				for (uint i = 0; i < iterations; i++) {
					if (someCondition) continue;
					if (someCondition) continue;
					if (someCondition) continue;
					if (someCondition) continue;
					if (someCondition) continue;
					if (someCondition) continue;
					if (someCondition) continue;
					if (someCondition) continue;
					if (someCondition) continue;
					if (someCondition) continue;
				}
			});
		}
	}
}
