using System.Collections.Generic;

namespace BBuffer {
	/// <summary>
	/// Thread safe reference to a pooled byte buffer
	/// </summary>
	internal class PooledBufferHolder {
		private static readonly Stack<PooledBufferHolder>[] bufferPool = CreateBufferPool();
		private static Stack<PooledBufferHolder>[] CreateBufferPool(byte highestPowerOfTwo = 16) {
			var bufferPool = new Stack<PooledBufferHolder>[highestPowerOfTwo + 1];
			for (int i = 0; i < bufferPool.Length; i++) {
				bufferPool[i] = new Stack<PooledBufferHolder>();
			}
			return bufferPool;
		}

		public byte[] buffer;

		/// <summary>
		/// Increased every time this object is pooled
		/// </summary>
		public int lifeCounter;

		private readonly byte byteCountPowerOf2;
		private readonly object key = new object();

		private bool isPooled;
		
		public PooledBufferHolder(byte[] buffer, byte byteCountPowerOf2) {
			this.buffer = buffer;
			this.byteCountPowerOf2 = byteCountPowerOf2;
		}

		public static PooledBufferHolder GetPooled(byte byteCountPowerOf2) {
			for (int i = byteCountPowerOf2; i < bufferPool.Length; i++) {
				var stack = bufferPool[byteCountPowerOf2];
				if (stack.Count > 0) {
					lock (stack) {
						if (stack.Count > 0) {
							var obj = stack.Pop();
							obj.isPooled = false;
							return obj;
						}
					}
				}
			}
			return null;
		}

		// TO DO: If too many pooled buffers, use this method remove some of them.
		public void Recycle() {
			if (!isPooled) {
				lock (key) {
					if (!isPooled) {
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
}
