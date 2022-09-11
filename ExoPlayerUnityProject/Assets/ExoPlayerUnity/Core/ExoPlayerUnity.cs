using System;
using System.Collections.Generic;
using UnityEngine;

public class ExoPlayerUnity : MonoBehaviour
{
    #region Singleton
    static ExoPlayerUnity instance;
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
        DontDestroyOnLoad(go);

        instance.androidRenderer = new AndroidRenderer();
        PopulateMethodDictionary();
    }

    private static void PopulateMethodDictionary()
    {
        instance.enumToMethod = new Dictionary<ExoPlayerMethod, JNIMethod>();

        instance.enumToMethod.Add(ExoPlayerMethod.PrepareVideo, new JNIMethod(instance.VideoPlayerClass, "Prepare", "(Landroid/content/Context;Ljava/lang/String;Lcom/tutandev/exoplayerunity/IUnityMessage;)V"));
        instance.enumToMethod.Add(ExoPlayerMethod.PlayVideo,    new JNIMethod(instance.VideoPlayerClass, "Play",  "()V"));
        instance.enumToMethod.Add(ExoPlayerMethod.PauseVideo,   new JNIMethod(instance.VideoPlayerClass, "Pause", "()V"));
        instance.enumToMethod.Add(ExoPlayerMethod.StopVideo,    new JNIMethod(instance.VideoPlayerClass, "Stop",  "()V"));
        
        instance.enumToMethod.Add(ExoPlayerMethod.SetLooping,          new JNIMethod(instance.VideoPlayerClass, "SetLooping", "(Z)V"));
        instance.enumToMethod.Add(ExoPlayerMethod.SetPlaybackPosition, new JNIMethod(instance.VideoPlayerClass, "SetPlaybackPosition", "(D)V"));
        instance.enumToMethod.Add(ExoPlayerMethod.SetPlaybacSpeed,     new JNIMethod(instance.VideoPlayerClass, "SetPlaybacSpeed", "(F)V"));
    }

    private void Awake()
    {
        instance = this;
    }
    #endregion Singleton


    IntPtr? _VideoPlayerClass;
    IntPtr? _Activity;
    IntPtr VideoPlayerClass
    {
        get
        {
            if (!_VideoPlayerClass.HasValue)
            {
                IntPtr myVideoPlayerClass = AndroidJNI.FindClass("com/tutandev/exoplayerunity/ExoPlayerUnity");
                _VideoPlayerClass = AndroidJNI.NewGlobalRef(myVideoPlayerClass);
                AndroidJNI.DeleteLocalRef(myVideoPlayerClass);
            }
            return _VideoPlayerClass.GetValueOrDefault();
        }
    }
    IntPtr Activity
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

    AndroidRenderer androidRenderer;
    Dictionary<ExoPlayerMethod, JNIMethod> enumToMethod;
    Coroutine UpdateTextureRoutine;
    AndroidVideoPlayer playerController;

    public void CallJavaMethod(ExoPlayerMethod method, params object[] args)
    {
        if (androidRenderer.TextureID == 0)
            return;

        if (!enumToMethod.TryGetValue(method, out var jniMethod))
            return;

        jniMethod.Call(args);
    }

    public void PrepareVideo(string url, AndroidVideoPlayer player)
    {
        if (androidRenderer.TextureID > 0)
            return;

        if (enumToMethod.TryGetValue(ExoPlayerMethod.PrepareVideo, out var jniMethod))
        {
            jniMethod.Call(Activity, url, AndroidJNIHelper.CreateJavaProxy(new AndroidUnityMessage()));
        }

        playerController = player;

        //start rendering updates
        if (UpdateTextureRoutine == null)
        {
            UpdateTextureRoutine = StartCoroutine(androidRenderer.CallPluginAtEndOfFrames());
        }
    }

    //////// GETTERS ////////
    //public int GetWidth()
    //{
    //    if (androidRenderer.TextureID == 0)
    //        return 0;

    //    IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetWidth", "()I");
    //    return AndroidJNI.CallStaticIntMethod(VideoPlayerClass, methodID, EmptyParams);
    //}
    //public int GetHeight()
    //{
    //    if (androidRenderer.TextureID == 0)
    //        return 0;

    //    IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetHeight", "()I");
    //    return AndroidJNI.CallStaticIntMethod(VideoPlayerClass, methodID, EmptyParams);
    //}
    //public bool GetIsPlaying()
    //{
    //    if (androidRenderer.TextureID == 0)
    //        return false;

    //    IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetIsPlaying", "()Z");
    //    return AndroidJNI.CallStaticBooleanMethod(VideoPlayerClass, methodID, EmptyParams);
    //}
    //public int GetCurrentPlaybackState()
    //{
    //    if (androidRenderer.TextureID == 0)
    //        return 0;

    //    IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetCurrentPlaybackState", "()I");
    //    return AndroidJNI.CallStaticIntMethod(VideoPlayerClass, methodID, EmptyParams);
    //}
    //public float GetLength()
    //{
    //    if (androidRenderer.TextureID == 0)
    //        return 0;

    //    IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetCurrentPlaybackState", "()J");
    //    return AndroidJNI.CallStaticLongMethod(VideoPlayerClass, methodID, EmptyParams);
    //}
    //public float GetPlaybackPosition()
    //{
    //    if (androidRenderer.TextureID == 0)
    //        return 0;

    //    IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetPlaybackPosition", "()D");
    //    return (float)AndroidJNI.CallStaticDoubleMethod(VideoPlayerClass, methodID, EmptyParams);
    //}


    #region Rendering

    public void CreateOESTexture(int externalID)
    {
        playerController.rend.material.mainTexture = androidRenderer.CreateOESTexture(externalID);
    }

    #endregion
}

public enum ExoPlayerMethod
{
    PrepareVideo,
    PlayVideo,
    PauseVideo,
    StopVideo,
    SetLooping,
    SetPlaybackPosition,
    SetPlaybacSpeed,
}
