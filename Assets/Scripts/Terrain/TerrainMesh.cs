using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainMesh : MonoBehaviour
{
    [Space(5)]
    [Header("Terrain")]
    [SerializeField, Range(1, 512)] 
    private int _Width = 8;
    [SerializeField, Range(1, 512)] 
    private int _Height = 8;
    [SerializeField, Range(1, 512)]
    private int _MaxWidth = 512;
    [SerializeField, Range(1, 512)]
    private int _MaxHeight = 512;
    [SerializeField, Range(0.001f, 8)] 
    private float _CellSize = 1.0f;

    [Space(10)]
    [MinMaxRange(0.0f, 1.0f)]
    public MinMaxFloat MountainsRidgeRange;
    [MinMaxRange(0.0f, 1.0f)]
    public MinMaxFloat MountainsRange;
    [MinMaxRange(0.0f, 1.0f)]
    public MinMaxFloat PlainsRange;
    [MinMaxRange(0.0f, 1.0f)]
    public MinMaxFloat BeachesRange;
    [MinMaxRange(0.0f, 1.0f)]
    public MinMaxFloat OceansRange;

    [Space(10)]
    [SerializeField] 
    private Gradient _ColorGradient;

    [Header("Seed")]
    [SerializeField] 
    private bool _UseSeed = false;
    [SerializeField, ConditionalField("_UseSeed"), Min(0)] 
    private int _Seed;

    [Space(5), Header("Noise")]
    [SerializeField] 
    private CustomNoiseParameters[] _Octaves;

    private Mesh _Mesh;
    private Vector3[] _Vertices;
    private int[] _Triangles;
    private Color[] _Colors;

    private float _Min, _Max;
    private float _TileSize;

    [NonSerialized]
    public bool[] MountainsRidge;
    [NonSerialized]
    public bool[] Mountains;
    [NonSerialized]
    public bool[] Plains;
    [NonSerialized]
    public bool[] Beaches;
    [NonSerialized]
    public bool[] Rivers;
    [NonSerialized]
    public bool[] Oceans;

    public Mesh Mesh => _Mesh;

    public int Width => _Width;
    public int Height => _Height;
    public float Min => _Min;
    public float Max => _Max;
    public float TileSize => _TileSize;

    public void CreateMesh()
    {
        if (_Mesh == null)
        {
            bool meshRendererExists = (gameObject.GetComponent<MeshRenderer>() != null);
            bool meshFilterExists = (gameObject.GetComponent<MeshFilter>() != null);

            MeshRenderer meshRenderer = (meshRendererExists) ? 
                gameObject.GetComponent<MeshRenderer>() : 
                gameObject.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = (meshFilterExists) ?
                gameObject.GetComponent<MeshFilter>() :
                gameObject.AddComponent<MeshFilter>();

            _Mesh = new Mesh();

            meshRenderer.material = new Material(Shader.Find("Custom/TerrainShader"));
            meshFilter.mesh = _Mesh;

            _Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        if (_Vertices == null || _Vertices.Length != (_Width + 1) * (_Height + 1) || _TileSize != _CellSize) // if mesh has been changed
        {
            _Vertices = new Vector3[(_Width + 1) * (_Height + 1)];
            _Triangles = new int[(_Width - 1) * (_Height - 1) * 2 * 3];
            _Colors = new Color[_Vertices.Length];

            BuildMesh();
        }
    }

    public void BuildMesh()
    {
        _TileSize = _CellSize;

        int triIndex = 0;
        for (int x = 0; x < _Width; ++x)
        {
            for (int z = 0; z < _Height; ++z)
            {
                int i = x + z * _Width;

                _Vertices[i] = new Vector3(x * _CellSize * (_MaxWidth / (float)_Width), 0, -z * _CellSize * (_MaxHeight / (float)_Height));

                if (x != _Width - 1 && z != _Height - 1)
                {
                    _Triangles[triIndex] = i;
                    _Triangles[triIndex + 1] = i + _Width + 1;
                    _Triangles[triIndex + 2] = i + _Width;

                    _Triangles[triIndex + 3] = i;
                    _Triangles[triIndex + 4] = i + 1;
                    _Triangles[triIndex + 5] = i + _Width + 1;

                    triIndex += 6;
                }
            }
        }
    }

    public void AddNoise()
    {
        _Min = float.PositiveInfinity;
        _Max = float.NegativeInfinity;

        if (_UseSeed) 
            CustomNoise.SetSeed(_Seed);
        else 
            CustomNoise.Restore();

        for (int x = 0; x < _Width; ++x)
        {
            for (int z = 0; z < _Height; ++z)
            {
                float yPos = 0.0f;
                for (int j = 0; j < _Octaves.Length; ++j)
                {
                    yPos += (float)_Octaves[j].PerlinNoise(x * (_MaxWidth / (float)_Width), z * (_MaxHeight / (float)_Height));
                }

                if (yPos > _Max)
                    _Max = yPos;
                if (yPos < _Min)
                    _Min = yPos;

                _Vertices[x + z * _Width].y = yPos;
            }
        }
    }

    public void SetColor()
    {
        for (int x = 0; x < _Width; ++x)
        {
            for (int z = 0; z < _Height; ++z)
            {
                int i = x + z * _Width;

                float posPercentage = Mathf.InverseLerp(_Min, _Max, _Vertices[i].y);
                _Colors[i] = _ColorGradient.Evaluate(posPercentage);
            }
        }
    }

    public void SetTerrainFeatures()
    {
        MountainsRidge = new bool[_Width * _Height];
        Mountains = new bool[_Width * _Height];
        Plains = new bool[_Width * _Height];
        Beaches = new bool[_Width * _Height];
        Rivers = new bool[_Width * _Height];
        Oceans = new bool[_Width * _Height];

        for (int x = 0; x < _Width; ++x)
        {
            for (int z = 0; z < _Height; ++z)
            {
                int i = x + z * _Width;

                float posPercentage = Mathf.InverseLerp(_Min, _Max, _Vertices[i].y);

                if (MountainsRidgeRange.IsInRange(posPercentage))
                    MountainsRidge[i] = true;
                if (MountainsRange.IsInRange(posPercentage))
                    Mountains[i] = true;
                if (PlainsRange.IsInRange(posPercentage))
                    Plains[i] = true;
                if (BeachesRange.IsInRange(posPercentage))
                    Beaches[i] = true;
                if (OceansRange.IsInRange(posPercentage))
                    Oceans[i] = true;
            }
        }
    }

    public void SetTerrainFeatures(float[] yCoords, int width, int height, float min, float max)
    {
        _Width = width;
        _Height = height;

        MountainsRidge = new bool[width * height];
        Mountains = new bool[width * height];
        Plains = new bool[width * height];
        Beaches = new bool[width * height];
        Rivers = new bool[width * height];
        Oceans = new bool[width * height];

        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                int i = x + z * width;

                float posPercentage = Mathf.InverseLerp(min, max, yCoords[i]);

                if (MountainsRidgeRange.IsInRange(posPercentage))
                    MountainsRidge[i] = true;
                if (MountainsRange.IsInRange(posPercentage))
                    Mountains[i] = true;
                if (PlainsRange.IsInRange(posPercentage))
                    Plains[i] = true;
                if (BeachesRange.IsInRange(posPercentage))
                    Beaches[i] = true;
                if (OceansRange.IsInRange(posPercentage))
                    Oceans[i] = true;
            }
        }
    }

    public void Generate()
    {
        CreateMesh();
        AddNoise();
        SetColor();
        SetTerrainFeatures();

        _Mesh.Clear(false);
        _Mesh.vertices = _Vertices;
        _Mesh.triangles = _Triangles;
        _Mesh.colors = _Colors;
        _Mesh.RecalculateNormals();
    }

    public void Generate(Vector3[] vertices)
    {
        _Vertices = vertices;

        SetColor();
        SetTerrainFeatures();

        _Mesh.Clear(false);
        _Mesh.vertices = _Vertices;
        _Mesh.triangles = _Triangles;
        _Mesh.colors = _Colors;
        _Mesh.RecalculateNormals();
    }

    public Vector2Int RandomPosition(bool[] terrainType, int width, int height)
    {
        int x, z, max = width + height;
        do
        {
            x = StaticRandom.Range(0, width);
            z = StaticRandom.Range(0, height);
        }
        while (!terrainType[x + z * width] && --max >= 0);

        return new Vector2Int(x, z);
    }
}
