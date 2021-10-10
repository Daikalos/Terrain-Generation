using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainMesh : MonoBehaviour
{
    [Space(5), Header("Terrain")]
    [SerializeField, Range(1, 256)] private int _Width = 8;
    [SerializeField, Range(1, 256)] private int _Height = 8;
    [SerializeField, Range(0.001f, 8)] private float _CellSize = 1.0f;
    [SerializeField] private Gradient _ColorGradient;

    [Header("Seed")]
    [SerializeField] private bool _UseSeed = false;
    [SerializeField] private int _Seed;

    [Space(5), Header("Noise")]
    [SerializeField] private CustomNoiseParameters[] _Octaves;

    private Mesh _Mesh;
    private Vector3[] _Vertices;
    private int[] _Triangles;
    private Color[] _Colors;
    private float _TileSize;

    public int Width => _Width;
    public int Height => _Height;
    public float TileSize => _TileSize;

    private void OnValidate()
    {
        CreateMesh();
        AddNoise();

        _Mesh.Clear();
        _Mesh.vertices = _Vertices;
        _Mesh.triangles = _Triangles;
        _Mesh.colors = _Colors;
        _Mesh.RecalculateNormals();
    }

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

            meshRenderer.hideFlags = HideFlags.HideInInspector;
            meshFilter.hideFlags = HideFlags.HideInInspector;

            meshRenderer.sharedMaterial = new Material(Shader.Find("Custom/TerrainShader"));
            meshFilter.mesh = new Mesh();

            _Mesh = meshFilter.sharedMesh;

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

                _Vertices[i] = new Vector3(x * _CellSize, 0, -z * _CellSize);

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
        float yMin = float.MaxValue;
        float yMax = -float.MaxValue;

        if (_UseSeed) 
            CustomNoise.SetSeed(_Seed);
        else 
            CustomNoise.Restore();

        for (int x = 0; x < _Width; ++x)
        {
            for (int z = 0; z < _Height; ++z)
            {
                int i = x + z * _Width;

                float yVertex = 0.0f;

                for (int j = 0; j < _Octaves.Length; ++j)
                {
                    yVertex += (float)_Octaves[j].PerlinNoise(x, z);
                }

                if (yVertex > yMax)
                    yMax = yVertex;
                if (yVertex < yMin)
                    yMin = yVertex;

                _Vertices[i] = new Vector3(_Vertices[i].x, yVertex, _Vertices[i].z);
            }
        }

        for (int x = 0; x < _Width; ++x)
        {
            for (int z = 0; z < _Height; ++z)
            {
                int i = x + z * _Width;

                float posPercentage = Mathf.InverseLerp(yMin, yMax, _Vertices[i].y);
                _Colors[i] = _ColorGradient.Evaluate(1.0f - posPercentage);
            }
        }
    }

    public void Generate()
    {
        _Mesh = null;

        CreateMesh();
        AddNoise();

        _Mesh.Clear();
        _Mesh.vertices = _Vertices;
        _Mesh.triangles = _Triangles;
        _Mesh.colors = _Colors;
        _Mesh.RecalculateNormals();
    }
}
