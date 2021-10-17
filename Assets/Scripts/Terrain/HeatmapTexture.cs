using UnityEngine;
using UnityEditor;

public class HeatmapTexture : MonoBehaviour
{
    [SerializeField] private AgentSystem _AgentSystem;
    [SerializeField] private RenderTexture _RenderTexture;
    [SerializeField] private Renderer _Renderer;
    [Space(10)]
    [SerializeField] private bool _UseRandomSeeds;
    [SerializeField] private bool _UseAlternative;
    [SerializeField] private int _Samples;
    [SerializeField] private Gradient _HeatmapGradient;

    private Texture2D _Texture;

    private int[] _Heatmap;

    private CustomNoiseParameters[] customNoises;
    private float[] _YCoords;

    private float _OceanLevel = 0.30f;

    public void Run()
    {
        _Heatmap = new int[_RenderTexture.width * _RenderTexture.height];

        for (int i = 0; i < _Samples; ++i) // simulate creating new terrain
        {
            AgentSystem tempAgentSystem = _AgentSystem;

            int width = StaticRandom.Range(48, 128);
            int height = StaticRandom.Range(48, 128);

            _YCoords = new float[(width + 1) * (height + 1)];

            int octaveCount = StaticRandom.Range(1, 5);
            customNoises = new CustomNoiseParameters[octaveCount];

            if (_UseRandomSeeds)
                CustomNoise.SetSeed(StaticRandom.Range(0, int.MaxValue));

            for (int j = 0; j < octaveCount; ++j) // initialize octaves
            {
                customNoises[j] = new CustomNoiseParameters
                {
                    Position = new Vector2(StaticRandom.Range(-512f, 512f), StaticRandom.Range(-512f, 512f)),
                    Amplitude = StaticRandom.Range(-512f, 512f),
                    Frequency = StaticRandom.Range(-512f, 512f)
                };
            }

            SimulateTerrain(width, height, out float minValue, out float maxValue);

            _AgentSystem.Terrain.SetTerrainFeatures(_YCoords, width, height, minValue, maxValue);
            _YCoords = _AgentSystem.RandomExecution(_YCoords, width, height);

            if (_UseAlternative)
                AddToHeatmapAlternative(width, height, minValue, maxValue);
            else
                AddToHeatmap(width, height, minValue, maxValue);

            _AgentSystem = tempAgentSystem;
        }

        CustomNoise.Restore();

        AssignTexture();
    }

    private void SimulateTerrain(int width, int height, out float min, out float max)
    {
        min = float.MaxValue;
        max = -float.MaxValue;

        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                float noise = 0.0f;
                for (int i = 0; i < customNoises.Length; ++i)
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
                leniency += (vertexProc < _OceanLevel) ? 1.0f : 0.0f;
            }
        }

        linearity /= (width * height);
        leniency /= (width * height);

        int pos = (int)(linearity * _RenderTexture.width) + (int)(leniency * _RenderTexture.height) * _RenderTexture.width;

        ++_Heatmap[pos];
    }

    private void AddToHeatmapAlternative(int width, int height, float min, float max)
    {
        double sumR2 = 0.0f;
        int r2Count = 0;

        for (int x = 0; x < width; ++x)
        {
            double sumZ = 0.0f;
            double sumZ2 = 0.0f;
            double sumY = 0.0f;
            double sumY2 = 0.0f;
            double sumZY = 0.0f;

            for (int z = 0; z < height; ++z)
            {
                double y = _YCoords[x + z * width];

                sumZ += z;
                sumZ2 += z * z;
                sumY += y;
                sumY2 += y * y;
                sumZY += z * y;
            }

            double r2 = System.Math.Pow((height * sumZY - sumZ * sumY) / System.Math.Sqrt((height * sumZ2 - sumZ * sumZ) * (height * sumY2 - sumY * sumY)), 2);

            if (!double.IsNaN(r2))
            {
                sumR2 += r2;
                ++r2Count;
            }
        }

        for (int z = 0; z < height; ++z)
        {
            double sumX = 0.0f;
            double sumX2 = 0.0f;
            double sumY = 0.0f;
            double sumY2 = 0.0f;
            double sumXY = 0.0f;

            for (int x = 0; x < width; ++x)
            {
                double y = _YCoords[x + z * width];

                sumX += x;
                sumX2 += x * x;
                sumY += y;
                sumY2 += y * y;
                sumXY += x * y;
            }

            double r2 = System.Math.Pow((width * sumXY - sumX * sumY) / System.Math.Sqrt((width * sumX2 - sumX * sumX) * (width * sumY2 - sumY * sumY)), 2);

            if (!double.IsNaN(r2))
            {
                sumR2 += r2;
                ++r2Count;
            }
        }

        if (r2Count == 0)
            return;

        double leniency = 0.0;
        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                leniency += (Mathf.InverseLerp(min, max, _YCoords[x + z * width]) < _OceanLevel) ? 1.0 : 0.0;
            }
        }

        double linearity = sumR2 / r2Count;
        leniency /= (width * height);

        int pos = (int)(linearity * _RenderTexture.width) + (int)(leniency * _RenderTexture.height) * _RenderTexture.width;

        ++_Heatmap[pos];
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
                float color = _Heatmap[x + y * _RenderTexture.width] / 7.0f;
                _Texture.SetPixel(y, (_RenderTexture.height - 1) - x, _HeatmapGradient.Evaluate(color));
            }
        }
        _Texture.Apply();

        RenderTexture.active = null;

        int usingAlt = _UseAlternative ? 1 : 0;

        byte[] bytes = _Texture.EncodeToPNG();
        System.IO.File.WriteAllBytes($"C:/Users/Tobias/Desktop/heatmap-{usingAlt}.png", bytes);
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
