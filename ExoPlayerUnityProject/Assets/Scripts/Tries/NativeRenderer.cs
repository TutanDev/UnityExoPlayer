using System;
using UnityEngine;

public class NativeRenderer 
{
    private AndroidJavaObject androidObject;

    public NativeRenderer(int iTextureWidth, int iTextureHeight, IntPtr textureID)
    {
        androidObject = new AndroidJavaObject("com.example.myandroidlib.SurfaceTextureRenderer",
            iTextureWidth,
            iTextureHeight,
            textureID.ToInt32());
    }

    public void Init()
    {
        androidObject.Call("init");
    }

    public void StopDrawing()
    {
        androidObject.Call("stopDrawInSurface");
    }

    public void StartDrawing()
    {
        androidObject.Call("startDrawInSurface");
    }

    public void UpdateSurfaceTexture()
    {
        androidObject.Call("updateSurfaceTexture");
    }
}
