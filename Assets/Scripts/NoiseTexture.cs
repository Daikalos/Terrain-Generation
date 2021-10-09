using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTexture : MonoBehaviour
{
    [SerializeField] private RenderTexture _RenderTexture;
    [SerializeField] private Renderer _Renderer;

    private Texture2D _Texture;

    public void CreateTexture()
    {
        _Texture = new Texture2D(_RenderTexture.width, _RenderTexture.height);
        _Renderer.sharedMaterial.mainTexture = _Texture;

        RenderTexture.active = _RenderTexture;

        _Texture.ReadPixels(new Rect(0, 0, _RenderTexture.width, _RenderTexture.height), 0, 0);
        for (int x = 0; x < _RenderTexture.width; ++x)
        {
            for (int y = 0; y < _RenderTexture.height; ++y)
            {
                float noise = (float)CustomNoise.GetNoise(
                    x / (_RenderTexture.width / 256.0) + 0.5, 
                    y / (_RenderTexture.height / 256.0) + 0.5);

                _Texture.SetPixel(x, y, new Color(noise, noise, noise));
            }
        }
        _Texture.Apply();

        RenderTexture.active = null;
    }
}
