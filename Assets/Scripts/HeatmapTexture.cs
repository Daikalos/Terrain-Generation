using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapTexture : MonoBehaviour
{
    [SerializeField] private RenderTexture _RenderTexture;
    [SerializeField] private Renderer _Renderer;
    [SerializeField] private Gradient _HeatmapGradient;

    private Texture2D _Texture;

    private int[] _Heatmap;
    private int _MaxHeat;

    private CustomNoiseParameters[] customNoises;
    private float[] _YCoords;
    private float[] _ColorsProc;

    public void Run()
    {
        _Heatmap = new int[_RenderTexture.width * _RenderTexture.height];

        for (int i = 0; i < _Heatmap.Length; ++i)
            _Heatmap[i] = 0;

        int count = 5;
        _MaxHeat = -int.MaxValue;

        for (int i = 0; i < count; ++i) // simulate creating new terrain
        {
            int width = Random.Range(16, 128);
            int height = Random.Range(16, 128);

            _YCoords = new float[(width + 1) * (height + 1)];
            _ColorsProc = new float[_YCoords.Length];

            int octaveCount = Random.Range(1, 5);
            customNoises = new CustomNoiseParameters[octaveCount];

            for (int j = 0; j < octaveCount; ++j) // initialize octaves
            {
                customNoises[j] = new CustomNoiseParameters();

                customNoises[j].Position = new Vector2(Random.Range(0, 128), Random.Range(0, 128));
                customNoises[j].Amplitude = Random.Range(1, 128);
                customNoises[j].Frequency = Random.Range(1, 128);
            }

            SimulateTerrain(octaveCount, width, height);

            AddToHeatmap();
        }

        AssignTexture();
    }

    private void SimulateTerrain(int octaveCount, int width, int height)
    {
        float maxValue = -float.MaxValue;
        float minValue = float.MaxValue;
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                int i = x + y * width;

                float noise = 0.0f;
                for (int j = 0; j < octaveCount; ++j)
                {
                    noise += customNoises[j].PerlinNoise(x, y);
                }

                if (noise > maxValue)
                    maxValue = noise;
                if (noise < minValue)
                    minValue = noise;

                _YCoords[i] = noise;
            }
        }

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                int i = x + y * width;
                _ColorsProc[i] = 1.0f - Mathf.InverseLerp(minValue, maxValue, _YCoords[i]);
            }
        }
    }

    private void AddToHeatmap(int width, int height)
    {
        float linearity = 0.0f;
        float leniency = 0.0f;

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                int i = x + y * width;

                linearity += _YCoords[i];

            }
        }
    }

    private void AssignTexture()
    {
        _Texture = new Texture2D(_RenderTexture.width, _RenderTexture.height);
        _Renderer.sharedMaterial.mainTexture = _Texture;

        RenderTexture.active = _RenderTexture;

        Debug.Log(_MaxHeat);

        _Texture.ReadPixels(new Rect(0, 0, _RenderTexture.width, _RenderTexture.height), 0, 0);
        for (int x = 0; x < _RenderTexture.width; ++x)
        {
            for (int y = 0; y < _RenderTexture.height; ++y)
            {
                float color = _Heatmap[x + y * _RenderTexture.width] / _MaxHeat;
                _Texture.SetPixel(x, y, _HeatmapGradient.Evaluate(color));
            }
        }
        _Texture.Apply();

        RenderTexture.active = null;
    }
}
