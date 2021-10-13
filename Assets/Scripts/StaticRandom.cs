using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class StaticRandom
{
    private static Random Random = new Random();
    private static readonly object SyncLock = new object();

    public static int Range(int min, int max)
    {
        lock (SyncLock) // synchronize
        {
            return Random.Next(min, max);
        }
    }

    public static float Range(float min, float max)
    {
        lock (SyncLock)
        {
            return (float)Random.NextDouble() * (max - min) + min;
        }
    }

    public static double Range(double min, double max)
    {
        lock (SyncLock)
        {
            return Random.NextDouble() * (max - min) + min;
        }
    }
}
