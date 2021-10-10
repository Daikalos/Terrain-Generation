using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HeatmapTexture : MonoBehaviour
{
    [SerializeField] private RenderTexture _RenderTexture;
    [SerializeField] private Renderer _Renderer;
    [Space(10)]
    [SerializeField] private int _Count;
    [SerializeField] private Gradient _HeatmapGradient;

    private Texture2D _Texture;

    private int[] _Heatmap;
    private int _MaxHeat;

    private CustomNoiseParameters[] customNoises;
    private float[] _YCoords;

    public void Run()
    {
        _Heatmap = new int[_RenderTexture.width * _RenderTexture.height];
        _MaxHeat = -int.MaxValue;

        for (int i = 0; i < _Count; ++i) // simulate creating new terrain
        {
            int width = Random.Range(32, 64);
            int height = Random.Range(32, 64);

            _YCoords = new float[(width + 1) * (height + 1)];

            int octaveCount = Random.Range(1, 8);
            customNoises = new CustomNoiseParameters[octaveCount];

            for (int j = 0; j < octaveCount; ++j) // initialize octaves
            {
                customNoises[j] = new CustomNoiseParameters
                {
                    Position = new Vector2(Random.Range(0.0f, 512.0f), Random.Range(0, 512.0f)),
                    Amplitude = Random.Range(0.01f, 1024.0f),
                    Frequency = Random.Range(0.01f, 1024.0f)
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
            for (int z = 0; z < height; ++z)
            {
                float noise = 0.0f;
                for (int i = 0; i < octaveCount; ++i)
                {
                    noise += customNoises[i].PerlinNoise(x, z);
                }

                if (noise > max)
                    max = noise;
                if (noise < min)
                    min = noise;

                _YCoords[x + z * width] = noise;
            }
        }
    }

    private void AddToHeatmap(int width, int height, float min, float max)
    {
        float linearity = 0.0f;
        float leniency = 0.0f;

        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                int i = x + z * width;

                float vertexProc = Mathf.InverseLerp(min, max, _YCoords[i]);

                linearity += vertexProc;
                leniency += (vertexProc < 0.3f) ? 1.0f : 0.0f;
            }
        }

        linearity /= (width * height);
        leniency /= (width * height);

        int pos = (int)(linearity * _RenderTexture.width - 1) + (int)(leniency * _RenderTexture.height - 1) * _RenderTexture.width;

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
                _Texture.SetPixel(y, (_RenderTexture.height - 1) - x, _HeatmapGradient.Evaluate(color));
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
