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
    [SerializeField, Range(0.001f, 8)] private float _TileSize = 1.0f;

    [Space(5), Header("Noise")]
    [SerializeField] private CustomNoiseParameters[] _Octaves;

    private Mesh _Mesh;
    private Vector3[] _Vertices;
    private int[] _Triangles;
    private Color[] _Colors;

    public int Width => _Width;
    public int Height => _Height;
    public float TileSize => _TileSize;

    private void OnValidate()
    {
        CreateMesh();
        BuildMesh();
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

        _Vertices = new Vector3[(_Width + 1) * (_Height + 1)];
        _Triangles = new int[(_Width - 1) * (_Height - 1) * 2 * 3];
        _Colors = new Color[_Vertices.Length];
    }

    public void BuildMesh()
    {
        int triIndex = 0;
        for (int x = 0; x < _Width; ++x)
        {
            for (int z = 0; z < _Height; ++z)
            {
                int i = x + z * _Width;

                _Vertices[i] = new Vector3(x * _TileSize, 0, -z * _TileSize);

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
        for (int x = 0; x < _Width; ++x)
        {
            for (int z = 0; z < _Height; ++z)
            {
                int i = x + z * _Width;

                float yCoord = 0.0f;
                for (int j = 0; j < _Octaves.Length; ++j)
                {
                    yCoord += (float)_Octaves[j].PerlinNoise(x, z);
                }

                _Vertices[i] = new Vector3(_Vertices[i].x, yCoord, _Vertices[i].z);

                float posPercentage = Mathf.InverseLerp(0.0f, 1.0f, yCoord);
                _Colors[i] = new Color(0, posPercentage, 0);
            }
        }
    }

    public void Generate()
    {
        _Mesh = null;

        CreateMesh();
        BuildMesh();
        AddNoise();

        _Mesh.Clear();
        _Mesh.vertices = _Vertices;
        _Mesh.triangles = _Triangles;
        _Mesh.colors = _Colors;
        _Mesh.RecalculateNormals();
    }
}
