using System.IO;

namespace MiNET.Utils
{
	public class ItemStack
	{
		public short Id { get; set; }
		public byte Count { get; set; }
		public short Metadata { get; set; }

		public ItemStack()
		{
		}

		public ItemStack(short id)
			: this()
		{
			Id = id;
			Count = 1;
			Metadata = 0;
		}

		public ItemStack(short id, byte count)
			: this(id)
		{
			Count = count;
		}

		public ItemStack(short id, byte count, short metadata)
			: this(id, count)
		{
			Metadata = metadata;
		}

		public static ItemStack FromStream(BinaryReader stream)
		{
			var slot = new ItemStack();
			slot.Id = stream.ReadInt16();
			slot.Count = stream.ReadByte();
			slot.Metadata = stream.ReadInt16();
			return slot;
		}

		public void WriteTo(BinaryWriter stream)
		{
			stream.Write(Id);
			stream.Write(Count);
			stream.Write(Metadata);
		}
	}
}