using System;
using UnityEngine;

public class NativePlayer 
{
    private AndroidJavaObject androidObject;

    public NativePlayer(string url, IntPtr textureID)
    {
        AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
        androidObject = new AndroidJavaObject("com.example.myandroidlib.ExoPlayerUnity");
        androidObject.Call("build", currentActivity, url, textureID.ToInt32());
    }

    public void UpdateTexture()
    {
        androidObject.Call("");
    }

    public void Stop()
    {
        androidObject.Call("stop");
    }

    public void Pause()
    {
        androidObject.Call("pause");
    }

    public void Resume()
    {
        androidObject.Call("resume");
    }
}
