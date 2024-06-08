using MineSharp.Sdk.Core;

namespace MineSharp.Sdk;

public interface IPlayer : ILivingEntity
{
    public Task TeleportAsync(Vector3<double> position);

    public Task<bool> TryGiveItemAsync(ItemStack itemStack);
    public Task ClearInventoryAsync();
}