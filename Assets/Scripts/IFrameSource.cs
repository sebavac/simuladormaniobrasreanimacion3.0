using UnityEngine;

public interface IFrameSource
{
    // Devuelve un frame como Texture (idealmente Texture2D o RenderTexture)
    Texture GetTexture();
    int Width { get; }
    int Height { get; }
    bool IsReady { get; }
}
