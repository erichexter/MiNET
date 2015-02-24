using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace MiNETPC.Classes
{
	public class PCChunkColumn
	{
		public int X { get; set; }
		public int Z { get; set; }
		public byte[] BiomeId = ArrayOf<byte>.Create(256, 1);
		public int[] BiomeColor = ArrayOf<int>.Create(256, 1);

		public ushort[] Blocks = new ushort[16 * 16 * 256];
		public NibbleArray Skylight = new NibbleArray(16 * 16 * 256);
		public NibbleArray Blocklight = new NibbleArray(16 * 16 * 256);

		private byte[] _cache = null;

		public PCChunkColumn()
		{
			for (int i = 0; i < Skylight.Length; i++)
				Skylight[i] = 0xff;
			for (int i = 0; i < BiomeColor.Length; i++)
				BiomeColor[i] = 8761930; // Grass color?
		}

		public ushort GetBlock(int x, int y, int z)
		{
			int index = x + 16 * z + 16 * 16 * y;
			if (index >= 0 && index < Blocks.Length)
				return Blocks[index];
			else return 900;
		}

		public byte GetMetadata(int x, int y, int z)
		{
			//return metadata[(x * 2048) + (z * 128) + y];
			return 0; //We dont support METADATA for now :P
		}

		public void SetBlock(int x, int y, int z, int blockid, int metadata)
		{
			int index = x + 16 * z + 16 * 16 * y;
			if (index >= 0 && index < Blocks.Length)
				Blocks[index] = Convert.ToUInt16((blockid << 4) | metadata);
		}

		public void SetBlocklight(int x, int y, int z, byte data)
		{
			_cache = null;
			Blocklight[(x * 2048) + (z * 256) + y] = data;
		}

		public void SetSkylight(int x, int y, int z, byte data)
		{
			_cache = null;
			Skylight[(x * 2048) + (z * 256) + y] = data;
		}

		public byte[] GetBytes()
		{

			//using (var stream = new MemoryStream())
			//{
			//	using (var writer = new NbtBinaryWriter(stream, true))
			//	{
					MSGBuffer writer = new MSGBuffer(new byte[]{});
					writer.WriteInt(X);
					writer.WriteInt(Z);
					writer.WriteBool(true);
					writer.WriteUShort((ushort)0xffff); // bitmap
					writer.WriteVarInt((Blocks.Length * 2) + Skylight.Data.Length + Blocklight.Data.Length + BiomeId.Length);

					foreach (ushort i in Blocks)
						writer.WriteUShort(i);

					writer.Write(Blocklight.Data);
					writer.Write(Skylight.Data);

					writer.Write(BiomeId); //OK
			//	}
			return writer.ExportWriter;
			//}
		}

		public byte[] Export()
		{
			MSGBuffer buffer = new MSGBuffer(new byte[0]);

			buffer.WriteInt(Blocks.Length);
			foreach (ushort i in Blocks)
				buffer.WriteUShort(i);

			buffer.WriteInt(Blocklight.Data.Length);
			buffer.Write(Blocklight.Data);

			buffer.WriteInt(Skylight.Data.Length);
			buffer.Write(Skylight.Data);

			buffer.WriteInt(BiomeId.Length);
			buffer.Write(BiomeId);

			return buffer.ExportWriter;

		}

		public void Pe2Pc(ChunkColumn sourceChunk)
		{
			for (int y = 0; y < 128; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					for (int z = 0; z < 16; z++)
					{
						SetBlock(x, y, z, sourceChunk.GetBlock(x, y, z), sourceChunk.GetMetadata(x, y, z));
					}
				}
			}
		}
	}

	public static class ArrayOf<T> where T : new()
	{
		public static T[] Create(int size, T initialValue)
		{
			var array = new T[size];
			for (int i = 0; i < array.Length; i++)
				array[i] = initialValue;
			return array;
		}

		public static T[] Create(int size)
		{
			var array = new T[size];
			for (int i = 0; i < array.Length; i++)
				array[i] = new T();
			return array;
		}
	}
}
