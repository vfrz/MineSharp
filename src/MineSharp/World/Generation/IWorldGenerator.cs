namespace MineSharp.World;

public interface IWorldGenerator
{
    ChunkData GenerateChunk(int chunkX, int chunkZ);
}