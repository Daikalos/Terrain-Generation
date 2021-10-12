using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class StaticRandom
{
    private static Random Random = new Random();
    private static readonly object SyncLock = new object();

    public static void Seed(int seed)
    {
        Random = new Random(seed.GetHashCode());
    }

    public static void Restore()
    {
        Random = new Random();
    }

    public static int RandomNumber(int min, int max)
    {
        lock (SyncLock) // synchronize
        {
            return Random.Next(min, max);
        }
    }

    public static double RandomDouble()
    {
        lock (SyncLock)
        {
            return Random.NextDouble();
        }
    }
}
