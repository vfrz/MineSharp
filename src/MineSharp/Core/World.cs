namespace MineSharp.Core;

public class World
{
    public TwoDimensionalArray<Chunk?> Chunks { get; }

    public World()
    {
        Chunks = new TwoDimensionalArray<Chunk?>(-1, 2, -1, 2);
    }

    public void InitializeDefault()
    {
        for (var x = Chunks.LowerBoundX; x < Chunks.UpperBoundX; x++)
        {
            for (var z = Chunks.LowerBoundZ; z < Chunks.UpperBoundZ; z++)
            {
                var chunk = new Chunk(x ,z);
                chunk.FillDefault();
                Chunks[x, z] = chunk;
            }
        }
    }
}