using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomNoiseParameters
{
    [Header("Noise Attributes")]
    [SerializeField] private Vector2 _Position;
    [SerializeField, Range(1, 128)] private float _Amplitude;
    [SerializeField, Range(1, 128)] private float _Frequency;
    [Header("Seed")]
    [SerializeField] private bool _CustomHash = false;
    [SerializeField] private int _Seed;

    public bool CustomHash => _CustomHash;
    public int Seed => _Seed;
    public Vector2 Position => _Position;
    public float Amplitude => _Amplitude;
    public float Frequency => _Frequency;

    public float PerlinNoise(float x, float y)
    {
        if (_CustomHash)
            CustomNoise.SetSeed(_Seed);
        else
            CustomNoise.Restore();

        return (float)CustomNoise.GetNoise(
            _Position.x + (x / _Frequency), 
            _Position.y + (y / _Frequency)) * _Amplitude;
    }
}
