using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vector2d
{
    public double x, y;
}

[System.Serializable]
public class CustomNoise
{
    [SerializeField] private int _Seed;
    [Space(10)]
    [SerializeField] private Vector2d _Position;
    [SerializeField, Range(2, 128)] private double _Scale;
    [SerializeField, Range(2, 128)] private double _Roughness;

    public double GetNoise(double x, double y)
    {
        double noise = 0.0f;

        x = _Position.x + (x / _Roughness); // effects
        y = _Position.y + (y / _Roughness);

        // get corners of the cell
        int x0 = (int)x; 
        int x1 = x0 + 1;
        int y0 = (int)y;
        int y1 = y0 + 1;

        // get relative coordinates within cell
        x = x - x0; 
        y = y - y0;



        return noise * _Scale;
    }

    /// <summary>
    /// pseudorandom approach
    /// </summary>
    private Vector2 Gradient(int x, int y)
    {


        return Vector2.zero;
    }

    private double Fade(double t)
    {
        return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    }
}
