using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class ExoPlayerUnity : MonoBehaviour
{
    #region Singleton
    static bool initialized;
    static ExoPlayerUnity instance;
    public static ExoPlayerUnity Instance
    {
        get
        {
            CreatePersistantInstance();
            return instance;
        }
    }
    static void CreatePersistantInstance()
    {
        if (initialized)
            return;

        if (!Application.isPlaying)
            return;

        initialized = true;
        var go = new GameObject("Loom");
        instance = go.AddComponent<ExoPlayerUnity>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        instance = this;
        initialized = true;
    }
    #endregion Singleton

    [DllImport("RenderingPlugin")]
    static extern System.IntPtr GetRenderEventFunc();

    [DllImport("RenderingPlugin")]
    static extern void DeleteSurfaceID(int surfaceID);

    const string VIDEOPLAYER_CLASS_NAME = "com/tutandev/exoplayerunity/ExoPlayerUnity";
    IntPtr? _VideoPlayerClass;
    IntPtr? _Activity;
    IntPtr VideoPlayerClass
    {
        get
        {
            if (!_VideoPlayerClass.HasValue)
            {
                IntPtr myVideoPlayerClass = AndroidJNI.FindClass(VIDEOPLAYER_CLASS_NAME);
                _VideoPlayerClass = AndroidJNI.NewGlobalRef(myVideoPlayerClass);
                AndroidJNI.DeleteLocalRef(myVideoPlayerClass);
            }
            return _VideoPlayerClass.GetValueOrDefault();
        }
    }
    private IntPtr Activity
    {
        get
        {
            if (_Activity.HasValue)
            {
                return _Activity.GetValueOrDefault();
            }

            try
            {
                IntPtr unityPlayerClass = AndroidJNI.FindClass("com/unity3d/player/UnityPlayer");
                IntPtr currentActivityField = AndroidJNI.GetStaticFieldID(unityPlayerClass, "currentActivity", "Landroid/app/Activity;");
                IntPtr activity = AndroidJNI.GetStaticObjectField(unityPlayerClass, currentActivityField);

                _Activity = AndroidJNI.NewGlobalRef(activity);

                AndroidJNI.DeleteLocalRef(activity);
                AndroidJNI.DeleteLocalRef(unityPlayerClass);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _Activity = System.IntPtr.Zero;
            }
            return _Activity.GetValueOrDefault();
        }
    }

    // All method pointers cached here
    // IntPtr PlayMethodID
    // ...
    readonly jvalue[] EmptyParams = new jvalue[0];


    int textureID;
    int textureToCreate;
    AndroidVideoPlayer playerController;

    public void PrepareVideo(string url, AndroidVideoPlayer player)
    {
        if (textureID > 0)
            return;

        textureID = 0;

        //call plugin function
        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Init", "(Landroid/content/Context;Ljava/lang/String;Lcom/tutandev/exoplayerunity/IUnityMessage;)V");
        jvalue[] prepareVideoParams = new jvalue[3];
        prepareVideoParams[0].l = Activity;
        prepareVideoParams[1].l = AndroidJNI.NewStringUTF(url);
        prepareVideoParams[2].l = AndroidJNIHelper.CreateJavaProxy(new AndroidUnityMessage());
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, prepareVideoParams);

        //add to textures list to create on render thread
        textureToCreate = 0;

        //set material
        playerController = player;

        //start rendering updates
        if (UpdateTextureRoutine == null)
        {
            UpdateTextureRoutine = StartCoroutine(CallPluginAtEndOfFrames());
        }
    }

    public void PlayVideo()
    {
        if(textureID == 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Play", "()V");
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, EmptyParams);
    }

    public void PauseVideo()
    {
        if (textureID == 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Pause", "()V");
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, EmptyParams);
    }

    public void StopVideo()
    {
        if (textureID == 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Stop", "()V");
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, EmptyParams);
    }

    //////// SETTERS //////// 
    public void SetLooping(bool shouldLoop)
    {
        if (textureID == 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "SetLooping", "(Z)V");
        jvalue[] setLoopingParams = new jvalue[1];
        setLoopingParams[0].z = shouldLoop;
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, setLoopingParams);
    }
    public void SetPlaybackPosition(float value)
    {
        if (textureID == 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "SetPlaybackPosition", "(D)V");
        jvalue[] SetPlaybackPositionParams = new jvalue[1];
        SetPlaybackPositionParams[0].d = value;
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, SetPlaybackPositionParams);
    }
    public void SetPlaybacSpeed(float value)
    {
        if (textureID == 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "SetPlaybackSpeed", "(F)V");
        jvalue[] SetPlaybackSpeedParams = new jvalue[1];
        SetPlaybackSpeedParams[0].f = value;
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, SetPlaybackSpeedParams);
    }

    //////// GETTERS ////////
    public int GetWidth()
    {
        if (textureID == 0)
            return 0;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetWidth", "()I");
        return AndroidJNI.CallStaticIntMethod(VideoPlayerClass, methodID, EmptyParams);
    }
    public int GetHeight()
    {
        if (textureID == 0)
            return 0;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetHeight", "()I");
        return AndroidJNI.CallStaticIntMethod(VideoPlayerClass, methodID, EmptyParams);
    }
    public bool GetIsPlaying()
    {
        if (textureID == 0)
            return false;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetIsPlaying", "()Z");
        return AndroidJNI.CallStaticBooleanMethod(VideoPlayerClass, methodID, EmptyParams);
    }
    public int GetCurrentPlaybackState()
    {
        if (textureID == 0)
            return 0;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetCurrentPlaybackState", "()I");
        return AndroidJNI.CallStaticIntMethod(VideoPlayerClass, methodID, EmptyParams);
    }
    public float GetLength()
    {
        if (textureID == 0)
            return 0;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetCurrentPlaybackState", "()J");
        return AndroidJNI.CallStaticLongMethod(VideoPlayerClass, methodID, EmptyParams);
    }
    public float GetPlaybackPosition()
    {
        if (textureID == 0)
            return 0;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetPlaybackPosition", "()D");
        return (float)AndroidJNI.CallStaticDoubleMethod(VideoPlayerClass, methodID, EmptyParams);
    }


    #region Rendering
    Coroutine UpdateTextureRoutine;
    IEnumerator CallPluginAtEndOfFrames()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            //create texture on render thread only
            if (textureToCreate == 0)
            {
                GL.IssuePluginEvent(GetRenderEventFunc(), 0);
                textureToCreate = -100;
            }

            //update texture
            GL.IssuePluginEvent(GetRenderEventFunc(), -1);
        }
    }

    public void CreateOESTexture(string videoInfo)
    {
        //get video info from message
        int externalID = int.Parse(videoInfo);

        Debug.Log("Texture ID from Unity: " + externalID);
        Texture2D oesTex = Texture2D.CreateExternalTexture(0, 0, TextureFormat.RGB24, false, true,(IntPtr)externalID);
        oesTex.Apply();

        // Set texture onto our material
        textureID = externalID;
        playerController.rend.material.mainTexture = oesTex;
    }

    #endregion
}
