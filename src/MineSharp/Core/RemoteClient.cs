using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using MineSharp.Network;
using MineSharp.Network.Packets;
using MineSharp.Saves;
using MineSharp.TileEntities;
using MineSharp.World;

namespace MineSharp.Core;

public class RemoteClient : IDisposable
{
    public enum ClientState
    {
        Initial,
        Ready
    }

    public Player? Player { get; private set; }
    public string NetworkId { get; }

    public ClientState State { get; private set; }

    private Socket Socket { get; }
    private MinecraftServer Server { get; }

    public RemoteClient(Socket socket, MinecraftServer server)
    {
        Socket = socket;
        Server = server;
        State = ClientState.Initial;
        NetworkId = socket.RemoteEndPoint?.ToString() ?? throw new Exception();
    }

    public async Task InitializePlayerAsync(string username)
    {
        if (Player is not null)
            throw new InvalidOperationException($"Can't initialize player {username} because it has already been initialized");

        if (SaveManager.IsPlayerSaved(username))
        {
            Player = Player.LoadPlayer(Server, this, username);
        }
        else
        {
            Player = await Player.NewPlayerAsync(Server, this, username);
        }

        Server.EntityManager.RegisterEntity(Player);
    }

    public void SetReady()
    {
        State = ClientState.Ready;
    }

    public async Task SendPacketAsync(IServerPacket packet)
    {
        await using var writer = new PacketWriter(packet.PacketId);
        packet.Write(writer);
        var data = writer.ToByteArray();
        await TrySendAsync(data);
    }

    public async Task<bool> TrySendAsync(byte[] data)
    {
        if (!Socket.Connected)
            return false;
        try
        {
            await Socket.SendAsync(data);
            return true;
        }
        catch (Exception ex)
        {
            Server.GetLogger<RemoteClient>().LogError(ex, "Failed to send data to client: {networkId}", NetworkId);
            return false;
        }
    }

    public async Task SendChatAsync(string message)
    {
        await SendPacketAsync(new ChatMessagePacket
        {
            Message = message
        });
    }

    public async Task KickAsync(string reason)
    {
        await SendPacketAsync(new PlayerDisconnectPacket
        {
            Reason = reason
        });
        await DisconnectSocketAsync();
    }

    public async Task DisconnectSocketAsync()
    {
        await Socket.DisconnectAsync(false);
    }

    public void Dispose()
    {
        Socket.Dispose();
    }
}