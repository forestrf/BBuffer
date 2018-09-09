using System;
using System.Collections.Generic;
using System.Threading;

namespace BBuffer {
	/// <summary>
	/// Thread safe reference to a pooled byte buffer
	/// </summary>
	internal class PooledBufferHolder {
		[ThreadStatic]
		private static Stack<PooledBufferHolder>[] bufferPool;
		private static readonly Stack<PooledBufferHolder>[] sharedBufferPool = CreateBufferPool();

		private static Stack<PooledBufferHolder>[] CreateBufferPool(byte highestPowerOfTwo = 16) {
			var pool = new Stack<PooledBufferHolder>[highestPowerOfTwo + 1];
			for (int i = 0; i < pool.Length; i++) {
				pool[i] = new Stack<PooledBufferHolder>();
			}
			return pool;
		}

		public byte[] buffer;

		private readonly Thread createdThread;
		private readonly byte byteCountPowerOf2;

		/// <summary>
		/// Increased every time this object is pooled
		/// </summary>
		public int lifeCounter;
		private bool isPooled;
		private readonly bool isGlobalPool;

		public PooledBufferHolder(byte[] buffer, byte byteCountPowerOf2, bool useGlobalPool) {
			this.buffer = buffer;
			this.byteCountPowerOf2 = byteCountPowerOf2;
			createdThread = Thread.CurrentThread;
			isGlobalPool = useGlobalPool;
		}

		public static PooledBufferHolder GetPooledOrNew(int byteCount, bool useGlobalPool = false) {
			if (byteCount > 1 << 16) throw new IndexOutOfRangeException("byteCount is greater than max allowed value");

			byte byteCountPowerOf2 = Log2BitPosition(byteCount);
			PooledBufferHolder obj;
			if (useGlobalPool) {
				lock (sharedBufferPool) {
					obj = GetPooledInternal(byteCountPowerOf2, sharedBufferPool);
				}
			}
			else {
				if (null == bufferPool) bufferPool = CreateBufferPool();
				obj = GetPooledInternal(byteCountPowerOf2, bufferPool);
			}
			if (null == obj) {
				obj = new PooledBufferHolder(new byte[1 << byteCountPowerOf2], byteCountPowerOf2, useGlobalPool);
			}
			return obj;
		}

		private static PooledBufferHolder GetPooledInternal(byte byteCountPowerOf2, Stack<PooledBufferHolder>[] bufferPoolInternal) {
			for (int i = byteCountPowerOf2; i < bufferPoolInternal.Length; i++) {
				var stack = bufferPoolInternal[byteCountPowerOf2];
				if (stack.Count > 0) {
					var obj = stack.Pop();
					obj.isPooled = false;
					return obj;
				}
			}
			return null;
		}

		// TO DO: If too many pooled buffers, use this method remove some of them.
		public void Recycle() {
			if (!isPooled) {
				if (isGlobalPool) {
					lifeCounter++;
					isPooled = true;
					if (byteCountPowerOf2 < sharedBufferPool.Length) {
						lock (sharedBufferPool) {
							sharedBufferPool[byteCountPowerOf2].Push(this);
						}
					}
				}
				else if (Thread.CurrentThread == createdThread) {
					lifeCounter++;
					isPooled = true;
					if (byteCountPowerOf2 < bufferPool.Length) {
						bufferPool[byteCountPowerOf2].Push(this);
					}
				}
			}
		}

		/// <summary>
		/// Answers the question:
		/// What is the smallest value of x that 1 << x is equal or greather than <paramref name="number"/>.
		/// Expected to be used when calling <see cref="GetPooled(byte)"/> giving this method the length of a buffer you want to clone.
		/// </summary>
		internal static byte Log2BitPosition(int number) {
			// Unrolled loop for performance
			if (number <= 1 << 0) return 0;
			if (number <= 1 << 1) return 1;
			if (number <= 1 << 2) return 2;
			if (number <= 1 << 3) return 3;
			if (number <= 1 << 4) return 4;
			if (number <= 1 << 5) return 5;
			if (number <= 1 << 6) return 6;
			if (number <= 1 << 7) return 7;
			if (number <= 1 << 8) return 8;
			if (number <= 1 << 9) return 9;
			if (number <= 1 << 10) return 10;
			if (number <= 1 << 11) return 11;
			if (number <= 1 << 12) return 12;
			if (number <= 1 << 13) return 13;
			if (number <= 1 << 14) return 14;
			if (number <= 1 << 15) return 15;
			return 16;
		}
	}
}
