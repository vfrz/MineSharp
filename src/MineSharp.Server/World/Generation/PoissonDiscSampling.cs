using MineSharp.Numerics;

namespace MineSharp.World.Generation;

/// <summary>
/// Original: https://theinstructionlimit.com/fast-uniform-poisson-disk-sampling-in-c
/// </summary>
public class UniformPoissonDiskSampler
{
    private const int DefaultPointsPerIteration = 30;

    private static readonly float SquareRootTwo = (float) Math.Sqrt(2);
    private const float Pi2 = MathF.PI * 2;

    private struct Settings
    {
        public Vector2<float> TopLeft, LowerRight, Center;
        public Vector2<float> Dimensions;
        public float? RejectionSqDistance;
        public float MinimumDistance;
        public float CellSize;
        public int GridWidth, GridHeight;
    }

    private struct State
    {
        public Vector2<float>?[,] Grid;
        public List<Vector2<float>> ActivePoints, Points;
    }

    private readonly int _seed;

    public UniformPoissonDiskSampler(int seed)
    {
        _seed = seed;
    }

    public List<Vector2<float>> SampleRectangle(int x, int z, int length, int width, float minimumDistance, float? rejectionDistance = null)
    {
        var topLeft = new Vector2<float>(x, z);
        var lowerRight = new Vector2<float>(x + width - 1, z + length - 1);

        var random = new Random(_seed + x + z * length);

        var settings = new Settings
        {
            TopLeft = topLeft, LowerRight = lowerRight,
            Dimensions = lowerRight - topLeft,
            Center = (topLeft + lowerRight) / 2f,
            CellSize = minimumDistance / SquareRootTwo,
            MinimumDistance = minimumDistance,
            RejectionSqDistance = rejectionDistance * rejectionDistance
        };
        settings.GridWidth = (int) (settings.Dimensions.X / settings.CellSize) + 1;
        settings.GridHeight = (int) (settings.Dimensions.Z / settings.CellSize) + 1;

        var state = new State
        {
            Grid = new Vector2<float>?[settings.GridWidth, settings.GridHeight],
            ActivePoints = new List<Vector2<float>>(),
            Points = new List<Vector2<float>>()
        };

        AddFirstPoint(ref settings, ref state, random);

        while (state.ActivePoints.Count != 0)
        {
            var listIndex = random.Next(state.ActivePoints.Count);

            var point = state.ActivePoints[listIndex];
            var found = false;

            for (var k = 0; k < DefaultPointsPerIteration; k++)
                found |= AddNextPoint(point, ref settings, ref state, random);

            if (!found)
                state.ActivePoints.RemoveAt(listIndex);
        }

        return state.Points;
    }

    private static void AddFirstPoint(ref Settings settings, ref State state, Random random)
    {
        var added = false;
        while (!added)
        {
            var d = random.NextSingle();
            var xr = settings.TopLeft.X + settings.Dimensions.X * d;

            d = random.NextSingle();
            var yr = settings.TopLeft.Z + settings.Dimensions.Z * d;

            var p = new Vector2<float>(xr, yr);
            if (settings.RejectionSqDistance != null && settings.Center.DistanceSquared(p) > settings.RejectionSqDistance)
                continue;
            added = true;

            var index = Denormalize(p, settings.TopLeft, settings.CellSize);

            state.Grid[(int) index.X, (int) index.Z] = p;

            state.ActivePoints.Add(p);
            state.Points.Add(p);
        }
    }

    private static bool AddNextPoint(Vector2<float> point, ref Settings settings, ref State state, Random random)
    {
        var found = false;
        var q = GenerateRandomAround(point, settings.MinimumDistance, random);

        if (q.X >= settings.TopLeft.X && q.X < settings.LowerRight.X &&
            q.Z > settings.TopLeft.Z && q.Z < settings.LowerRight.Z &&
            (settings.RejectionSqDistance == null || settings.Center.DistanceSquared(q) <= settings.RejectionSqDistance))
        {
            var qIndex = Denormalize(q, settings.TopLeft, settings.CellSize);
            var tooClose = false;

            for (var i = (int) Math.Max(0, qIndex.X - 2); i < Math.Min(settings.GridWidth, qIndex.X + 3) && !tooClose; i++)
            for (var j = (int) Math.Max(0, qIndex.Z - 2); j < Math.Min(settings.GridHeight, qIndex.Z + 3) && !tooClose; j++)
                if (state.Grid[i, j].HasValue && state.Grid[i, j]!.Value.DistanceSquared(q) < settings.MinimumDistance)
                    tooClose = true;

            if (!tooClose)
            {
                found = true;
                state.ActivePoints.Add(q);
                state.Points.Add(q);
                state.Grid[(int) qIndex.X, (int) qIndex.Z] = q;
            }
        }

        return found;
    }

    private static Vector2<float> GenerateRandomAround(Vector2<float> center, float minimumDistance, Random random)
    {
        var d = random.NextDouble();
        var radius = minimumDistance + minimumDistance * d;

        d = random.NextDouble();
        var angle = Pi2 * d;

        var newX = radius * Math.Sin(angle);
        var newZ = radius * Math.Cos(angle);

        return new Vector2<float>((float) (center.X + newX), (float) (center.Z + newZ));
    }

    private static Vector2<float> Denormalize(Vector2<float> point, Vector2<float> origin, double cellSize)
    {
        return new Vector2<float>((int) ((point.X - origin.X) / cellSize), (int) ((point.Z - origin.Z) / cellSize));
    }
}