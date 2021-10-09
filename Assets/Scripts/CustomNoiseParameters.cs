using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomNoiseParameters
{
    [SerializeField] private Vector2 _Position;
    [SerializeField, Range(1, 128)] private float _Amplitude;
    [SerializeField, Range(1, 128)] private float _Frequency;

    public Vector2 Position { get => _Position; set => _Position = value; }
    public float Amplitude { get => _Amplitude; set => _Amplitude = value; }
    public float Frequency { get => _Frequency; set => _Frequency = value; }

    public float PerlinNoise(float x, float y)
    {
        return (float)CustomNoise.GetNoise(
            _Position.x + (x / _Frequency), 
            _Position.y + (y / _Frequency)) * _Amplitude;
    }
}
