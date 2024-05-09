namespace MineSharp.World.Generation;

public interface IWorldGenerator
{
    void GenerateChunk(int chunkX, int chunkZ, IBlockChunkData chunkData);
}