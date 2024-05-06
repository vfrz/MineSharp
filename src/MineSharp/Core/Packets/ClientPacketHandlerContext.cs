using MineSharp.Network;

namespace MineSharp.Core.Packets;

public record ClientPacketHandlerContext(MinecraftServer Server, MinecraftRemoteClient RemoteClient);