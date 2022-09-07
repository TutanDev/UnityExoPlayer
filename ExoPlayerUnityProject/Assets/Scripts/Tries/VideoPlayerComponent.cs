using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayerComponent : MonoBehaviour
{
    [TextArea]
    [SerializeField] string videoUri;
    [SerializeField] RawImage image;

    NativePlayer player;
    private Texture2D m_texture;

    //void Update()
    //{
    //    player.UpdateSurfaceTexture();
    //}

    public void StartPlayer()
    {
        Debug.Log("TUTAN starting player");
        m_texture = new Texture2D(512, 512, TextureFormat.ARGB32, false, false);
        m_texture.filterMode = FilterMode.Point;
        player = new NativePlayer(videoUri, m_texture.GetNativeTexturePtr());
        image.texture = m_texture;
    }

    public void PausePlayer()
    {
        player.Pause();
    }

    public void ResumePlayer()
    {
        player.Resume();
    }

    //public void AssignTexture()
    //{
    //    image.texture = NativePlayerJNI.Texture;
    //}

    private void OnDisable()
    {
        player.Stop();
    }
}
