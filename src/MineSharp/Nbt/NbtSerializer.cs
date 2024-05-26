using System.IO.Compression;
using System.Text;
using MineSharp.Extensions;
using MineSharp.Nbt.Tags;

namespace MineSharp.Nbt;

public class NbtSerializer
{
    public static INbtTag Deserialize(Stream stream, NbtCompression compression = NbtCompression.None)
    {
        if (compression is NbtCompression.None)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            return ParseTag(reader);
        }

        if (compression is NbtCompression.Gzip)
        {
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress, true);
            using var reader = new BinaryReader(gzipStream, Encoding.UTF8, true);
            return ParseTag(reader);
        }

        if (compression is NbtCompression.Zlib)
        {
            using var gzipStream = new ZLibStream(stream, CompressionMode.Decompress, true);
            using var reader = new BinaryReader(gzipStream, Encoding.UTF8, true);
            return ParseTag(reader);
        }

        throw new Exception($"Unknown NBT compression type: {compression}");
    }

    public static INbtTag Deserialize(byte[] bytes, NbtCompression compression = NbtCompression.None)
    {
        using var memoryStream = new MemoryStream(bytes);
        return Deserialize(memoryStream, compression);
    }

    public static void Serialize(INbtTag tag, Stream stream, NbtCompression compression = NbtCompression.None)
    {
        if (compression is NbtCompression.None)
        {
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            WriteTag(tag, writer);
        }
        else if (compression is NbtCompression.Gzip)
        {
            using var gzipStream = new GZipStream(stream, CompressionMode.Compress, true);
            using var writer = new BinaryWriter(gzipStream, Encoding.UTF8, true);
            WriteTag(tag, writer);
        }
        else if (compression is NbtCompression.Zlib)
        {
            using var gzipStream = new ZLibStream(stream, CompressionMode.Compress, true);
            using var writer = new BinaryWriter(gzipStream, Encoding.UTF8, true);
            WriteTag(tag, writer);
        }
        else
        {
            throw new Exception($"Unknown NBT compression type: {compression}");
        }
    }

    public static byte[] Serialize(INbtTag tag, NbtCompression compression = NbtCompression.None)
    {
        using var memoryStream = new MemoryStream();
        Serialize(tag, memoryStream, compression);
        return memoryStream.ToArray();
    }

    private static INbtTag ParseTag(BinaryReader reader)
    {
        var tagType = (TagType) reader.ReadByte();
        return tagType switch
        {
            TagType.End => EndNbtTag.Instance,
            TagType.Byte => ParseByteTag(reader),
            TagType.Short => ParseShortTag(reader),
            TagType.Int => ParseIntTag(reader),
            TagType.Long => ParseLongTag(reader),
            TagType.Float => ParseFloatTag(reader),
            TagType.Double => ParseDoubleNbtTag(reader),
            TagType.ByteArray => ParseByteArrayNbtTag(reader),
            TagType.String => ParseStringTag(reader),
            TagType.List => ParseListTag(reader),
            TagType.Compound => ParseCompoundTag(reader),
            _ => throw new Exception($"Unknown NBT tag type: {tagType}")
        };
    }

    private static ByteNbtTag ParseByteTag(BinaryReader reader)
    {
        var name = reader.ReadNbtString();
        var value = reader.ReadByte();
        return new ByteNbtTag(name!, value);
    }

    private static ShortNbtTag ParseShortTag(BinaryReader reader)
    {
        var name = reader.ReadNbtString();
        var value = reader.ReadBigEndianShort();
        return new ShortNbtTag(name!, value);
    }

    private static IntNbtTag ParseIntTag(BinaryReader reader)
    {
        var name = reader.ReadNbtString();
        var value = reader.ReadBigEndianInt();
        return new IntNbtTag(name!, value);
    }

    private static LongNbtTag ParseLongTag(BinaryReader reader)
    {
        var name = reader.ReadNbtString();
        var value = reader.ReadBigEndianLong();
        return new LongNbtTag(name!, value);
    }

    private static FloatNbtTag ParseFloatTag(BinaryReader reader)
    {
        var name = reader.ReadNbtString();
        var value = reader.ReadBigEndianFloat();
        return new FloatNbtTag(name!, value);
    }

    private static DoubleNbtTag ParseDoubleNbtTag(BinaryReader reader)
    {
        var name = reader.ReadNbtString();
        var value = reader.ReadBigEndianDouble();
        return new DoubleNbtTag(name!, value);
    }

    private static ByteArrayNbtTag ParseByteArrayNbtTag(BinaryReader reader)
    {
        var name = reader.ReadNbtString();
        var length = reader.ReadBigEndianInt();
        var value = reader.ReadBytes(length);
        return new ByteArrayNbtTag(name!, value);
    }

    private static StringNbtTag ParseStringTag(BinaryReader reader)
    {
        var name = reader.ReadNbtString();
        var value = reader.ReadNbtString();
        return new StringNbtTag(name!, value);
    }

    private static ListNbtTag ParseListTag(BinaryReader reader)
    {
        var name = reader.ReadNbtString();
        var tagType = (TagType) reader.ReadByte();
        var length = reader.ReadBigEndianInt();
        var tags = new List<INbtTag>();
        for (var i = 0; i < length; i++)
        {
            switch (tagType)
            {
                case TagType.Byte:
                    tags.Add(new ByteNbtTag(null, reader.ReadByte()));
                    break;
                case TagType.Short:
                    tags.Add(new ShortNbtTag(null, reader.ReadBigEndianShort()));
                    break;
                case TagType.Int:
                    tags.Add(new IntNbtTag(null, reader.ReadBigEndianInt()));
                    break;
                case TagType.Long:
                    tags.Add(new LongNbtTag(null, reader.ReadBigEndianLong()));
                    break;
                case TagType.Float:
                    tags.Add(new FloatNbtTag(null, reader.ReadBigEndianFloat()));
                    break;
                case TagType.Double:
                    tags.Add(new DoubleNbtTag(null, reader.ReadBigEndianDouble()));
                    break;
                case TagType.String:
                    tags.Add(new StringNbtTag(null, reader.ReadNbtString()));
                    break;
                case TagType.Compound:
                    var compoundTag = new CompoundNbtTag(null);

                    INbtTag currentTag;
                    while ((currentTag = ParseTag(reader)) != EndNbtTag.Instance)
                    {
                        compoundTag[currentTag.Name!] = currentTag;
                    }

                    tags.Add(compoundTag);
                    break;
                case TagType.ByteArray:
                case TagType.List:
                case TagType.End:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return new ListNbtTag(name, tagType, tags);
    }

    private static CompoundNbtTag ParseCompoundTag(BinaryReader reader)
    {
        var name = reader.ReadNbtString();
        var compoundTag = new CompoundNbtTag(name);

        INbtTag currentTag;
        while ((currentTag = ParseTag(reader)) != EndNbtTag.Instance)
        {
            compoundTag[currentTag.Name!] = currentTag;
        }

        return compoundTag;
    }

    private static void WriteTag(INbtTag tag, BinaryWriter writer)
    {
        if (tag is ByteNbtTag byteNbtTag)
        {
            WriteByteTag(byteNbtTag, writer);
        }
        else if (tag is ShortNbtTag shortNbtTag)
        {
            WriteShortTag(shortNbtTag, writer);
        }
        else if (tag is IntNbtTag intNbtTag)
        {
            WriteIntTag(intNbtTag, writer);
        }
        else if (tag is LongNbtTag longNbtTag)
        {
            WriteLongTag(longNbtTag, writer);
        }
        else if (tag is FloatNbtTag floatNbtTag)
        {
            WriteFloatTag(floatNbtTag, writer);
        }
        else if (tag is DoubleNbtTag doubleNbtTag)
        {
            WriteDoubleTag(doubleNbtTag, writer);
        }
        else if (tag is ByteArrayNbtTag byteArrayNbtTag)
        {
            WriteByteArrayNbtTag(byteArrayNbtTag, writer);
        }
        else if (tag is StringNbtTag stringNbtTag)
        {
            WriteStringTag(stringNbtTag, writer);
        }
        else if (tag is ListNbtTag listNbtTag)
        {
            WriteListTag(listNbtTag, writer);
        }
        else if (tag is CompoundNbtTag compoundNbtTag)
        {
            WriteCompoundTag(compoundNbtTag, writer);
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    private static void WriteByteTag(ByteNbtTag tag, BinaryWriter writer)
    {
        writer.Write((byte) TagType.Byte);
        writer.WriteNbtString(tag.Name);
        writer.Write(tag.Value);
    }

    private static void WriteShortTag(ShortNbtTag tag, BinaryWriter writer)
    {
        writer.Write((byte) TagType.Short);
        writer.WriteNbtString(tag.Name);
        writer.WriteBigEndianShort(tag.Value);
    }

    private static void WriteIntTag(IntNbtTag tag, BinaryWriter writer)
    {
        writer.Write((byte) TagType.Int);
        writer.WriteNbtString(tag.Name);
        writer.WriteBigEndianInt(tag.Value);
    }

    private static void WriteLongTag(LongNbtTag tag, BinaryWriter writer)
    {
        writer.Write((byte) TagType.Long);
        writer.WriteNbtString(tag.Name);
        writer.WriteBigEndianLong(tag.Value);
    }

    private static void WriteFloatTag(FloatNbtTag tag, BinaryWriter writer)
    {
        writer.Write((byte) TagType.Float);
        writer.WriteNbtString(tag.Name);
        writer.WriteBigEndianFloat(tag.Value);
    }

    private static void WriteDoubleTag(DoubleNbtTag tag, BinaryWriter writer)
    {
        writer.Write((byte) TagType.Double);
        writer.WriteNbtString(tag.Name);
        writer.WriteBigEndianDouble(tag.Value);
    }

    private static void WriteByteArrayNbtTag(ByteArrayNbtTag tag, BinaryWriter writer)
    {
        writer.Write((byte) TagType.ByteArray);
        writer.WriteNbtString(tag.Name);
        writer.WriteBigEndianInt(tag.Value.Length);
        writer.Write(tag.Value);
    }

    private static void WriteStringTag(StringNbtTag tag, BinaryWriter writer)
    {
        writer.Write((byte) TagType.String);
        writer.WriteNbtString(tag.Name);
        writer.WriteNbtString(tag.Value);
    }

    private static void WriteListTag(ListNbtTag tag, BinaryWriter writer)
    {
        writer.Write((byte) TagType.List);
        writer.WriteNbtString(tag.Name);
        writer.Write((byte) tag.TagType);
        writer.WriteBigEndianInt(tag.Tags.Count);
        foreach (var innerTag in tag.Tags)
        {
            if (innerTag is ByteNbtTag byteNbtTag)
            {
                writer.Write(byteNbtTag.Value);
            }
            else if (innerTag is ShortNbtTag shortNbtTag)
            {
                writer.WriteBigEndianShort(shortNbtTag.Value);
            }
            else if (innerTag is IntNbtTag intNbtTag)
            {
                writer.WriteBigEndianInt(intNbtTag.Value);
            }
            else if (innerTag is LongNbtTag longNbtTag)
            {
                writer.WriteBigEndianLong(longNbtTag.Value);
            }
            else if (innerTag is FloatNbtTag floatNbtTag)
            {
                writer.WriteBigEndianFloat(floatNbtTag.Value);
            }
            else if (innerTag is DoubleNbtTag doubleNbtTag)
            {
                writer.WriteBigEndianDouble(doubleNbtTag.Value);
            }
            else if (innerTag is StringNbtTag stringNbtTag)
            {
                writer.WriteNbtString(stringNbtTag.Value);
            }
            else if (innerTag is CompoundNbtTag compoundNbtTag)
            {
                foreach (var compoundInnerTag in compoundNbtTag)
                {
                    WriteTag(compoundInnerTag, writer);
                }

                writer.Write((byte) TagType.End);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static void WriteCompoundTag(CompoundNbtTag tag, BinaryWriter writer)
    {
        writer.Write((byte) TagType.Compound);
        writer.WriteNbtString(tag.Name);

        foreach (var innerTag in tag)
        {
            WriteTag(innerTag, writer);
        }

        writer.Write((byte) TagType.End);
    }
}