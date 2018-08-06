using System;
using System.Text;

namespace BBuffer {
	/// <summary>
	/// Struct that wraps an <cref=data>array</cref> and writes/reads to it in Little Endian.
	/// It works on the hole array or on a subset of it.
	/// Because it is an struct, you don't need to pool it but you may need to pass it to other methods using the ref keyword
	/// ALL METHODS are GUARANTEED to write/read the same ammount of bits independent from the bit position or offset
	/// </summary>
	public struct BitBuffer {
		#region pooling
		private readonly int lifeCounter;
		private PooledBufferHolder pooledBufferHolder;

		internal BitBuffer(PooledBufferHolder pooledBuffeHolder, int length) : this() {
			this.pooledBufferHolder = pooledBuffeHolder;
			lifeCounter = pooledBuffeHolder.lifeCounter;
			data = pooledBuffeHolder.buffer;
			absLength = length;
		}

		/// <summary>
		/// Check if this buffer can be used or, if it is a pooled buffer, if it has already been pooled.
		/// In case it is not valid it MUST NOT be used.
		/// </summary>
		public bool IsValid() {
			return null != data && (null == pooledBufferHolder || lifeCounter == pooledBufferHolder.lifeCounter);
		}

		/// <summary>
		/// Get a buffer from the pool. It should be recycled later calling at <see cref="Recycle"/>
		/// </summary>
		/// <param name="byteCountPowerOf2">minimum byte count = 1 << this</param>
		public static BitBuffer GetPooled(ushort size) {
			byte byteCountPowerOf2 = Log2BitPosition(size);
			var obj = PooledBufferHolder.GetPooled(byteCountPowerOf2);
			if (null == obj) {
				obj = new PooledBufferHolder(new byte[1 << byteCountPowerOf2], byteCountPowerOf2);
			}
			return new BitBuffer(obj, size);
		}

		/// <summary>
		/// Clone a buffer to a different byte array using the pool.
		/// The length of this buffer with be used to decide the length of the needed byte array.
		/// The returned buffer will be cropped
		/// </summary>
		/// <returns></returns>
		public BitBuffer CloneUsingPool() {
			var b = GetPooled((ushort) (Math.Min(Length + 8, ushort.MaxValue)));
			b.absOffset = b.absPosition = absPosition & 0x7;
			b.Put(this);
			return b.GetCropToCurrentPosition();
		}

		/// <summary>
		/// Answers the question:
		/// What is the smallest value of x that 1 << x is equal or greather than <paramref name="number"/>.
		/// Expected to be used when calling <see cref="GetPooled(byte)"/> giving this method the length of a buffer you want to clone.
		/// </summary>
		internal static byte Log2BitPosition(ushort number) {
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

		/// <summary>
		/// Recycle a booled a buffer.
		/// This method will not fail even if it didn't need to be recycled.
		/// </summary>
		public void Recycle() {
			if (null != pooledBufferHolder && lifeCounter == pooledBufferHolder.lifeCounter) {
				pooledBufferHolder.Recycle();
			}
		}
		#endregion

		/// <summary>
		/// Wrapped array
		/// </summary>
		public byte[] data;

		/// <summary>
		/// Position of the read/write cursor in <see cref="data"/>. In bits
		/// </summary>
		public int absPosition;

		/// <summary>
		/// Position of the 0-index for this section inside <see cref="data"/>. In bits
		/// </summary>
		public int absOffset;

		/// <summary>
		/// usable bits in <see cref="data"/> from 0. In bits
		/// </summary>
		public int absLength;

		/// <summary>
		/// If true, writting will only advance <see cref="Position"/>/<see cref="absPosition"/>
		/// </summary>
		public bool simulateWrites;

		/// <summary>
		/// If true, serializer methods will read, otherwise they will write
		/// Based on https://gafferongames.com/post/serialization_strategies/
		/// </summary>
		public bool serializerWriteMode;



		public BitBuffer(byte[] buffer) : this(buffer, 0, null != buffer ? buffer.Length * 8 : 0) { }
		/// <param name="offset">in bits</param>
		public BitBuffer(byte[] buffer, int offset) : this(buffer, offset, null != buffer ? buffer.Length * 8 - offset : 0) { }
		/// <param name="offset">in bits</param>
		/// <param name="length">in bits</param>
		public BitBuffer(byte[] buffer, int offset, int length) : this() {
			data = buffer;
			absPosition = offset;
			absLength = offset + length;
			absOffset = offset;
		}

		public void SkipBytes(int numberOfBytes) {
			SkipBits(8 * numberOfBytes);
		}

		public void SkipBits(int numberOfBits) {
			absPosition += numberOfBits;
		}

		/// <summary>Relative to <see cref="absOffset"/>. In bits</summary>
		public int Position {
			get { return absPosition - absOffset; }
			set { absPosition = value + absOffset; }
		}

		/// <summary>Relative to <see cref="absOffset"/>. In bits</summary>
		public int Length {
			get { return absLength - absOffset; }
			set {
				absLength = value + absOffset;
				if (absPosition > absLength) absPosition = absLength;
			}
		}

		/// <summary>Relative to <see cref="absOffset"/>. In bytes</summary>
		public byte this[int index] {
			get {
				return data[absOffset + index];
			}
			set {
				data[absOffset + index] = value;
			}
		}

		public int Remaining() {
			return absLength - absPosition;
		}

		public BitBuffer slice() {
			BitBuffer b = new BitBuffer(data);
			b.absOffset = b.absPosition = absPosition;
			b.absLength = absLength;
			return b;
		}
		public BitBuffer flip() {
			absLength = absPosition;
			absPosition = absOffset;
			return this;
		}

		public BitBuffer GetCropToCurrentPosition() {
			BitBuffer b = new BitBuffer(data);
			b.absOffset = b.absPosition = absOffset;
			b.absLength = absPosition;
			return b;
		}

		public void Rewind() {
			absPosition = 0;
		}
		public void Rewind(int bits) {
			absPosition -= bits;
		}

		public byte[] ToArray() {
			byte[] copy = new byte[(int) Math.Ceiling(Length / 8f)];
			GetBits(copy, 0, Length);
			return copy;
		}

		public bool BufferEquals(BitBuffer other) {
			if (Length != other.Length)
				return false;
			for (int i = 0; i < Length / 8; i++)
				if (GetByteAt(i * 8) != other.GetByteAt(i * 8))
					return false;
			if (GetByteAt(~0x7 & Length, 0x7 & Length) != other.GetByteAt(~0x7 & Length, 0x7 & Length))
				return false;
			return true;
		}

		public bool HasData() {
			return null != data;
		}

		#region PutMethods
		public void Put(byte value, int bitCount = sizeof(byte) * 8) {
			if (simulateWrites) {
				absPosition += bitCount;
				return;
			}
			FastBit.Byte.Write(value, data, absPosition, bitCount);
			absPosition += bitCount;
		}
		public void PutAt(int offset, byte value, int bitCount = sizeof(byte) * 8) {
			if (simulateWrites) return;
			FastBit.Byte.Write(value, data, absOffset + offset, bitCount);
		}

		public void Put(bool value) {
			if (simulateWrites) {
				absPosition += 1;
				return;
			}
			Put(value ? (byte) 1 : (byte) 0, 1);
		}
		public void PutAt(int offset, bool value) {
			if (simulateWrites) return;
			PutAt(offset, value ? (byte) 1 : (byte) 0, 1);
		}

		public void Put(short value, int bitCount = sizeof(short) * 8) {
			if (simulateWrites) {
				absPosition += bitCount;
				return;
			}
			new FastBit.UShort((ushort) value).Write(data, absPosition, bitCount);
			absPosition += bitCount;
		}
		public void PutAt(int offset, short value, int bitCount = sizeof(short) * 8) {
			if (simulateWrites) return;
			new FastBit.UShort((ushort) value).Write(data, absOffset + offset, bitCount);
		}

		public void Put(ushort value, int bitCount = sizeof(ushort) * 8) {
			if (simulateWrites) {
				absPosition += bitCount;
				return;
			}
			new FastBit.UShort(value).Write(data, absPosition, bitCount);
			absPosition += bitCount;
		}
		public void PutAt(int offset, ushort value, int bitCount = sizeof(ushort) * 8) {
			if (simulateWrites) return;
			new FastBit.UShort(value).Write(data, absOffset + offset, bitCount);
		}

		public void Put(int value, int bitCount = sizeof(int) * 8) {
			if (simulateWrites) {
				absPosition += bitCount;
				return;
			}
			new FastBit.UInt((uint) value).Write(data, absPosition, bitCount);
			absPosition += bitCount;
		}
		public void PutAt(int offset, int value, int bitCount = sizeof(int) * 8) {
			if (simulateWrites) return;
			new FastBit.UInt((uint) value).Write(data, absOffset + offset, bitCount);
		}

		public void Put(uint value, int bitCount = sizeof(uint) * 8) {
			if (simulateWrites) {
				absPosition += bitCount;
				return;
			}
			new FastBit.UInt(value).Write(data, absPosition, bitCount);
			absPosition += bitCount;
		}
		public void PutAt(int offset, uint value, int bitCount = sizeof(uint) * 8) {
			if (simulateWrites) return;
			new FastBit.UInt(value).Write(data, absOffset + offset, bitCount);
		}

		public void Put(long value, int bitCount = sizeof(long) * 8) {
			if (simulateWrites) {
				absPosition += bitCount;
				return;
			}
			new FastBit.ULong((ulong) value).Write(data, absPosition, bitCount);
			absPosition += bitCount;
		}
		public void PutAt(int offset, long value, int bitCount = sizeof(long) * 8) {
			if (simulateWrites) return;
			new FastBit.ULong((ulong) value).Write(data, absOffset + offset, bitCount);
		}

		public void Put(ulong value, int bitCount = sizeof(ulong) * 8) {
			if (simulateWrites) {
				absPosition += bitCount;
				return;
			}
			new FastBit.ULong(value).Write(data, absPosition, bitCount);
			absPosition += bitCount;
		}
		public void PutAt(int offset, ulong value, int bitCount = sizeof(ulong) * 8) {
			if (simulateWrites) return;
			new FastBit.ULong(value).Write(data, absOffset + offset, bitCount);
		}

		public void Put(float value) {
			if (simulateWrites) {
				absPosition += sizeof(float) * 8;
				return;
			}
			PutAt(Position, value);
			absPosition += sizeof(float) * 8;
		}
		public void PutAt(int offset, float value) {
			if (simulateWrites) return;
			const int bitCount = sizeof(float) * 8;
			if (!BitConverter.IsLittleEndian) value = new FastByte.Float(value).GetReversed();
			var f = new FastByte.Float(value);
			new FastBit.UInt(f.b0, f.b1, f.b2, f.b3).Write(data, absOffset + offset, bitCount);
		}

		public void Put(double value) {
			if (simulateWrites) {
				absPosition += sizeof(double) * 8;
				return;
			}
			PutAt(Position, value);
			absPosition += sizeof(double) * 8;
		}
		public void PutAt(int offset, double value) {
			if (simulateWrites) return;
			const int bitCount = sizeof(double) * 8;
			if (!BitConverter.IsLittleEndian) value = new FastByte.Double(value).GetReversed();
			var f = new FastByte.Double(value);
			new FastBit.ULong(f.b0, f.b1, f.b2, f.b3, f.b4, f.b5, f.b6, f.b7).Write(data, absOffset + offset, bitCount);
		}


		public void PutVariableLength(int value) {
			uint zigzag = ((uint) value << 1) ^ (uint) (value >> (sizeof(int) * 8 - 1));
			PutVariableLength(zigzag);
		}
		public int PutVariableLengthAt(int offset, int value) {
			uint zigzag = ((uint) value << 1) ^ (uint) (value >> (sizeof(int) * 8 - 1));
			return PutVariableLengthAt(offset, zigzag);
		}

		public void PutVariableLength(uint value) {
			absPosition += PutVariableLengthAt(Position, value) * 8;
		}
		public int PutVariableLengthAt(int offset, uint value) {
			return PutVariableLengthAt(offset, (ulong) value);
		}

		public void PutVariableLength(long value) {
			ulong zigzag = ((ulong) value << 1) ^ (ulong) (value >> (sizeof(long) * 8 - 1));
			PutVariableLength(zigzag);
		}
		public int PutVariableLengthAt(int offset, long value) {
			ulong zigzag = ((ulong) value << 1) ^ (ulong) (value >> (sizeof(long) * 8 - 1));
			return PutVariableLengthAt(offset, zigzag);
		}

		public void PutVariableLength(ulong value) {
			absPosition += PutVariableLengthAt(Position, value) * 8;
		}
		public int PutVariableLengthAt(int offset, ulong value) {
			int bytes = 0;
			while (value >= 0x80) {
				PutAt(offset, (byte) (0x80 | value));
				offset += 8;
				bytes++;
				value >>= 7;
			}
			PutAt(offset, (byte) value);
			bytes++;
			return bytes;
		}

		public static int BitsNeededForVariableLength(int value) {
			uint zigzag = (uint) (value << 1) ^ (uint) (value >> (sizeof(int) * 8 - 1));
			return BitsNeededForVariableLength(zigzag);
		}
		public static int BitsNeededForVariableLength(long value) {
			ulong zigzag = (ulong) (value << 1) ^ (ulong) (value >> (sizeof(long) * 8 - 1));
			return BitsNeededForVariableLength(zigzag);
		}
		public static int BitsNeededForVariableLength(uint value) {
			return BitsNeededForVariableLength((ulong) value);
		}
		public static int BitsNeededForVariableLength(ulong value) {
			int bytes = 0;
			while (value >= 0x80) {
				bytes++;
				value >>= 7;
			}
			bytes++;
			return bytes * 8;
		}


		public void PutRanged(float value, float min, float max, int numberOfBits) {
			PutRangedAt(Position, value, min, max, numberOfBits);
			absPosition += numberOfBits;
		}
		public void PutRangedAt(int offset, float value, float min, float max, int numberOfBits) {
			if (simulateWrites) return;
			if (0 == numberOfBits) return;
			double unit = (value - min) / (max - min);
			uint maxVal = uint.MaxValue >> (32 - numberOfBits);
			PutAt(offset, (uint) Math.Round(unit * maxVal), numberOfBits);
		}

		public void PutRanged(int value, int min, int max) {
			int numberOfBits;
			PutRangedAt(Position, value, min, max, out numberOfBits);
			absPosition += numberOfBits;
		}
		public void PutRangedAt(int offset, int value, int min, int max) {
			int numberOfBits;
			PutRangedAt(offset, value, min, max, out numberOfBits);
		}
		public void PutRangedAt(int offset, int value, int min, int max, out int numberOfBits) {
			numberOfBits = BitsOccupiedByRange(min, max);
			uint rvalue = (uint) (value - min);
			PutAt(offset, rvalue, numberOfBits);
		}

		public void PutRanged(long value, long min, long max) {
			int numberOfBits;
			PutRangedAt(Position, value, min, max, out numberOfBits);
			absPosition += numberOfBits;
		}
		public void PutRangedAt(int offset, long value, long min, long max) {
			int numberOfBits;
			PutRangedAt(offset, value, min, max, out numberOfBits);
		}
		public void PutRangedAt(int offset, long value, long min, long max, out int numberOfBits) {
			numberOfBits = BitsOccupiedByRange(min, max);
			ulong rangedValue = (ulong) (value - min);
			PutAt(offset, rangedValue, numberOfBits);
		}

		public static int BitsOccupiedByRange(int min, int max) {
			return BitsOccupied((uint) (max - min));
		}
		public static int BitsOccupiedByRange(long min, long max) {
			return BitsOccupied((ulong) (max - min));
		}

		public static int BitsOccupied(uint value) {
			int bits = 0;
			while (value > 0) {
				bits++;
				value >>= 1;
			}
			return bits;
		}
		public static int BitsOccupied(ulong value) {
			int bits = 0;
			while (value > 0) {
				bits++;
				value >>= 1;
			}
			return bits;
		}


		public void Put(byte[] v) {
			Put(v, 0, v.Length * 8);
		}
		public void Put(byte[] src, int srcOffset, int length) {
			if (simulateWrites) {
				absPosition += length;
				return;
			}
			Put(new BitBuffer(src, srcOffset, length));
		}
		public void PutAt(int offset, BitBuffer src) {
			int srcByteAlignment = 0x7 & src.absOffset;
			int dstByteAlignment = 0x7 & (absOffset + offset);

			bool srcArrIsByteAligned = 0 == srcByteAlignment;
			bool dstArrIsByteAligned = 0 == dstByteAlignment;

			if (srcByteAlignment == dstByteAlignment && !srcArrIsByteAligned) {
				// Both have the same alineation but are not byte aligned. Copy first and last byte manually and the rest using blockcopy
				int bitsToByteAlign = 8 - srcByteAlignment;
				int bitsToWrite = src.Length >= bitsToByteAlign ? bitsToByteAlign : src.Length;
				PutAt(offset, src.GetByteAt(0, bitsToWrite), bitsToWrite);
				if (src.Length > bitsToByteAlign) {
					PutAt(offset + bitsToByteAlign, new BitBuffer(src.data, src.absOffset + bitsToByteAlign, src.Length - bitsToByteAlign));
				}
			}
			else {
				int writtenLength = ~0x7 & src.Length;
				if (writtenLength > 0) {
					if (srcArrIsByteAligned && dstArrIsByteAligned) {
						Buffer.BlockCopy(src.data, src.absOffset / 8, data, (absOffset + offset) / 8, src.Length / 8);
					}
					else {
						for (int i = 0; i < src.Length / 8; i++) {
							PutAt(offset + i * 8, src.GetByteAt(i * 8));
						}
					}
				}
				int lastBitsCount = 0x7 & src.Length;
				if (0 != lastBitsCount) {
					byte lastByte = src.GetByteAt(writtenLength, lastBitsCount);
					PutAt(offset + writtenLength, lastByte, lastBitsCount);
				}
			}
		}
		public void Put(BitBuffer bb) {
			if (simulateWrites) {
				absPosition += bb.Length;
				return;
			}
			PutAt(Position, bb);
			absPosition += bb.Length;
		}
		public void Put(string str) {
			int bytesNeeded = Encoding.UTF8.GetByteCount(str);
			PutVariableLength((uint) (ushort) bytesNeeded);
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			Put(bytes);
		}
		#endregion

		#region GetMethods
		public byte GetByte(int bitCount = sizeof(byte) * 8) {
			byte res = FastBit.Byte.Read(data, absPosition, bitCount);
			absPosition += bitCount;
			return res;
		}
		public byte GetByteAt(int offset, int bitCount = sizeof(byte) * 8) {
			return FastBit.Byte.Read(data, absOffset + offset, bitCount);
		}

		public bool GetBool() {
			return 1 == GetByte(1);
		}
		public bool GetBoolAt(int offset) {
			return 1 == GetByteAt(offset, 1);
		}

		public short GetShort(int bitCount = sizeof(short) * 8) {
			short result = (short) new FastBit.UShort().Read(data, absPosition, bitCount);
			absPosition += bitCount;
			return result;
		}
		public short GetShortAt(int offset, int bitCount = sizeof(short) * 8) {
			return (short) new FastBit.UShort().Read(data, absOffset + offset, bitCount);
		}

		public ushort GetUShort(int bitCount = sizeof(ushort) * 8) {
			ushort v = new FastBit.UShort().Read(data, absPosition, bitCount);
			absPosition += bitCount;
			return v;
		}
		public ushort GetUShortAt(int offset, int bitCount = sizeof(ushort) * 8) {
			return new FastBit.UShort().Read(data, absOffset + offset, bitCount);
		}

		public int GetInt(int bitCount = sizeof(int) * 8) {
			int result = (int) new FastBit.UInt().Read(data, absPosition, bitCount);
			absPosition += bitCount;
			return result;
		}
		public int GetIntAt(int offset, int bitCount = sizeof(int) * 8) {
			return (int) new FastBit.UInt().Read(data, absOffset + offset, bitCount);
		}

		public uint GetUInt(int bitCount = sizeof(uint) * 8) {
			uint result = new FastBit.UInt().Read(data, absPosition, bitCount);
			absPosition += bitCount;
			return result;
		}
		public uint GetUIntAt(int offset, int bitCount = sizeof(uint) * 8) {
			return new FastBit.UInt().Read(data, absOffset + offset, bitCount);
		}

		public long GetLong(int bitCount = sizeof(long) * 8) {
			long result = (long) new FastBit.ULong().Read(data, absPosition, bitCount);
			absPosition += bitCount;
			return result;
		}
		public long GetLongAt(int offset, int bitCount = sizeof(long) * 8) {
			return (long) new FastBit.ULong().Read(data, absOffset + offset, bitCount);
		}

		public ulong GetULong(int bitCount = sizeof(long) * 8) {
			ulong result = new FastBit.ULong().Read(data, absPosition, bitCount);
			absPosition += bitCount;
			return result;
		}
		public ulong GetULongAt(int offset, int bitCount = sizeof(long) * 8) {
			return new FastBit.ULong().Read(data, absOffset + offset, bitCount);
		}

		public float GetFloat() {
			float result = GetFloatAt(Position);
			absPosition += sizeof(float) * 8;
			return result;
		}
		public float GetFloatAt(int offset) {
			const int bitCount = sizeof(float) * 8;
			var u = new FastBit.UInt();
			u.Read(data, absOffset + offset, bitCount);
			var f = new FastByte.Float(u.b0, u.b1, u.b2, u.b3);
			return !BitConverter.IsLittleEndian ? f.GetReversed() : f.value;
		}

		public double GetDouble() {
			double result = GetDoubleAt(Position);
			absPosition += sizeof(double) * 8;
			return result;
		}
		public double GetDoubleAt(int offset) {
			const int bitCount = sizeof(double) * 8;
			var u = new FastBit.ULong();
			u.Read(data, absOffset + offset, bitCount);
			var f = new FastByte.Double(u.b0, u.b1, u.b2, u.b3, u.b4, u.b5, u.b6, u.b7);
			return !BitConverter.IsLittleEndian ? f.GetReversed() : f.value;
		}

		/// <param name="dstOffset">in bits</param>
		/// <param name="destination">target of the copy, a non null array with enough length</param>
		/// <param name="lenght">in bits</param>
		public void GetBits(byte[] destination, int dstOffset, int lenght) {
			var toPut = new BitBuffer(data, absOffset, Math.Min(lenght, Length));
			new BitBuffer(destination, dstOffset).Put(toPut);
		}

		/// <param name="length">in bits</param>
		public BitBuffer GetBits(int length) {
			BitBuffer b = new BitBuffer(data, absPosition, length);
			absPosition += length;
			return b;
		}

		public BitBuffer GetBitsAt(int offset, int length) {
			return new BitBuffer(data, absOffset + offset, length);
		}

		public int GetIntVariableLength() {
			int bytes;
			return GetIntVariableLength(out bytes);
		}
		public int GetIntVariableLength(out int bytes) {
			uint zigzag = GetUIntVariableLength(out bytes);
			return (int) ((zigzag >> 1) ^ -(zigzag & 1));
		}
		public int GetIntVariableLengthAt(int offset) {
			int bytes;
			return GetIntVariableLengthAt(offset, out bytes);
		}
		public int GetIntVariableLengthAt(int offset, out int bytes) {
			uint zigzag = GetUIntVariableLengthAt(offset, out bytes);
			return (int) ((zigzag >> 1) ^ -(zigzag & 1));
		}

		public uint GetUIntVariableLength() {
			int bytes;
			return GetUIntVariableLength(out bytes);
		}
		public uint GetUIntVariableLength(out int bytes) {
			uint value = GetUIntVariableLengthAt(Position, out bytes);
			absPosition += bytes * 8;
			return value;
		}
		public uint GetUIntVariableLengthAt(int offset) {
			int bytes;
			return (uint) GetULongVariableLengthAt(offset, out bytes);
		}
		public uint GetUIntVariableLengthAt(int offset, out int bytes) {
			return (uint) GetULongVariableLengthAt(offset, out bytes);
		}

		public long GetLongVariableLength() {
			int bytes;
			return GetLongVariableLength(out bytes);
		}
		public long GetLongVariableLength(out int bytes) {
			ulong zigzag = GetULongVariableLength(out bytes);
			const int bitSign = sizeof(long) * 8 - 1;
			long xor = ((long) (zigzag << bitSign)) >> bitSign;
			return ((long) (zigzag >> 1)) ^ xor;
		}
		public long GetLongVariableLengthAt(int offset) {
			int bytes;
			return GetLongVariableLengthAt(offset, out bytes);
		}
		public long GetLongVariableLengthAt(int offset, out int bytes) {
			ulong zigzag = GetULongVariableLengthAt(offset, out bytes);
			const int bitSign = sizeof(long) * 8 - 1;
			long xor = ((long) (zigzag << bitSign)) >> bitSign;
			return ((long) (zigzag >> 1)) ^ xor;
		}

		public ulong GetULongVariableLength() {
			int bytes;
			return GetULongVariableLength(out bytes);
		}
		public ulong GetULongVariableLength(out int bytes) {
			ulong value = GetULongVariableLengthAt(Position, out bytes);
			absPosition += bytes * 8;
			return value;
		}
		public ulong GetULongVariableLengthAt(int offset) {
			int bytes;
			return GetULongVariableLengthAt(offset, out bytes);
		}
		public ulong GetULongVariableLengthAt(int offset, out int bytes) {
			ulong value = 0;
			int bitOffset = 0;
			bytes = 0;
			while (absLength - absPosition >= 8) {
				byte b = GetByteAt(offset + bytes * 8);
				value |= (0x7ful & b) << bitOffset;
				bitOffset += 7;
				bytes++;
				if (0 == (0x80 & b))
					return value;
			}

			// Malformed
			return value;
		}


		public float GetRangedFloat(float min, float max, int numberOfBits) {
			float value = GetRangedFloatAt(Position, min, max, numberOfBits);
			absPosition += numberOfBits;
			return value;
		}
		public float GetRangedFloatAt(int offset, float min, float max, int numberOfBits) {
			if (0 == numberOfBits) return min;
			uint maxVal = uint.MaxValue >> (32 - numberOfBits);
			uint encodedVal = GetUIntAt(offset, numberOfBits);
			float unit = encodedVal / (float) maxVal;
			return min + unit * (max - min);
		}

		public int GetRangedInt(int min, int max) {
			int numBits = BitsOccupiedByRange(min, max);
			int rvalue = (int) GetUInt(numBits);
			return min + rvalue;
		}
		public int GetRangedIntAt(int offset, int min, int max) {
			int numBits = BitsOccupiedByRange(min, max);
			int rvalue = (int) GetUIntAt(offset, numBits);
			return min + rvalue;
		}

		public long GetRangedLong(long min, long max) {
			int numBits = BitsOccupiedByRange(min, max);
			long rvalue = (long) GetUInt(numBits);
			return min + rvalue;
		}
		public long GetRangedLongAt(int offset, long min, long max) {
			int numBits = BitsOccupiedByRange(min, max);
			long rvalue = (long) GetUIntAt(offset, numBits);
			return min + rvalue;
		}
		public string GetString() {
			ushort length = (ushort) GetUIntVariableLength();
			var array = GetBits(length * 8).ToArray();
			return Encoding.UTF8.GetString(array, 0, length);
		}
		#endregion

		#region SerializeMethods
		public void Serialize(ref byte value, int bitCount = sizeof(byte) * 8) {
			if (serializerWriteMode) Put(value, bitCount);
			else value = GetByte(bitCount);
		}
		public void SerializeAt(int offset, ref byte value, int bitCount = sizeof(byte) * 8) {
			if (serializerWriteMode) PutAt(value, bitCount);
			else value = GetByteAt(offset, bitCount);
		}

		public void Serialize(ref bool value) {
			if (serializerWriteMode) Put(value);
			else value = GetBool();
		}
		public void SerializeAt(int offset, ref bool value) {
			if (serializerWriteMode) PutAt(offset, value);
			else value = GetBoolAt(offset);
		}

		public void Serialize(ref short value, int bitCount = sizeof(short) * 8) {
			if (serializerWriteMode) Put(value, bitCount);
			else value = GetShort(bitCount);
		}
		public void SerializeAt(int offset, ref short value, int bitCount = sizeof(short) * 8) {
			if (serializerWriteMode) PutAt(offset, value, bitCount);
			else value = GetShortAt(offset, bitCount);
		}

		public void Serialize(ref ushort value, int bitCount = sizeof(ushort) * 8) {
			if (serializerWriteMode) Put(value, bitCount);
			else value = GetUShort(bitCount);
		}
		public void SerializeAt(int offset, ref ushort value, int bitCount = sizeof(ushort) * 8) {
			if (serializerWriteMode) PutAt(offset, value, bitCount);
			else value = GetUShortAt(offset, bitCount);
		}

		public void Serialize(ref int value, int bitCount = sizeof(int) * 8) {
			if (serializerWriteMode) Put(value, bitCount);
			else value = GetInt(bitCount);
		}
		public void SerializeAt(int offset, ref int value, int bitCount = sizeof(int) * 8) {
			if (serializerWriteMode) PutAt(offset, value, bitCount);
			else value = GetIntAt(offset, bitCount);
		}

		public void Serialize(ref uint value, int bitCount = sizeof(uint) * 8) {
			if (serializerWriteMode) Put(value, bitCount);
			else value = GetUInt(bitCount);
		}
		public void SerializeAt(int offset, ref uint value, int bitCount = sizeof(uint) * 8) {
			if (serializerWriteMode) PutAt(offset, value, bitCount);
			else value = GetUIntAt(offset, bitCount);
		}

		public void Serialize(ref long value, int bitCount = sizeof(long) * 8) {
			if (serializerWriteMode) Put(value, bitCount);
			else value = GetLong(bitCount);
		}
		public void SerializeAt(int offset, ref long value, int bitCount = sizeof(long) * 8) {
			if (serializerWriteMode) PutAt(offset, value, bitCount);
			else value = GetLongAt(offset, bitCount);
		}

		public void Serialize(ref ulong value, int bitCount = sizeof(ulong) * 8) {
			if (serializerWriteMode) Put(value, bitCount);
			else value = GetULong(bitCount);
		}
		public void SerializeAt(int offset, ref ulong value, int bitCount = sizeof(ulong) * 8) {
			if (serializerWriteMode) PutAt(offset, value, bitCount);
			else value = GetULongAt(offset, bitCount);
		}

		public void Serialize(ref float value) {
			if (serializerWriteMode) Put(value);
			else value = GetFloat();
		}
		public void SerializeAt(int offset, ref float value) {
			if (serializerWriteMode) PutAt(offset, value);
			else value = GetFloatAt(offset);
		}

		public void Serialize(ref double value) {
			if (serializerWriteMode) Put(value);
			else value = GetDouble();
		}
		public void SerializeAt(int offset, ref double value) {
			if (serializerWriteMode) PutAt(offset, value);
			else value = GetDoubleAt(offset);
		}


		public void SerializeVariableLength(ref int value) {
			if (serializerWriteMode) PutVariableLength(value);
			else value = GetIntVariableLength();
		}
		public int SerializeVariableLengthAt(int offset, ref int value) {
			int bytes;
			if (serializerWriteMode) bytes = PutVariableLengthAt(offset, value);
			else value = GetIntVariableLengthAt(offset, out bytes);
			return bytes;
		}

		public void SerializeVariableLength(ref uint value) {
			if (serializerWriteMode) PutVariableLength(value);
			else value = GetUIntVariableLength();
		}
		public int SerializeVariableLengthAt(int offset, ref uint value) {
			int bytes;
			if (serializerWriteMode) bytes = PutVariableLengthAt(offset, value);
			else value = GetUIntVariableLengthAt(offset, out bytes);
			return bytes;
		}

		public void SerializeVariableLength(ref long value) {
			if (serializerWriteMode) PutVariableLength(value);
			else value = GetLongVariableLength();
		}
		public int SerializeVariableLengthAt(int offset, ref long value) {
			int bytes;
			if (serializerWriteMode) bytes = PutVariableLengthAt(offset, value);
			else value = GetLongVariableLengthAt(offset, out bytes);
			return bytes;
		}

		public void SerializeVariableLength(ref ulong value) {
			if (serializerWriteMode) PutVariableLength(value);
			else value = GetULongVariableLength();
		}
		public int SerializeVariableLengthAt(int offset, ref ulong value) {
			int bytes;
			if (serializerWriteMode) bytes = PutVariableLengthAt(offset, value);
			else value = GetULongVariableLengthAt(offset, out bytes);
			return bytes;
		}


		public void SerializeRanged(ref float value, float min, float max, int numberOfBits) {
			if (serializerWriteMode) PutRanged(value, min, max, numberOfBits);
			else value = GetRangedFloat(min, max, numberOfBits);
		}
		public void SerializeRangedAt(int offset, ref float value, float min, float max, int numberOfBits) {
			if (serializerWriteMode) PutRangedAt(offset, value, min, max, numberOfBits);
			else value = GetRangedFloatAt(offset, min, max, numberOfBits);
		}

		public void SerializeRanged(ref int value, int min, int max) {
			if (serializerWriteMode) PutRanged(value, min, max);
			else value = GetRangedInt(min, max);
		}
		public void SerializeRangedAt(int offset, ref int value, int min, int max) {
			if (serializerWriteMode) PutRangedAt(offset, value, min, max);
			else value = GetRangedIntAt(offset, min, max);
		}

		public void SerializeRanged(ref long value, long min, long max) {
			if (serializerWriteMode) PutRanged(value, min, max);
			else value = GetRangedLong(min, max);
		}
		public void SerializeRangedAt(int offset, ref long value, long min, long max) {
			if (serializerWriteMode) PutRangedAt(offset, value, min, max);
			else value = GetRangedLongAt(offset, min, max);
		}


		public void Serialize(byte[] v) {
			Serialize(v, 0, v.Length * 8);
		}
		public void Serialize(byte[] src, int srcOffset, int length) {
			BitBuffer bb = new BitBuffer(src, srcOffset, length);
			Serialize(ref bb);
		}
		public void SerializeAt(int offset, ref BitBuffer src) {
			if (serializerWriteMode) PutAt(offset, src);
			else src = GetBitsAt(offset, src.Length);
		}
		public void Serialize(ref BitBuffer bb) {
			if (serializerWriteMode) Put(bb);
			else bb = GetBits(bb.Length);
		}
		public void Serialize(ref string str) {
			if (serializerWriteMode) Put(str);
			else str = GetString();
		}
		#endregion
	}
}
