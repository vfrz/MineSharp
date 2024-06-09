using MineSharp.Content;
using MineSharp.Entities;
using MineSharp.Numerics;

namespace MineSharp.Core;

public interface IPlayer : ILivingEntity
{
    public Task TeleportAsync(Vector3<double> position);

    public Task<bool> TryGiveItemAsync(ItemStack itemStack);
    public Task ClearInventoryAsync();
}