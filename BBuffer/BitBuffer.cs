﻿using System;

namespace BBuffer {
	/// <summary>
	/// Struct that wraps an <cref=data>array</cref> and writes/reads to it in Little Endian.
	/// It works on the hole array or on a subset of it.
	/// Because it is an struct, you don't need to pool it but you may need to pass it to other methods using the ref keyword
	/// </summary>
	public struct BitBuffer {
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



		public BitBuffer(byte[] buffer) : this(buffer, 0, null != buffer ? buffer.Length * 8 : 0) { }
		public BitBuffer(byte[] buffer, int offset, int length) : this() {
			data = buffer;
			absPosition = offset;
			absLength = offset + length;
			absOffset = offset;
		}

		public void SkipBytes(int v) {
			absPosition += 8 * v;
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

		public void SkipBits(int numberOfBits) {
			absPosition += numberOfBits;
		}

		public void Rewind() {
			absPosition = 0;
		}

		/// <summary>
		/// Needs testing
		/// </summary>
		public byte[] ToArray() {
			byte[] copy = new byte[(int) Math.Ceiling(Length / 8f)];
			int shift = 0x7 & absOffset;
			if (0 == shift) {
				Buffer.BlockCopy(data, absOffset / 8, copy, 0, copy.Length);
			}
			else {
				for (int i = 0; i < copy.Length; i++) {
					copy[i] = (byte) (
						data[absOffset / 8 + i] >> shift |
						data[absOffset / 8 + i + 1] << (8 - shift)
					);
				}
			}
			if (0 != (0x7 & Length)) {
				copy[Length / 8] &= (byte) (1 << (0x7 & Length) - 1);
			}
			return copy;
		}

		public bool BufferEquals(BitBuffer other) {
			if (Length != other.Length) return false;
			for (int i = 0; i < Length; i += 8)
				if (GetByteAt(i) != other.GetByteAt(i))
					return false;
			return true;
		}

		public bool HasData() {
			return null != data;
		}

		/// <summary>
		/// Advance the position of the buffer to byte align it, unless it is already aligned.
		/// Read/write operations are faster when the buffer is byte aligned
		/// </summary>
		public void ByteAlignPosition() {
			int delta = 0x7 & (sizeof(byte) - 1 - (0x7 & absPosition));
			absPosition += delta;
		}
		public bool IsPositionByteAligned() {
			return 0 == (0x7 & absPosition);
		}

		#region PutMethods
		void UpdateDataSize(int position) {
			if (position > Length) Length = position;
		}

		public void Put(byte value, int bitCount = sizeof(byte) * 8) {
			FastBit.Byte.Write(value, data, absPosition, bitCount);
			absPosition += bitCount;
			UpdateDataSize(absPosition);
		}
		public void PutAt(int offset, byte value, int bitCount = sizeof(byte) * 8) {
			FastBit.Byte.Write(value, data, absOffset + offset, bitCount);
			UpdateDataSize(offset + absOffset + bitCount);
		}

		public void Put(short value, int bitCount = sizeof(short) * 8) {
			new FastBit.UShort((ushort) value).Write(data, absPosition, bitCount);
			absPosition += bitCount;
			UpdateDataSize(absPosition);
		}
		public void PutAt(int offset, short value, int bitCount = sizeof(short) * 8) {
			new FastBit.UShort((ushort) value).Write(data, absOffset + offset, bitCount);
			UpdateDataSize(offset + absOffset + bitCount);
		}

		public void Put(ushort value, int bitCount = sizeof(ushort) * 8) {
			new FastBit.UShort(value).Write(data, absPosition, bitCount);
			absPosition += bitCount;
			UpdateDataSize(absPosition);
		}
		public void PutAt(int offset, ushort value, int bitCount = sizeof(ushort) * 8) {
			new FastBit.UShort(value).Write(data, absOffset + offset, bitCount);
			UpdateDataSize(offset + absOffset + bitCount);
		}

		public void Put(int value, int bitCount = sizeof(int) * 8) {
			new FastBit.UInt((uint) value).Write(data, absPosition, bitCount);
			absPosition += bitCount;
			UpdateDataSize(absPosition);
		}
		public void PutAt(int offset, int value, int bitCount = sizeof(int) * 8) {
			new FastBit.UInt((uint) value).Write(data, absOffset + offset, bitCount);
			UpdateDataSize(offset + absOffset + bitCount);
		}

		public void Put(uint value, int bitCount = sizeof(uint) * 8) {
			new FastBit.UInt(value).Write(data, absPosition, bitCount);
			absPosition += bitCount;
			UpdateDataSize(absPosition);
		}
		public void PutAt(int offset, uint value, int bitCount = sizeof(uint) * 8) {
			new FastBit.UInt(value).Write(data, absOffset + offset, bitCount);
			UpdateDataSize(offset + absOffset + bitCount);
		}

		public void Put(long value, int bitCount = sizeof(long) * 8) {
			new FastBit.ULong((ulong) value).Write(data, absPosition, bitCount);
			absPosition += bitCount;
			UpdateDataSize(absPosition);
		}
		public void PutAt(int offset, long value, int bitCount = sizeof(long) * 8) {
			new FastBit.ULong((ulong) value).Write(data, absOffset + offset, bitCount);
			UpdateDataSize(offset + absOffset + bitCount);
		}

		public void Put(ulong value, int bitCount = sizeof(ulong) * 8) {
			new FastBit.ULong(value).Write(data, absPosition, bitCount);
			absPosition += bitCount;
			UpdateDataSize(absPosition);
		}
		public void PutAt(int offset, ulong value, int bitCount = sizeof(ulong) * 8) {
			new FastBit.ULong(value).Write(data, absOffset + offset, bitCount);
			UpdateDataSize(offset + absOffset + bitCount);
		}

		public void Put(float value) {
			PutAt(Position, value);
			absPosition += sizeof(float) * 8;
		}
		public void PutAt(int offset, float value) {
			const int bitCount = sizeof(float) * 8;
			if (!BitConverter.IsLittleEndian) value = new FastByte.Float(value).GetReversed();
			var f = new FastByte.Float(value);
			new FastBit.UInt(f.b0, f.b1, f.b2, f.b3).Write(data, absOffset + offset, bitCount);
			UpdateDataSize(offset + absOffset + bitCount);
		}

		public void Put(double value) {
			PutAt(Position, value);
			absPosition += sizeof(double) * 8;
		}
		public void PutAt(int offset, double value) {
			const int bitCount = sizeof(double) * 8;
			if (!BitConverter.IsLittleEndian) value = new FastByte.Double(value).GetReversed();
			var f = new FastByte.Double(value);
			new FastBit.ULong(f.b0, f.b1, f.b2, f.b3, f.b4, f.b5, f.b6, f.b7).Write(data, absOffset + offset, bitCount);
			UpdateDataSize(offset + absOffset + bitCount);
		}


		public void PutVariableLength(int value) {
			// Right shift 1, moving the MSB to the LSB, so negative numbers can be compressed too
			uint zigzag = (uint) (value << 1) ^ (uint) (value >> (sizeof(int) * 8 - 1));
			PutVariableLength(zigzag);
		}
		public int PutVariableLengthAt(int offset, int value) {
			// Right shift 1, moving the MSB to the LSB, so negative numbers can be compressed too
			uint zigzag = (uint) (value << 1) ^ (uint) (value >> (sizeof(int) * 8 - 1));
			return PutVariableLengthAt(offset, zigzag);
		}

		public void PutVariableLength(uint value) {
			absPosition += PutVariableLengthAt(Position, value) * 8;
			UpdateDataSize(absPosition);
		}
		public int PutVariableLengthAt(int offset, uint value) {
			return PutVariableLengthAt(offset, (ulong) value);
		}

		public void PutVariableLength(long value) {
			// Right shift 1, moving the MSB to the LSB, so negative numbers can be compressed too
			ulong zigzag = (ulong) (value << 1) ^ (ulong) (value >> (sizeof(long) * 8 - 1));
			PutVariableLength(zigzag);
		}
		public int PutVariableLengthAt(int offset, long value) {
			// Right shift 1, moving the MSB to the LSB, so negative numbers can be compressed too
			ulong zigzag = (ulong) (value << 1) ^ (ulong) (value >> (sizeof(long) * 8 - 1));
			return PutVariableLengthAt(offset, zigzag);
		}

		public void PutVariableLength(ulong value) {
			absPosition += PutVariableLengthAt(Position, value) * 8;
			UpdateDataSize(absPosition);
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


		public void PutDeltaCompress(int value, int previousValue) {
			PutVariableLength(value - previousValue);
		}
		public void PutDeltaCompressAt(int offset, int value, int previousValue) {
			PutVariableLengthAt(offset, value - previousValue);
		}

		public void PutDeltaCompress(long value, long previousValue) {
			PutVariableLength(value - previousValue);
		}
		public void PutDeltaCompressAt(int offset, long value, long previousValue) {
			PutVariableLengthAt(offset, value - previousValue);
		}


		public void Put(byte[] src, int srcOffset, int lengthBytes) {
			if (0 == (0x7 & absPosition)) {
				Buffer.BlockCopy(src, srcOffset, data, absPosition / 8, lengthBytes);
				absPosition += lengthBytes;
				UpdateDataSize(absPosition);
			}
			else {
				for (int i = 0; i < lengthBytes; i++) {
					Put(src[srcOffset + i]);
				}
			}
		}
		public void PutAt(BitBuffer src, int srcBitOffset, int dstBitOffset, int bitLength) {
			bool srcArrIsByteAligned = 0 == (0x7 & (src.absOffset + srcBitOffset));
			bool dstArrIsByteAligned = 0 == (0x7 & (absOffset + dstBitOffset));
			if (srcArrIsByteAligned && dstArrIsByteAligned) {
				Buffer.BlockCopy(src.data, (src.absOffset + srcBitOffset) / 8, data, (absOffset + dstBitOffset) / 8, bitLength / 8);
				absPosition += (bitLength / 8) * 8;
			}
			else {
				for (int i = 0; i < bitLength / 8; i++) {
					Put(src.GetByte(srcBitOffset));
				}
			}
			if (0 != (0x7 & bitLength)) {
				// last byte is not aligned and has to be copied manually
				Put(src.data, src.data[(src.absOffset + srcBitOffset + bitLength) / 8], 0x7 & bitLength);
			}
			absPosition += bitLength;
			UpdateDataSize(absPosition);
		}
		public void Put(byte[] data) {
			Put(data, 0, data.Length);
		}
		public void Put(BitBuffer data) {
			PutAt(data, 0, absPosition, data.Length);
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

		public void GetBytes(int srcOffset, byte[] destination) {
			GetBytes(srcOffset, destination, destination.Length);
		}
		public void GetBytes(int srcOffset, byte[] destination, int lenght) {
			GetBytes(srcOffset, destination, 0, lenght);
		}
		public void GetBytes(int srcOffset, byte[] destination, int dstOffset, int lenght) {
			Buffer.BlockCopy(data, absOffset + srcOffset, destination, dstOffset, lenght);
		}
		public void GetBytes(byte[] destination) {
			GetBytes(destination, destination.Length);
		}
		public void GetBytes(byte[] destination, int lenght) {
			GetBytes(destination, 0, lenght);
		}
		public void GetBytes(byte[] destination, int offset, int lenght) {
			Buffer.BlockCopy(data, absPosition, destination, offset, lenght);
			absPosition += lenght;
		}


		public int GetIntVariableLength() {
			uint zigzag = GetUIntVariableLength();
			return (int) ((zigzag >> 1) ^ -(zigzag & 1));
		}
		public int GetIntVariableLengthAt(int offset, out int bytes) {
			uint zigzag = GetUIntVariableLengthAt(offset, out bytes);
			return (int) ((zigzag >> 1) ^ -(zigzag & 1));
		}

		public uint GetUIntVariableLength() {
			int bytes;
			uint value = GetUIntVariableLengthAt(Position, out bytes);
			absPosition += bytes * 8;
			UpdateDataSize(absPosition);
			return value;
		}
		public uint GetUIntVariableLengthAt(int offset, out int bytes) {
			return (uint) GetULongVariableLengthAt(offset, out bytes);
		}

		public long GetLongVariableLength() {
			ulong zigzag = GetULongVariableLength();
			return (long) ((zigzag >> 1) ^ (zigzag << (sizeof(long) * 8 - 1)));
		}
		public long GetLongVariableLengthAt(int offset, out int bytes) {
			ulong zigzag = GetULongVariableLengthAt(offset, out bytes);
			return (long) ((zigzag >> 1) ^ (zigzag << (sizeof(long) * 8 - 1)));
		}

		public ulong GetULongVariableLength() {
			int bytes;
			ulong value = GetULongVariableLengthAt(Position, out bytes);
			absPosition += bytes * 8;
			UpdateDataSize(absPosition);
			return value;
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


		public int GetIntDeltaCompress(int previousValue) {
			return GetIntVariableLength() + previousValue;
		}
		public int GetIntDeltaCompressAt(int offset, int previousValue) {
			int bytes;
			return GetIntVariableLengthAt(offset, out bytes) + previousValue;
		}

		public long GetLongDeltaCompress(long previousValue) {
			return GetLongVariableLength() + previousValue;
		}
		public long GetLongDeltaCompressAt(int offset, long previousValue) {
			int bytes;
			return GetLongVariableLengthAt(offset, out bytes) + previousValue;
		}

		#endregion
	}
}