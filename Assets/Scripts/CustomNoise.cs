using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public struct Vector2d
{
    public double x, y;
    public Vector2d(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public static Vector2d operator +(Vector2d lhs, Vector2d rhs) => new Vector2d(lhs.x + rhs.x, lhs.y + rhs.y);
    public static Vector2d operator -(Vector2d lhs, Vector2d rhs) => new Vector2d(lhs.x - rhs.x, lhs.y - rhs.y);

    public Vector2d Normalize()
    {
        double length = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        return new Vector2d(x / length, y / length);
    }
}

[Serializable]
public class CustomNoise
{
    private static int[] _Table;
    private static readonly int _TableSize = 256;
    private static readonly int _TableMask = _TableSize - 1;

    private static int _Seed = 0;

    public static void SetSeed(int seed)
    {
        if (seed == _Seed)
            return;

        _Seed = seed;

        Random rng = new Random(_Seed.GetHashCode());

        _Table = new int[_TableSize * 2];
        for (int i = 0; i < _TableSize; ++i)
        {
            _Table[i] = i;
        }
        for (int i = _TableSize - 1; i > 0; --i)
        {
            int r = rng.Next(0, i + 1);

            // swap
            int temp = _Table[i];
            _Table[i] = _Table[r];
            _Table[r] = temp;
        }
        for (int i = 0; i < _Table.Length; ++i)
        {
            _Table[_TableSize + i] = _Table[i];
        }
    }

    public static void Restore()
    {
        _Table = _KenPerlinHash;
        _Seed = 0;
    }

    public static double GetNoise(double x, double y)
    {
        // get corners of the cell
        // & _HashMask to stay in range of gradient grid
        int x0 = (int)x & _TableMask;
        int y0 = (int)y & _TableMask;
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        // get relative coordinates within cell
        double xr = x - (int)x; 
        double yr = y - (int)y;

        // value for each corner to determine gradient
        int v0 = _Table[_Table[x0] + y0];
        int v1 = _Table[_Table[x1] + y0];
        int v2 = _Table[_Table[x0] + y1];
        int v3 = _Table[_Table[x1] + y1];

        // get gradient vectors
        Vector2d g0 = Gradient(v0);
        Vector2d g1 = Gradient(v1);
        Vector2d g2 = Gradient(v2);
        Vector2d g3 = Gradient(v3);

        Vector2d p0 = new Vector2d(xr, yr) - new Vector2d(0.0f, 0.0f); // from top left to point in cell
        Vector2d p1 = new Vector2d(xr, yr) - new Vector2d(1.0f, 0.0f); // from top right to point in cell
        Vector2d p2 = new Vector2d(xr, yr) - new Vector2d(0.0f, 1.0f); // from bot left to point in cell
        Vector2d p3 = new Vector2d(xr, yr) - new Vector2d(1.0f, 1.0f); // from bot right to point in cell

        // get dot products for each corner
        // uses dot product between gradient and distance vector from point in cell to corner
        double d0 = Dot(g0.x, g0.y, p0.x, p0.y);
        double d1 = Dot(g1.x, g1.y, p1.x, p1.y);
        double d2 = Dot(g2.x, g2.y, p2.x, p2.y);
        double d3 = Dot(g3.x, g3.y, p3.x, p3.y);

        double xf = Fade(xr);
        double yf = Fade(yr);

        double noise = (Lerp(Lerp(d0, d1, xf), Lerp(d2, d3, xf), yf) + 1) / 2.0;

        return noise;
    }

    /// <summary>
    /// pseudorandom gradient using the permutation table
    /// </summary>
    private static Vector2d Gradient(int hash)
    {
        switch (hash & 3)
        {
            case 0: return new Vector2d( 1.0,  1.0).Normalize();
            case 1: return new Vector2d(-1.0,  1.0).Normalize();
            case 2: return new Vector2d( 1.0, -1.0).Normalize();
            case 3: return new Vector2d(-1.0, -1.0).Normalize();
            default: 
                return new Vector2d(0, 0);
        }
    }

    /// <summary>
    /// ken perlin's fade function
    /// </summary>
    private static double Fade(double t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static double Lerp(double a, double b, double t)
    {
        return t * (b - a) + a;
    }

    private static double Dot(double x0, double y0, double x1, double y1)
    {
        return x0 * x1 + y0 * y1;
    }

    /// <summary>
    /// ken perlin's permutation table, used as a table to select pseudorandom gradients
    /// </summary>
    private static readonly int[] _KenPerlinHash = 
    {
        151, 160, 137,  91,  90,  15, 131,  13, 201,  95,  96,  53, 194, 233,   7, 225,
        140,  36, 103,  30,  69, 142,   8,  99,  37, 240,  21,  10,  23, 190,   6, 148,
        247, 120, 234,  75,   0,  26, 197,  62,  94, 252, 219, 203, 117,  35,  11,  32,
         57, 177,  33,  88, 237, 149,  56,  87, 174,  20, 125, 136, 171, 168,  68, 175,
         74, 165,  71, 134, 139,  48,  27, 166,  77, 146, 158, 231,  83, 111, 229, 122,
         60, 211, 133, 230, 220, 105,  92,  41,  55,  46, 245,  40, 244, 102, 143,  54,
         65,  25,  63, 161,   1, 216,  80,  73, 209,  76, 132, 187, 208,  89,  18, 169,
        200, 196, 135, 130, 116, 188, 159,  86, 164, 100, 109, 198, 173, 186,   3,  64,
         52, 217, 226, 250, 124, 123,   5, 202,  38, 147, 118, 126, 255,  82,  85, 212,
        207, 206,  59, 227,  47,  16,  58,  17, 182, 189,  28,  42, 223, 183, 170, 213,
        119, 248, 152,   2,  44, 154, 163,  70, 221, 153, 101, 155, 167,  43, 172,   9,
        129,  22,  39, 253,  19,  98, 108, 110,  79, 113, 224, 232, 178, 185, 112, 104,
        218, 246,  97, 228, 251,  34, 242, 193, 238, 210, 144,  12, 191, 179, 162, 241,
         81,  51, 145, 235, 249,  14, 239, 107,  49, 192, 214,  31, 181, 199, 106, 157,
        184,  84, 204, 176, 115, 121,  50,  45, 127,   4, 150, 254, 138, 236, 205,  93,
        222, 114,  67,  29,  24,  72, 243, 141, 128, 195,  78,  66, 215,  61, 156, 180,

        151, 160, 137,  91,  90,  15, 131,  13, 201,  95,  96,  53, 194, 233,   7, 225, // double to avoid overflow
        140,  36, 103,  30,  69, 142,   8,  99,  37, 240,  21,  10,  23, 190,   6, 148,
        247, 120, 234,  75,   0,  26, 197,  62,  94, 252, 219, 203, 117,  35,  11,  32,
         57, 177,  33,  88, 237, 149,  56,  87, 174,  20, 125, 136, 171, 168,  68, 175,
         74, 165,  71, 134, 139,  48,  27, 166,  77, 146, 158, 231,  83, 111, 229, 122,
         60, 211, 133, 230, 220, 105,  92,  41,  55,  46, 245,  40, 244, 102, 143,  54,
         65,  25,  63, 161,   1, 216,  80,  73, 209,  76, 132, 187, 208,  89,  18, 169,
        200, 196, 135, 130, 116, 188, 159,  86, 164, 100, 109, 198, 173, 186,   3,  64,
         52, 217, 226, 250, 124, 123,   5, 202,  38, 147, 118, 126, 255,  82,  85, 212,
        207, 206,  59, 227,  47,  16,  58,  17, 182, 189,  28,  42, 223, 183, 170, 213,
        119, 248, 152,   2,  44, 154, 163,  70, 221, 153, 101, 155, 167,  43, 172,   9,
        129,  22,  39, 253,  19,  98, 108, 110,  79, 113, 224, 232, 178, 185, 112, 104,
        218, 246,  97, 228, 251,  34, 242, 193, 238, 210, 144,  12, 191, 179, 162, 241,
         81,  51, 145, 235, 249,  14, 239, 107,  49, 192, 214,  31, 181, 199, 106, 157,
        184,  84, 204, 176, 115, 121,  50,  45, 127,   4, 150, 254, 138, 236, 205,  93,
        222, 114,  67,  29,  24,  72, 243, 141, 128, 195,  78,  66, 215,  61, 156, 180,
    };
}
