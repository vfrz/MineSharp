using System.Text.Json;
using MineSharp.Core.Packets;

namespace MineSharp.Extensions;

public static class PacketExtensions
{
    public static string ToDebugString(this IPacket packet) => JsonSerializer.Serialize(packet, packet.GetType());
}