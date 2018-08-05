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

		private static Stack<PooledBufferHolder>[] CreateBufferPool(byte highestPowerOfTwo = 16) {
			var bufferPool = new Stack<PooledBufferHolder>[highestPowerOfTwo + 1];
			for (int i = 0; i < bufferPool.Length; i++) {
				bufferPool[i] = new Stack<PooledBufferHolder>();
			}
			return bufferPool;
		}

		public byte[] buffer;

		private readonly Thread createdThread;
		private readonly byte byteCountPowerOf2;

		/// <summary>
		/// Increased every time this object is pooled
		/// </summary>
		public int lifeCounter;
		private bool isPooled;

		public PooledBufferHolder(byte[] buffer, byte byteCountPowerOf2) {
			this.buffer = buffer;
			this.byteCountPowerOf2 = byteCountPowerOf2;
			createdThread = Thread.CurrentThread;
		}

		public static PooledBufferHolder GetPooled(byte byteCountPowerOf2) {
			if (null == bufferPool) bufferPool = CreateBufferPool();

			for (int i = byteCountPowerOf2; i < bufferPool.Length; i++) {
				var stack = bufferPool[byteCountPowerOf2];
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
			if (!isPooled && Thread.CurrentThread == createdThread) {
				lifeCounter++;
				isPooled = true;
				if (byteCountPowerOf2 < bufferPool.Length) {
					bufferPool[byteCountPowerOf2].Push(this);
				}
			}
		}
	}
}
