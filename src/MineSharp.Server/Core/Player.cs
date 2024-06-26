using MineSharp.Content;
using MineSharp.Entities;
using MineSharp.Entities.Metadata;
using MineSharp.Network.Packets;
using MineSharp.Numerics;
using MineSharp.Saves;
using MineSharp.TileEntities;
using MineSharp.Windows;
using MineSharp.World;

namespace MineSharp.Core;

public class Player : LivingEntity, IPlayer
{
    public const double Height = 1.62;
    public const double YMinOffset = 0.000000005;

    public override short MaxHealth => 20;

    public double Stance { get; set; }

    public bool PositionDirty { get; set; }

    public bool Respawning { get; set; }

    public required string Username { get; init; }

    private Inventory Inventory { get; }

    public RemoteClient RemoteClient { get; }

    public short SelectedHotbarSlot { get; private set; }
    public ItemStack HoldItemStack => Inventory.Hotbar[SelectedHotbarSlot];

    public bool Crouched
    {
        get
        {
            var entityFlags = Metadata.GetOrDefault(0, () => new EntityFlagsMetadata(EntityFlags.Default)).FlagsValue;
            return entityFlags.HasFlag(EntityFlags.Crouched);
        }
        private set
        {
            var entityFlags = Metadata.GetOrDefault(0, () => new EntityFlagsMetadata(EntityFlags.Default)).FlagsValue;
            Metadata.Set(0, value
                ? new EntityFlagsMetadata(entityFlags | EntityFlags.Crouched)
                : new EntityFlagsMetadata(entityFlags & ~EntityFlags.Crouched));
        }
    }

    private HashSet<Vector2<int>> _loadedChunks = new();

    public IReadOnlySet<Vector2<int>> LoadedChunks => _loadedChunks;

    private Player(MinecraftServer server, RemoteClient remoteClient) : base(server)
    {
        RemoteClient = remoteClient;
        Inventory = new Inventory();
        SelectedHotbarSlot = 0;
    }

    public static async Task<Player> NewPlayerAsync(MinecraftServer server, RemoteClient remoteClient, string username)
    {
        var spawnHeight = await server.World.GetHighestBlockHeightAsync(new Vector2<int>(0, 0)) + 1;
        var position = new Vector3<double>(0.5, spawnHeight + YMinOffset, 0.5);

        var player = new Player(server, remoteClient)
        {
            Username = username,
            Position = position,
            Stance = position.Y + Height,
            OnGround = true,
            Pitch = 0,
            Yaw = 0,
            Health = 20
        };

        player.Save();

        return player;
    }

    public static Player LoadPlayer(MinecraftServer server, RemoteClient remoteClient, string username)
    {
        var saveData = SaveManager.LoadPlayer(username);

        var player = new Player(server, remoteClient)
        {
            Username = username,
            Position = saveData.Position,
            Stance = saveData.Position.Y + Height + YMinOffset,
            OnGround = true,
            Pitch = saveData.Pitch,
            Yaw = saveData.Yaw,
            Health = saveData.Health
        };

        foreach (var slot in saveData.Inventory)
        {
            player.Inventory.SetSlot(slot.Slot, slot.ItemStack);
        }

        return player;
    }

    public void Save()
    {
        var saveData = GetSaveData();
        SaveManager.SavePlayer(Username, saveData);
    }

    private PlayerSaveData GetSaveData()
    {
        var inventory = Inventory.Slots
            .Select((itemStack, index) => new InventorySlotSaveData((byte) index, itemStack))
            .ToArray();

        return new PlayerSaveData
        {
            Position = Position,
            Yaw = Yaw,
            Pitch = Pitch,
            Inventory = inventory,
            Health = Health
        };
    }

    public async Task<bool> TryGiveItemAsync(ItemStack itemStack)
    {
        var emptyIndex = Inventory.GetFirstEmptyStorageSlot();
        if (!emptyIndex.HasValue)
            return false;
        Inventory.SetSlot(emptyIndex.Value, itemStack);
        await RemoteClient.SendPacketAsync(new SetSlotPacket
        {
            WindowId = WindowId.Inventory,
            Slot = emptyIndex.Value,
            ItemId = itemStack.ItemId,
            ItemCount = itemStack.Count,
            ItemMetadata = itemStack.Metadata
        });
        if (emptyIndex.Value == Inventory.HotbarSlotToInventorySlot(SelectedHotbarSlot))
            await BroadcastHoldItemAsync();
        return true;
    }

    public async Task<bool> PopSelectedItemStackAsync(byte quantity = 1)
    {
        var itemStack = Inventory.Hotbar[SelectedHotbarSlot];
        if (itemStack != ItemStack.Empty && itemStack.Count >= quantity)
        {
            var slotIndex = Inventory.HotbarSlotToInventorySlot(SelectedHotbarSlot);
            if (itemStack.Count == quantity)
            {
                Inventory.SetSlot(slotIndex, ItemStack.Empty);
                await BroadcastHoldItemAsync();
            }
            else
            {
                Inventory.SetSlot(slotIndex, itemStack with { Count = (byte) (itemStack.Count - quantity) });
            }

            return true;
        }

        return false;
    }

    public async Task SendInventoryAsync()
    {
        await RemoteClient.SendPacketAsync(new WindowItemsPacket
        {
            WindowId = WindowId.Inventory,
            Items = Inventory.Slots
        });
        await BroadcastHoldItemAsync();
    }

    public async Task TeleportAsync(Vector3<double> position)
    {
        Position = position;
        Stance = position.Y + Height + YMinOffset;
        await LoadChunkAsync(Chunk.GetChunkPositionForWorldPosition(Position));
        await SendPositionAndLookAsync();
    }

    public async Task ClearInventoryAsync()
    {
        Inventory.Clear();
        await SendInventoryAsync();
    }

    public async Task HoldItemChangedAsync(short slotId)
    {
        SelectedHotbarSlot = slotId;
        await BroadcastHoldItemAsync();
    }

    public async Task ToggleCrouchAsync(bool crouched)
    {
        if (Crouched == crouched)
            return;
        Crouched = crouched;
        await BroadcastEntityMetadataAsync();
    }

    protected override async Task BroadcastEntityMetadataAsync()
    {
        await Server.BroadcastPacketAsync(new EntityMetadataPacket
        {
            EntityId = EntityId,
            Metadata = Metadata
        }, RemoteClient);
    }

    public async Task AttackEntityAsync(ILivingEntity targetEntity)
    {
        await Server.BroadcastPacketAsync(new EntityStatusPacket
        {
            EntityId = targetEntity.EntityId,
            Status = EntityStatus.Hurt
        });

        var multiplier = targetEntity.KnockBackMultiplier;
        await Server.BroadcastPacketAsync(new EntityVelocityPacket
        {
            EntityId = targetEntity.EntityId,
            VelocityX = (short) (-MinecraftMath.SinDegree(Yaw) * 3000 * multiplier.X),
            VelocityY = (short) (targetEntity.OnGround ? 3000 * multiplier.Y : 0),
            VelocityZ = (short) (MinecraftMath.CosDegree(Yaw) * 3000 * multiplier.Z)
        });

        var damage = HoldItemStack == ItemStack.Empty ? 1 : ItemInfoProvider.Get(HoldItemStack.ItemId).DamageOnEntity;
        await targetEntity.SetHealthAsync((short) (targetEntity.Health - damage));
    }

    private async Task BroadcastHoldItemAsync()
    {
        await Server.BroadcastPacketAsync(new EntityEquipmentPacket
        {
            EntityId = EntityId,
            Slot = 0,
            ItemId = HoldItemStack.ItemId
        }, RemoteClient);
    }

    public async Task AddHealthAsync(int amount) => await SetHealthAsync((short) (Health + amount));

    public override async Task SetHealthAsync(short health)
    {
        Health = Math.Clamp(health, (short) 0, MaxHealth);
        await SendHealthAsync();

        if (Health == 0)
        {
            await Server.BroadcastPacketAsync(new EntityStatusPacket
            {
                EntityId = EntityId,
                Status = EntityStatus.Dead
            }, RemoteClient);

            //TODO This is a bit dirty
            Server.Looper.Schedule(TimeSpan.FromSeconds(1.5), async _ =>
            {
                if (Health == 0)
                {
                    await Server.BroadcastPacketAsync(new DestroyEntityPacket
                    {
                        EntityId = EntityId
                    }, RemoteClient);
                }
            });
        }
    }

    public async Task SendHealthAsync()
    {
        await RemoteClient.SendPacketAsync(new UpdateHealthPacket
        {
            Health = Health
        });
    }

    public async Task RespawnAsync(MinecraftDimension dimension)
    {
        //TODO This is a bit dirty
        if (Server.EntityManager.EntityExists(RemoteClient.Player!.EntityId))
        {
            await Server.BroadcastPacketAsync(new DestroyEntityPacket
            {
                EntityId = RemoteClient.Player!.EntityId
            }, RemoteClient);
        }

        Respawning = true;

        var spawnHeight = await Server.World.GetHighestBlockHeightAsync(new Vector2<int>(0, 0)) + Height + YMinOffset;
        Position = new Vector3<double>(0.5, spawnHeight, 0.5);
        Stance = Position.Y + Height;
        OnGround = true;
        Yaw = 0;
        Pitch = 0;

        await UnloadChunksAsync();

        foreach (var remoteClient in Server.RemoteClients)
        {
            if (remoteClient == RemoteClient || remoteClient.Player is null)
                continue;

            var player = remoteClient.Player!;

            await RemoteClient.SendPacketAsync(new DestroyEntityPacket
            {
                EntityId = player.EntityId
            });
        }

        await RemoteClient.SendPacketAsync(new RespawnPacket
        {
            Dimension = dimension
        });

        await UpdateLoadedChunksAsync();

        await SendPositionAndLookAsync();

        await RemoteClient.SendPacketAsync(new TimeUpdatePacket
        {
            Time = Server.World.Time
        });

        if (Server.World.Raining)
        {
            await RemoteClient.SendPacketAsync(new NewStatePacket
            {
                Reason = NewStatePacket.ReasonType.BeginRaining
            });
        }

        foreach (var remoteClient in Server.RemoteClients)
        {
            if (remoteClient == RemoteClient || remoteClient.Player is null)
                continue;

            var player = remoteClient.Player!;

            await RemoteClient.SendPacketAsync(new NamedEntitySpawnPacket
            {
                EntityId = player.EntityId,
                Username = Username,
                X = player.Position.X.ToAbsolutePosition(),
                Y = player.Position.Y.ToAbsolutePosition(),
                Z = player.Position.Z.ToAbsolutePosition(),
                Yaw = MinecraftMath.RotationFloatToSByte(Yaw),
                Pitch = MinecraftMath.RotationFloatToSByte(Pitch),
                CurrentItem = 0
            });
        }

        //TODO Handle spawn point correctly
        await Server.BroadcastPacketAsync(new NamedEntitySpawnPacket
        {
            EntityId = EntityId,
            X = Position.X.ToAbsolutePosition(),
            Y = Position.Y.ToAbsolutePosition(),
            Z = Position.Z.ToAbsolutePosition(),
            Yaw = MinecraftMath.RotationFloatToSByte(Yaw),
            Pitch = MinecraftMath.RotationFloatToSByte(Pitch),
            Username = Username,
            CurrentItem = 0
        }, RemoteClient);

        await SetHealthAsync(MaxHealth);

        Respawning = false;
    }

    public async Task SendPositionAndLookAsync()
    {
        await RemoteClient.SendPacketAsync(new PlayerPositionAndLookServerPacket
        {
            X = Position.X,
            Y = Position.Y,
            Z = Position.Z,
            Stance = Stance,
            OnGround = OnGround,
            Yaw = Yaw,
            Pitch = Pitch
        });
    }

    public async Task UnloadChunksAsync()
    {
        foreach (var chunkToUnload in _loadedChunks)
        {
            await RemoteClient.SendPacketAsync(new PreChunkPacket
            {
                X = chunkToUnload.X,
                Z = chunkToUnload.Z,
                Mode = PreChunkPacket.LoadingMode.Unload
            });
        }

        _loadedChunks.Clear();
    }

    public async Task LoadChunkAsync(Vector2<int> chunkToLoad)
    {
        if (_loadedChunks.Contains(chunkToLoad))
            return;

        var chunk = await Server.World.GetOrCreateChunkAsync(chunkToLoad);

        await RemoteClient.SendPacketAsync(new PreChunkPacket
        {
            X = chunkToLoad.X,
            Z = chunkToLoad.Z,
            Mode = PreChunkPacket.LoadingMode.Load
        });

        await RemoteClient.SendPacketAsync(new ChunkPacket
        {
            X = chunkToLoad.X * Chunk.ChunkWidth,
            Y = 0,
            Z = chunkToLoad.Z * Chunk.ChunkWidth,
            SizeX = Chunk.ChunkWidth - 1,
            SizeY = Chunk.ChunkHeight - 1,
            SizeZ = Chunk.ChunkWidth - 1,
            CompressedData = await chunk.ToCompressedDataAsync()
        });

        foreach (var tileEntity in chunk.TileEntities)
        {
            var worldPosition = chunk.LocalToWorld(tileEntity.LocalPosition);

            if (tileEntity is SignTileEntity signTileEntity)
            {
                await RemoteClient.SendPacketAsync(new UpdateSignPacket
                {
                    X = worldPosition.X,
                    Y = (short) worldPosition.Y,
                    Z = worldPosition.Z,
                    Text1 = signTileEntity.Text1 ?? string.Empty,
                    Text2 = signTileEntity.Text2 ?? string.Empty,
                    Text3 = signTileEntity.Text3 ?? string.Empty,
                    Text4 = signTileEntity.Text4 ?? string.Empty
                });
            }
            else
            {
                //TODO Handle other TileEntity types
            }
        }
    }

    public async Task UpdateLoadedChunksAsync()
    {
        var visibleChunks =
            Chunk.GetChunksAround(GetCurrentChunk(), Server.Configuration.VisibleChunksDistance);
        var chunksToLoad = visibleChunks.Except(_loadedChunks);
        var chunksToUnload = _loadedChunks.Except(visibleChunks);

        foreach (var chunkToLoad in chunksToLoad)
        {
            await LoadChunkAsync(chunkToLoad);
        }

        foreach (var chunkToUnload in chunksToUnload)
        {
            await RemoteClient.SendPacketAsync(new PreChunkPacket
            {
                X = chunkToUnload.X,
                Z = chunkToUnload.Z,
                Mode = PreChunkPacket.LoadingMode.Unload
            });
        }

        _loadedChunks = visibleChunks;
    }
}