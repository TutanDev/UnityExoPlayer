using System;
using UnityEngine;
using UnityEngine.UI;

public class NativeRendererComponent : MonoBehaviour
{
    //component variable
    [SerializeField] RawImage m_rawimage;
    // member variable
    private Texture2D m_texture;
    private NativeRenderer m_renderer;
    private bool m_bPlaying;

    void Start()
    {
        // Create texture
        // filterMode : Specifies how to interpolate the image. point = do not interpolate. Other options are Bilinear and Trilinear.
        m_texture = new Texture2D(512, 512, TextureFormat.ARGB32, false, false);
        m_texture.filterMode = FilterMode.Point;

        // Set as texture for Unity's RawImage
        m_rawimage.texture = m_texture;

    }

    void Update()
    {
        if (m_bPlaying)
        {
            // Texture Update
            m_renderer.UpdateSurfaceTexture();
        }
    }

    // event processing
    public void OnEvent()
    {
        if (null == m_renderer)
        {
            m_renderer = new NativeRenderer(512, 512, m_texture.GetNativeTexturePtr());
            m_renderer.Init();
        }

        // Drawing on Surface
        if (m_bPlaying)
        {
            m_renderer.StopDrawing();
            m_bPlaying = false;
        }
        else
        {
            m_renderer.StartDrawing();
            m_bPlaying = true;
        }
    }
}
