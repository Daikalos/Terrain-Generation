using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomNoiseParameters
{
    [SerializeField] private int _Seed;
    [Space(10)]
    [SerializeField] private Vector2 _Position;
    [SerializeField, Range(1, 128)] private float _Amplitude;
    [SerializeField, Range(0.00001f, 1.0f)] private float _Frequency;

    public int Seed => _Seed;
    public Vector2 Position => _Position;
    public float Amplitude => _Amplitude;
    public float Frequency => _Frequency;

    public float PerlinNoise(float x, float y)
    {
        return (float)CustomNoise.GetNoise(
            _Position.x + (x * _Frequency), 
            _Position.y + (y * _Frequency)) * _Amplitude;
    }
}
