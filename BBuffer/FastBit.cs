using System;
using System.Runtime.InteropServices;

namespace BBuffer {
	/// <summary>
	/// structs for converting simple types to bits and back
	/// Includes methods to write and read from a buffer, in little endian, specifying an offset in bits and an ammount of bits
	/// </summary>
	public static class FastBit {
		private static bool IsBigEndian = !BitConverter.IsLittleEndian;

		[StructLayout(LayoutKind.Explicit)]
		public struct ULong {
			[FieldOffset(0)] public ulong value;
			[FieldOffset(0)] public byte b0;
			[FieldOffset(1)] public byte b1;
			[FieldOffset(2)] public byte b2;
			[FieldOffset(3)] public byte b3;
			[FieldOffset(4)] public byte b4;
			[FieldOffset(5)] public byte b5;
			[FieldOffset(6)] public byte b6;
			[FieldOffset(7)] public byte b7;

			public ULong(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7) : this() {
				this.b0 = b0;
				this.b1 = b1;
				this.b2 = b2;
				this.b3 = b3;
				this.b4 = b4;
				this.b5 = b5;
				this.b6 = b6;
				this.b7 = b7;
			}
			public ULong(ulong value) : this() {
				this.value = value;
			}
			public void Write(byte[] buffer, int bitOffset, int bitCount) {
				if (IsBigEndian) {
					Write(buffer, bitOffset, bitCount, b7, b6, b5, b4, b3, b2, b1, b0);
				}
				else {
					Write(buffer, bitOffset, bitCount, b0, b1, b2, b3, b4, b5, b6, b7);
				}
			}
			private void Write(byte[] buffer, int offset, int count, byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7) {
				int bitOffsetInByte = 0x7 & offset;
				if (0 == bitOffsetInByte) {
					if (count == 64) {
						buffer[offset / 8] = b0;
						buffer[offset / 8 + 1] = b1;
						buffer[offset / 8 + 2] = b2;
						buffer[offset / 8 + 3] = b3;
						buffer[offset / 8 + 4] = b4;
						buffer[offset / 8 + 5] = b5;
						buffer[offset / 8 + 6] = b6;
						buffer[offset / 8 + 7] = b7;
					}
					else if (count >= 8) {
						buffer[offset / 8] = b0;
						if (count >= 16) {
							buffer[offset / 8 + 1] = b1;
							if (count >= 24) {
								buffer[offset / 8 + 2] = b2;
								if (count >= 32) {
									buffer[offset / 8 + 3] = b3;
									if (count >= 40) {
										buffer[offset / 8 + 4] = b4;
										if (count >= 48) {
											buffer[offset / 8 + 5] = b5;
											if (count >= 56) {
												buffer[offset / 8 + 6] = b6;
												if (count >= 64) {
													buffer[offset / 8 + 7] = b7;
												}
												else Byte.Write(b7, buffer, offset + 56, count - 56);
											}
											else Byte.Write(b6, buffer, offset + 48, count - 48);
										}
										else Byte.Write(b5, buffer, offset + 40, count - 40);
									}
									else Byte.Write(b4, buffer, offset + 32, count - 32);
								}
								else Byte.Write(b3, buffer, offset + 24, count - 24);
							}
							else Byte.Write(b2, buffer, offset + 16, count - 16);
						}
						else Byte.Write(b1, buffer, offset + 8, count - 8);
					}
					else Byte.Write(b0, buffer, offset, count);
				}
				else {
					if (count > 8) {
						var bitsToAlign = 8 - bitOffsetInByte;
						Byte.Write(b0, buffer, offset, bitsToAlign);
						new ULong(value >> bitsToAlign).Write(buffer, offset + bitsToAlign, count - bitsToAlign);
					}
					else {
						Byte.Write(b0, buffer, offset, count);
					}
				}
			}

			public ulong Read(byte[] buffer, int bitOffset, int bitCount) {
				if (IsBigEndian) {
					Read(buffer, bitOffset, bitCount, ref b7, ref b6, ref b5, ref b4, ref b3, ref b2, ref b1, ref b0);
				}
				else {
					Read(buffer, bitOffset, bitCount, ref b0, ref b1, ref b2, ref b3, ref b4, ref b5, ref b6, ref b7);
				}
				return value;
			}
			private static void Read(byte[] buffer, int offset, int count, ref byte b0, ref byte b1, ref byte b2, ref byte b3, ref byte b4, ref byte b5, ref byte b6, ref byte b7) {
				if (0 == (0x7 & offset)) {
					if (count == 64) {
						b0 = buffer[offset / 8];
						b1 = buffer[offset / 8 + 1];
						b2 = buffer[offset / 8 + 2];
						b3 = buffer[offset / 8 + 3];
						b4 = buffer[offset / 8 + 4];
						b5 = buffer[offset / 8 + 5];
						b6 = buffer[offset / 8 + 6];
						b7 = buffer[offset / 8 + 7];
					}
					if (count >= 8) {
						b0 = buffer[offset / 8];
						if (count >= 16) {
							b1 = buffer[offset / 8 + 1];
							if (count >= 24) {
								b2 = buffer[offset / 8 + 2];
								if (count >= 32) {
									b3 = buffer[offset / 8 + 3];
									if (count >= 40) {
										b4 = buffer[offset / 8 + 4];
										if (count >= 48) {
											b5 = buffer[offset / 8 + 5];
											if (count >= 56) {
												b6 = buffer[offset / 8 + 6];
												if (count >= 64) {
													b7 = buffer[offset / 8 + 7];
												}
												else {
													b7 = Byte.Read(buffer, offset + 56, count - 56);
												}
											}
											else {
												b6 = Byte.Read(buffer, offset + 48, count - 48);
												b7 = 0;
											}
										}
										else {
											b5 = Byte.Read(buffer, offset + 40, count - 40);
											b6 = b7 = 0;
										}
									}
									else {
										b4 = Byte.Read(buffer, offset + 32, count - 32);
										b5 = b6 = b7 = 0;
									}
								}
								else {
									b3 = Byte.Read(buffer, offset + 24, count - 24);
									b4 = b5 = b6 = b7 = 0;
								}
							}
							else {
								b2 = Byte.Read(buffer, offset + 16, count - 16);
								b3 = b4 = b5 = b6 = b7 = 0;
							}
						}
						else {
							b1 = Byte.Read(buffer, offset + 8, count - 8);
							b2 = b3 = b4 = b5 = b6 = b7 = 0;
						}
					}
					else {
						b0 = Byte.Read(buffer, offset, count);
						b1 = b2 = b3 = b4 = b5 = b6 = b7 = 0;
					}
				}
				else {
					b0 = Byte.Read(buffer, offset, count);
					if (count > 8) {
						b1 = Byte.Read(buffer, offset + 8, count - 8);
						if (count > 16) {
							b2 = Byte.Read(buffer, offset + 16, count - 16);
							if (count > 24) {
								b3 = Byte.Read(buffer, offset + 24, count - 24);
								if (count > 32) {
									b4 = Byte.Read(buffer, offset + 32, count - 32);
									if (count > 40) {
										b5 = Byte.Read(buffer, offset + 40, count - 40);
										if (count > 48) {
											b6 = Byte.Read(buffer, offset + 48, count - 48);
											if (count > 56) {
												b7 = Byte.Read(buffer, offset + 56, count - 56);
											}
											else b7 = 0;
										}
										else b6 = b7 = 0;
									}
									else b5 = b6 = b7 = 0;
								}
								else b4 = b5 = b6 = b7 = 0;
							}
							else b3 = b4 = b5 = b6 = b7 = 0;
						}
						else b2 = b3 = b4 = b5 = b6 = b7 = 0;
					}
					else b1 = b2 = b3 = b4 = b5 = b6 = b7 = 0;
				}
			}
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct UInt {
			[FieldOffset(0)] public uint value;
			[FieldOffset(0)] public byte b0;
			[FieldOffset(1)] public byte b1;
			[FieldOffset(2)] public byte b2;
			[FieldOffset(3)] public byte b3;

			public UInt(byte b0, byte b1, byte b2, byte b3) : this() {
				this.b0 = b0;
				this.b1 = b1;
				this.b2 = b2;
				this.b3 = b3;
			}
			public UInt(uint value) : this() {
				this.value = value;
			}

			public void Write(byte[] buffer, int bitOffset, int bitCount) {
				if (IsBigEndian) {
					Write(buffer, bitOffset, bitCount, b3, b2, b1, b0);
				}
				else {
					Write(buffer, bitOffset, bitCount, b0, b1, b2, b3);
				}
			}
			private void Write(byte[] buffer, int offset, int count, byte b0, byte b1, byte b2, byte b3) {
				int bitOffsetInByte = 0x7 & offset;
				if (0 == bitOffsetInByte) {
					if (count == 32) {
						buffer[offset / 8] = b0;
						buffer[offset / 8 + 1] = b1;
						buffer[offset / 8 + 2] = b2;
						buffer[offset / 8 + 3] = b3;
					}
					else if (count >= 8) {
						buffer[offset / 8] = b0;
						if (count >= 16) {
							buffer[offset / 8 + 1] = b1;
							if (count >= 24) {
								buffer[offset / 8 + 2] = b2;
								if (count >= 32) {
									buffer[offset / 8 + 3] = b3;
								}
								else Byte.Write(b3, buffer, offset + 24, count - 24);
							}
							else Byte.Write(b2, buffer, offset + 16, count - 16);
						}
						else Byte.Write(b1, buffer, offset + 8, count - 8);
					}
					else Byte.Write(b0, buffer, offset, count);
				}
				else {
					if (count > 8) {
						var bitsToAlign = 8 - bitOffsetInByte;
						Byte.Write(b0, buffer, offset, bitsToAlign);
						new UInt(value >> bitsToAlign).Write(buffer, offset + bitsToAlign, count - bitsToAlign);
					}
					else {
						Byte.Write(b0, buffer, offset, count);
					}
				}
			}

			public uint Read(byte[] buffer, int bitOffset, int bitCount) {
				if (IsBigEndian) {
					Read(buffer, bitOffset, bitCount, ref b3, ref b2, ref b1, ref b0);
				}
				else {
					Read(buffer, bitOffset, bitCount, ref b0, ref b1, ref b2, ref b3);
				}
				return value;
			}
			private static void Read(byte[] buffer, int offset, int count, ref byte b0, ref byte b1, ref byte b2, ref byte b3) {
				if (0 == (0x7 & offset)) {
					if (count == 32) {
						// Special case, more performance
						b0 = buffer[offset / 8];
						b1 = buffer[offset / 8 + 1];
						b2 = buffer[offset / 8 + 2];
						b3 = buffer[offset / 8 + 3];
					}
					else if (count >= 8) {
						b0 = buffer[offset / 8];
						if (count >= 16) {
							b1 = buffer[offset / 8 + 1];
							if (count >= 24) {
								b2 = buffer[offset / 8 + 2];
								if (count >= 32) {
									b3 = buffer[offset / 8 + 3];
								}
								else {
									b3 = Byte.Read(buffer, offset + 24, count - 24);
								}
							}
							else {
								b2 = Byte.Read(buffer, offset + 16, count - 16);
								b3 = 0;
							}
						}
						else {
							b1 = Byte.Read(buffer, offset + 8, count - 8);
							b2 = b3 = 0;
						}
					}
					else {
						b0 = Byte.Read(buffer, offset, count);
						b1 = b2 = b3 = 0;
					}
				}
				else {
					b0 = Byte.Read(buffer, offset, count);
					if (count > 8) {
						b1 = Byte.Read(buffer, offset + 8, count - 8);
						if (count > 16) {
							b2 = Byte.Read(buffer, offset + 16, count - 16);
							if (count > 24) {
								b3 = Byte.Read(buffer, offset + 24, count - 24);
							}
							else b3 = 0;
						}
						else b2 = b3 = 0;
					}
					else b1 = b2 = b3 = 0;
				}
			}
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct UShort {
			[FieldOffset(0)] public ushort value;
			[FieldOffset(0)] public byte b0;
			[FieldOffset(1)] public byte b1;

			public UShort(byte b0, byte b1) : this() {
				this.b0 = b0;
				this.b1 = b1;
			}
			public UShort(ushort value) : this() {
				this.value = value;
			}

			public void Write(byte[] buffer, int bitOffset, int bitCount) {
				if (IsBigEndian) {
					Write(buffer, bitOffset, bitCount, b1, b0);
				}
				else {
					Write(buffer, bitOffset, bitCount, b0, b1);
				}
			}
			private void Write(byte[] buffer, int bitOffset, int bitCount, byte byte0, byte byte1) {
				if (0 == (0x7 & bitOffset)) {
					if (bitCount >= 8) buffer[bitOffset / 8] = byte0;
					else Byte.Write(byte0, buffer, bitOffset, bitCount);
					if (bitCount >= 16) buffer[bitOffset / 8 + 1] = byte1;
					else if (bitCount > 8) Byte.Write(byte1, buffer, bitOffset + 8, bitCount - 8);
				}
				else {
					Byte.Write(byte0, buffer, bitOffset, bitCount);
					if (bitCount > 8) Byte.Write(byte1, buffer, bitOffset + 8, bitCount - 8);
				}
			}

			public ushort Read(byte[] buffer, int bitOffset, int bitCount) {
				if (IsBigEndian) {
					Read(buffer, bitOffset, bitCount, ref b1, ref b0);
				}
				else {
					Read(buffer, bitOffset, bitCount, ref b0, ref b1);
				}
				return value;
			}
			private static void Read(byte[] buffer, int offset, int count, ref byte b0, ref byte b1) {
				if (0 == (0x7 & offset)) {
					if (count >= 8) {
						b0 = buffer[offset / 8];
						if (count >= 16) {
							b1 = buffer[offset / 8 + 1];
						}
						else {
							b1 = Byte.Read(buffer, offset + 8, count - 8);
						}
					}
					else {
						b0 = Byte.Read(buffer, offset, count);
						b1 = 0;
					}
				}
				else {
					b0 = Byte.Read(buffer, offset, count);
					if (count > 8) {
						b1 = Byte.Read(buffer, offset + 8, count - 8);
					}
					else b1 = 0;
				}
			}
		}

		public static class Byte {
			public static void Write(byte value, byte[] buffer, int bitOffset, int bitCount) {
				if (bitCount > 8) bitCount = 8;
				else if (bitCount <= 0) return;

				int bitOffsetInByte = 0x7 & bitOffset;
				if (0 == bitOffsetInByte) {
					if (8 == bitCount) {
						buffer[bitOffset / 8] = value;
					}
					else {
						int mask = (1 << bitCount) - 1;
						buffer[bitOffset / 8] = (byte) ((~mask & buffer[bitOffset / 8]) | (mask & value));
					}
				}
				else {
					int mask = (bitCount == 8 ? 0xff : (1 << bitCount) - 1) << bitOffsetInByte;
					buffer[bitOffset / 8] = (byte) ((~mask & buffer[bitOffset / 8]) | (mask & (value << bitOffsetInByte)));

					if (bitCount > 8 - bitOffsetInByte) {
						int mask2 = mask >> 8;
						buffer[bitOffset / 8 + 1] = (byte) ((~mask2 & buffer[bitOffset / 8 + 1]) | (mask2 & (value >> (8 - bitOffsetInByte))));
					}
				}
			}
			public static byte Read(byte[] buffer, int bitOffset, int bitCount) {
				if (bitCount <= 0) return 0;

				int bitOffsetInByte = 0x7 & bitOffset;

				byte value;
				if (0 == bitOffsetInByte) {
					value = buffer[bitOffset / 8];
				}
				else {
					value = (byte) (buffer[bitOffset / 8] >> bitOffsetInByte);
					if (bitCount > 8 - bitOffsetInByte)
						value |= (byte) (buffer[bitOffset / 8 + 1] << (8 - bitOffsetInByte));
				}
				return bitCount >= 8 ? value : (byte) (value & ((1 << bitCount) - 1));
			}
		}
	}
}
