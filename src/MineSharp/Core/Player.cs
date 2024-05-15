using MineSharp.Entities;
using MineSharp.Entities.Metadata;
using MineSharp.Items;
using MineSharp.Items.Infos;
using MineSharp.Network.Packets;
using MineSharp.World;

namespace MineSharp.Core;

public class Player : LivingEntity
{
    public const double Height = 1.62;

    public override short MaxHealth => 20;

    public double Stance { get; set; }

    public bool PositionDirty { get; set; }

    public bool Respawning { get; set; }

    public required string Username { get; init; }

    private Inventory Inventory { get; }

    private RemoteClient RemoteClient { get; }

    public short SelectedHotbarSlot { get; private set; }
    public ItemStack HoldItem => Inventory.Hotbar[SelectedHotbarSlot];

    public bool Crouched
    {
        get
        {
            var entityFlags = Metadata.GetOrDefault(0, new EntityFlagsMetadata(EntityFlags.Default)).FlagsValue;
            return entityFlags.HasFlag(EntityFlags.Crouched);
        }
        private set
        {
            var entityFlags = Metadata.GetOrDefault(0, new EntityFlagsMetadata(EntityFlags.Default)).FlagsValue;
            Metadata.Set(0, value
                ? new EntityFlagsMetadata(entityFlags | EntityFlags.Crouched)
                : new EntityFlagsMetadata(entityFlags & ~EntityFlags.Crouched));
        }
    }

    public Player(RemoteClient remoteClient)
    {
        RemoteClient = remoteClient;
        Inventory = new Inventory();
        SelectedHotbarSlot = 0;
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
                Inventory.SetSlot(slotIndex, itemStack with { Count = (byte)(itemStack.Count - quantity) });
            }
            
            return true;
        }

        return false;
    }

    public async Task ClearInventoryAsync()
    {
        var heldItem = Inventory.Hotbar[SelectedHotbarSlot];
        Inventory.Clear();
        await RemoteClient.SendPacketAsync(new WindowItemsPacket
        {
            WindowId = WindowId.Inventory,
            Items = Inventory.Slots
        });
        if (heldItem != ItemStack.Empty)
            await BroadcastHoldItemAsync();
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
        await Server!.BroadcastPacketAsync(new EntityMetadataPacket
        {
            EntityId = EntityId,
            Metadata = Metadata
        }, RemoteClient);
    }

    public async Task AttackEntityAsync(ILivingEntity targetEntity)
    {
        await Server!.BroadcastPacketAsync(new EntityStatusPacket
        {
            EntityId = targetEntity.EntityId,
            Status = EntityStatus.Hurt
        });

        var multiplier = targetEntity.KnockBackMultiplier;
        await Server.BroadcastPacketAsync(new EntityVelocityPacket
        {
            EntityId = targetEntity.EntityId,
            VelocityX = (short)(-MinecraftMath.SinDegree(Yaw) * 3000 * multiplier.X),
            VelocityY = (short)(targetEntity.OnGround ? 3000 * multiplier.Y : 0),
            VelocityZ = (short)(MinecraftMath.CosDegree(Yaw) * 3000 * multiplier.Z)
        });
        //TODO Adapt damage depending on player's weapon/tool
        //var selectedItem = remotePlayer.
        var damage = HoldItem == ItemStack.Empty ? 1 : ItemInfoProvider.Get(HoldItem.ItemId).DamageOnEntity;
        await targetEntity.SetHealthAsync((short)(targetEntity.Health - damage));
    }

    private async Task BroadcastHoldItemAsync()
    {
        await Server!.BroadcastPacketAsync(new EntityEquipmentPacket
        {
            EntityId = EntityId,
            Slot = 0,
            ItemId = HoldItem.ItemId
        }, RemoteClient);
    }

    public override async Task SetHealthAsync(short health)
    {
        Health = Math.Clamp(health, (short)0, MaxHealth);
        await RemoteClient.SendPacketAsync(new UpdateHealthPacket
        {
            Health = health
        });

        if (Health == 0)
        {
            await Server!.BroadcastPacketAsync(new EntityStatusPacket
            {
                EntityId = EntityId,
                Status = EntityStatus.Dead
            }, RemoteClient);

            //TODO This is a bit dirty
            Server!.Looper.Schedule(TimeSpan.FromSeconds(1.5), async _ =>
            {
                if (Health == 0)
                {
                    await Server!.BroadcastPacketAsync(new DestroyEntityPacket
                    {
                        EntityId = EntityId
                    }, RemoteClient);
                }
            });
        }
    }

    public async Task RespawnAsync(MinecraftDimension dimension)
    {
        //TODO This is a bit dirty
        if (Server!.EntityManager.EntityExists(RemoteClient.Player!.EntityId))
        {
            await Server.BroadcastPacketAsync(new DestroyEntityPacket
            {
                EntityId = RemoteClient.Player!.EntityId
            }, RemoteClient);
        }

        Respawning = true;

        var spawnHeight = await Server!.World.GetHighestBlockHeightAsync(new Vector2i(0, 0)) + 1.6200000047683716;
        Position = new Vector3d(0.5, spawnHeight, 0.5);
        Stance = Position.Y + Height;
        OnGround = true;
        Yaw = 0;
        Pitch = 0;

        await RemoteClient.UnloadChunksAsync();

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

        await RemoteClient.UpdateLoadedChunksAsync();

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
                X = player.Position.X.ToAbsoluteInt(),
                Y = player.Position.Y.ToAbsoluteInt(),
                Z = player.Position.Z.ToAbsoluteInt(),
                Yaw = MinecraftMath.RotationFloatToSByte(Yaw),
                Pitch = MinecraftMath.RotationFloatToSByte(Pitch),
                CurrentItem = 0
            });
        }

        //TODO Handle spawn point correctly
        await Server!.BroadcastPacketAsync(new NamedEntitySpawnPacket
        {
            EntityId = EntityId,
            X = Position.X.ToAbsoluteInt(),
            Y = Position.Y.ToAbsoluteInt(),
            Z = Position.Z.ToAbsoluteInt(),
            Yaw = MinecraftMath.RotationFloatToSByte(Yaw),
            Pitch = MinecraftMath.RotationFloatToSByte(Pitch),
            Username = Username,
            CurrentItem = 0
        }, RemoteClient);

        await SetHealthAsync(MaxHealth);

        Respawning = false;
    }

    private async Task SendPositionAndLookAsync()
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
}