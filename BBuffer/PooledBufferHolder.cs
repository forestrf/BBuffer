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

		public static PooledBufferHolder GetPooled(byte byteCountPowerOf2, bool useGlobalPool = false) {
			if (useGlobalPool) {
				lock (sharedBufferPool) {
					return GetPooledInternal(byteCountPowerOf2, sharedBufferPool);
				}
			}
			else {
				if (null == bufferPool) bufferPool = CreateBufferPool();
				return GetPooledInternal(byteCountPowerOf2, bufferPool);
			}
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
	}
}
