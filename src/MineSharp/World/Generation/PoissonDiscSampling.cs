using System.Numerics;

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
        public Vector2 TopLeft, LowerRight, Center;
        public Vector2 Dimensions;
        public float? RejectionSqDistance;
        public float MinimumDistance;
        public float CellSize;
        public int GridWidth, GridHeight;
    }

    private struct State
    {
        public Vector2?[,] Grid;
        public List<Vector2> ActivePoints, Points;
    }

    private readonly int _seed;

    public UniformPoissonDiskSampler(int seed)
    {
        _seed = seed;
    }

    public List<Vector2> SampleRectangle(int x, int y, int length, int width, float minimumDistance, float? rejectionDistance = null)
    {
        var topLeft = new Vector2(x, y);
        var lowerRight = new Vector2(x + width - 1, y + length - 1);
        
        var random = new Random(_seed + x + y * length);
        
        var settings = new Settings
        {
            TopLeft = topLeft, LowerRight = lowerRight,
            Dimensions = lowerRight - topLeft,
            Center = (topLeft + lowerRight) / 2,
            CellSize = minimumDistance / SquareRootTwo,
            MinimumDistance = minimumDistance,
            RejectionSqDistance = rejectionDistance * rejectionDistance
        };
        settings.GridWidth = (int) (settings.Dimensions.X / settings.CellSize) + 1;
        settings.GridHeight = (int) (settings.Dimensions.Y / settings.CellSize) + 1;

        var state = new State
        {
            Grid = new Vector2?[settings.GridWidth, settings.GridHeight],
            ActivePoints = new List<Vector2>(),
            Points = new List<Vector2>()
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
            var d = random.NextDouble();
            var xr = settings.TopLeft.X + settings.Dimensions.X * d;

            d = random.NextDouble();
            var yr = settings.TopLeft.Y + settings.Dimensions.Y * d;

            var p = new Vector2((float) xr, (float) yr);
            if (settings.RejectionSqDistance != null && Vector2.DistanceSquared(settings.Center, p) > settings.RejectionSqDistance)
                continue;
            added = true;

            var index = Denormalize(p, settings.TopLeft, settings.CellSize);

            state.Grid[(int) index.X, (int) index.Y] = p;

            state.ActivePoints.Add(p);
            state.Points.Add(p);
        }
    }

    private static bool AddNextPoint(Vector2 point, ref Settings settings, ref State state, Random random)
    {
        var found = false;
        var q = GenerateRandomAround(point, settings.MinimumDistance, random);

        if (q.X >= settings.TopLeft.X && q.X < settings.LowerRight.X &&
            q.Y > settings.TopLeft.Y && q.Y < settings.LowerRight.Y &&
            (settings.RejectionSqDistance == null || Vector2.DistanceSquared(settings.Center, q) <= settings.RejectionSqDistance))
        {
            var qIndex = Denormalize(q, settings.TopLeft, settings.CellSize);
            var tooClose = false;

            for (var i = (int) Math.Max(0, qIndex.X - 2); i < Math.Min(settings.GridWidth, qIndex.X + 3) && !tooClose; i++)
            for (var j = (int) Math.Max(0, qIndex.Y - 2); j < Math.Min(settings.GridHeight, qIndex.Y + 3) && !tooClose; j++)
                if (state.Grid[i, j].HasValue && Vector2.Distance(state.Grid[i, j]!.Value, q) < settings.MinimumDistance)
                    tooClose = true;

            if (!tooClose)
            {
                found = true;
                state.ActivePoints.Add(q);
                state.Points.Add(q);
                state.Grid[(int) qIndex.X, (int) qIndex.Y] = q;
            }
        }

        return found;
    }

    private static Vector2 GenerateRandomAround(Vector2 center, float minimumDistance, Random random)
    {
        var d = random.NextDouble();
        var radius = minimumDistance + minimumDistance * d;

        d = random.NextDouble();
        var angle = Pi2 * d;

        var newX = radius * Math.Sin(angle);
        var newY = radius * Math.Cos(angle);

        return new Vector2((float) (center.X + newX), (float) (center.Y + newY));
    }

    private static Vector2 Denormalize(Vector2 point, Vector2 origin, double cellSize)
    {
        return new Vector2((int) ((point.X - origin.X) / cellSize), (int) ((point.Y - origin.Y) / cellSize));
    }
}