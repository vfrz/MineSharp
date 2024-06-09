using MineSharp.Core;

namespace MineSharp.Network;

public record ClientPacketHandlerContext(MinecraftServer Server, RemoteClient RemoteClient);