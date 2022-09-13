using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class RenderingPlugin 
{
    [DllImport("RenderingPlugin")]
    static extern IntPtr GetRenderEventFunc();
    [DllImport("RenderingPlugin")]
    static extern void DeleteSurfaceID(int surfaceID);


    int textureID;
    Texture2D oesTex;

    public int ID => textureID;
    public Texture2D OESTex => oesTex;


    public RenderingPlugin()
    {
        textureID = 0;
    }

    public IEnumerator CallPluginAtEndOfFrames()
    {
        var waitFrame = new WaitForEndOfFrame();
        while (true)
        {
            yield return waitFrame;
            GL.IssuePluginEvent(GetRenderEventFunc(), textureID);
        }
    }

    public Texture2D CreateOESTexture(int externalID)
    {
        Debug.Log("Texture ID from Unity: " + externalID);
        oesTex = Texture2D.CreateExternalTexture(0, 0, TextureFormat.RGB24, false, true, (IntPtr)externalID);
        textureID = externalID;
        return oesTex;
    }

    public void DeleteSurface()
    {
        DeleteSurfaceID(textureID);
    }
}