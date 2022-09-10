using System;
using UnityEngine;

public class ExoPlayerUnity : MonoBehaviour
{
    #region Singleton
    static ExoPlayerUnity instance;
    new static AndroidRenderer renderer;
    public static ExoPlayerUnity Instance
    {
        get
        {
            if(!instance)
            {
                CreatePersistantInstance();
            }

            return instance;
        }
    }
    static void CreatePersistantInstance()
    {
        var go = new GameObject("ExoPlayerUnity");
        instance = go.AddComponent<ExoPlayerUnity>();
        renderer = new AndroidRenderer();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        instance = this;
    }
    #endregion Singleton


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

    AndroidVideoPlayer playerController;

    public void PrepareVideo(string url, AndroidVideoPlayer player)
    {
        if (renderer.TextureID > 0)
            return;

        //call plugin function
        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Prepare", "(Landroid/content/Context;Ljava/lang/String;Lcom/tutandev/exoplayerunity/IUnityMessage;)V");
        jvalue[] prepareVideoParams = new jvalue[3];
        prepareVideoParams[0].l = Activity;
        prepareVideoParams[1].l = AndroidJNI.NewStringUTF(url);
        prepareVideoParams[2].l = AndroidJNIHelper.CreateJavaProxy(new AndroidUnityMessage());
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, prepareVideoParams);

        playerController = player;

        //start rendering updates
        if (UpdateTextureRoutine == null)
        {
            UpdateTextureRoutine = StartCoroutine(renderer.CallPluginAtEndOfFrames());
        }
    }

    public void PlayVideo()
    {
        if(renderer.TextureID == 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Play", "()V");
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, EmptyParams);
    }

    public void PauseVideo()
    {
        if (renderer.TextureID == 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Pause", "()V");
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, EmptyParams);
    }

    public void StopVideo()
    {
        if (renderer.TextureID == 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Stop", "()V");
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, EmptyParams);
    }

    //////// SETTERS //////// 
    public void SetLooping(bool shouldLoop)
    {
        if (renderer.TextureID == 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "SetLooping", "(Z)V");
        jvalue[] setLoopingParams = new jvalue[1];
        setLoopingParams[0].z = shouldLoop;
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, setLoopingParams);
    }
    public void SetPlaybackPosition(float value)
    {
        if (renderer.TextureID == 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "SetPlaybackPosition", "(D)V");
        jvalue[] SetPlaybackPositionParams = new jvalue[1];
        SetPlaybackPositionParams[0].d = value;
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, SetPlaybackPositionParams);
    }
    public void SetPlaybacSpeed(float value)
    {
        if (renderer.TextureID == 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "SetPlaybackSpeed", "(F)V");
        jvalue[] SetPlaybackSpeedParams = new jvalue[1];
        SetPlaybackSpeedParams[0].f = value;
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, SetPlaybackSpeedParams);
    }

    //////// GETTERS ////////
    public int GetWidth()
    {
        if (renderer.TextureID == 0)
            return 0;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetWidth", "()I");
        return AndroidJNI.CallStaticIntMethod(VideoPlayerClass, methodID, EmptyParams);
    }
    public int GetHeight()
    {
        if (renderer.TextureID == 0)
            return 0;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetHeight", "()I");
        return AndroidJNI.CallStaticIntMethod(VideoPlayerClass, methodID, EmptyParams);
    }
    public bool GetIsPlaying()
    {
        if (renderer.TextureID == 0)
            return false;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetIsPlaying", "()Z");
        return AndroidJNI.CallStaticBooleanMethod(VideoPlayerClass, methodID, EmptyParams);
    }
    public int GetCurrentPlaybackState()
    {
        if (renderer.TextureID == 0)
            return 0;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetCurrentPlaybackState", "()I");
        return AndroidJNI.CallStaticIntMethod(VideoPlayerClass, methodID, EmptyParams);
    }
    public float GetLength()
    {
        if (renderer.TextureID == 0)
            return 0;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetCurrentPlaybackState", "()J");
        return AndroidJNI.CallStaticLongMethod(VideoPlayerClass, methodID, EmptyParams);
    }
    public float GetPlaybackPosition()
    {
        if (renderer.TextureID == 0)
            return 0;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetPlaybackPosition", "()D");
        return (float)AndroidJNI.CallStaticDoubleMethod(VideoPlayerClass, methodID, EmptyParams);
    }


    #region Rendering
    Coroutine UpdateTextureRoutine;

    public void CreateOESTexture(int externalID)
    {
        playerController.rend.material.mainTexture = renderer.CreateOESTexture(externalID);
    }

    #endregion
}
