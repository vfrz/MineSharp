using System.Text.Json;
using MineSharp.Network;

namespace MineSharp.Extensions;

public static class PacketExtensions
{
    public static string ToDebugString(this IPacket packet) => JsonSerializer.Serialize(packet, packet.GetType());
}