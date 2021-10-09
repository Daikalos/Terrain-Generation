using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
        _MaxHeat = -int.MaxValue;

        for (int i = 0; i < _Heatmap.Length; ++i)
            _Heatmap[i] = 0;

        for (int i = 0, count = 2048; i < count; ++i) // simulate creating new terrain
        {
            int width = Random.Range(4, 256);
            int height = Random.Range(4, 256);

            _YCoords = new float[(width + 1) * (height + 1)];
            _ColorsProc = new float[_YCoords.Length];

            int octaveCount = Random.Range(1, 5);
            customNoises = new CustomNoiseParameters[octaveCount];

            for (int j = 0; j < octaveCount; ++j) // initialize octaves
            {
                customNoises[j] = new CustomNoiseParameters
                {
                    Position = new Vector2(Random.Range(0, 512), Random.Range(0, 512)),
                    Amplitude = Random.Range(1, 512),
                    Frequency = Random.Range(1, 512)
                };
            }

            SimulateTerrain(octaveCount, width, height, out float minValue, out float maxValue);

            AddToHeatmap(width, height, minValue, maxValue);
        }

        AssignTexture();
    }

    private void SimulateTerrain(int octaveCount, int width, int height, out float min, out float max)
    {
        min = float.MaxValue;
        max = -float.MaxValue;

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

                if (noise > max)
                    max = noise;
                if (noise < min)
                    min = noise;

                _YCoords[i] = noise;
            }
        }

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                int i = x + y * width;
                _ColorsProc[i] = Mathf.InverseLerp(min, max, _YCoords[i]);
            }
        }
    }

    private void AddToHeatmap(int width, int height, float min, float max)
    {
        float linearity = 0.0f;
        float leniency = 0.0f;

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                int i = x + y * width;

                linearity += ((max - min) != 0) ? (_YCoords[i] - min) / (max - min) : 1.0f;
                leniency += (_ColorsProc[i] < 0.3f) ? 1.0f : 0.0f;
            }
        }

        linearity /= (width * height);
        leniency /= (width * height);

        int pos = (int)(linearity * (_RenderTexture.width - 1)) + (int)(leniency * (_RenderTexture.height - 1)) * _RenderTexture.width;

        if (++_Heatmap[pos] > _MaxHeat)
            _MaxHeat = _Heatmap[pos];
    }

    private void AssignTexture()
    {
        _Texture = new Texture2D(_RenderTexture.width, _RenderTexture.height);
        _Renderer.sharedMaterial.mainTexture = _Texture;

        RenderTexture.active = _RenderTexture;

        _Texture.ReadPixels(new Rect(0, 0, _RenderTexture.width, _RenderTexture.height), 0, 0);
        for (int x = 0; x < _RenderTexture.width; ++x)
        {
            for (int y = 0; y < _RenderTexture.height; ++y)
            {
                float color = _Heatmap[x + y * _RenderTexture.width] / (float)_MaxHeat;
                _Texture.SetPixel(x, y, _HeatmapGradient.Evaluate(color));
            }
        }
        _Texture.Apply();

        RenderTexture.active = null;
    }

    private void OnDrawGizmos()
    {
        Bounds bounds = GetComponent<MeshRenderer>().bounds;

        GUIStyle style = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24
        };

        Handles.Label(transform.position + new Vector3(-bounds.size.x / 2,  bounds.size.y / 2, 0) + new Vector3(-4,  4, 0), "1.0", style);
        Handles.Label(transform.position + new Vector3(-bounds.size.x / 2, -bounds.size.y / 2, 0) + new Vector3(-4, -1, 0), "0.0", style);
        Handles.Label(transform.position + new Vector3( bounds.size.x / 2, -bounds.size.y / 2, 0) + new Vector3( 1, -1, 0), "1.0", style);

        Handles.Label(transform.position + new Vector3(0, -bounds.size.y / 2, 0) + new Vector3(0, -2, 0), "Linearity", style);
        Handles.Label(transform.position + new Vector3(-bounds.size.x / 2, 0, 0) + new Vector3(-12,  0, 0), "Leniency", style);
    }
}
