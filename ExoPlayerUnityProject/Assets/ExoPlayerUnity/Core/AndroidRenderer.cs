using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class AndroidRenderer 
{
    [DllImport("RenderingPlugin")]
    static extern IntPtr GetRenderEventFunc();
    [DllImport("RenderingPlugin")]
    static extern void DeleteSurfaceID(int surfaceID);


    int textureID;
    public int TextureID => textureID;

    public AndroidRenderer()
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
        Texture2D oesTex = Texture2D.CreateExternalTexture(0, 0, TextureFormat.RGB24, false, true, (IntPtr)externalID);
        oesTex.Apply();
        textureID = externalID;
        return oesTex;
    }
}