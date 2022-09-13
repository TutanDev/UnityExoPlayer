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
        instance.androidTexture = new RenderingPlugin();
        DontDestroyOnLoad(go);
    }
    #endregion Singleton

    IntPtr VideoPlayerClass;
    IntPtr Activity;
    Dictionary<ExoPlayerMethod, JNIMethod> enumToMethod;

    RenderingPlugin androidTexture;
    Coroutine UpdateTextureRoutine;

    private void Awake()
    {
        PopulatePtrs();
        PopulateMethodDictionary();
    }

    private void PopulatePtrs()
    {
        // Activity
        IntPtr unityPlayerClass = AndroidJNI.FindClass("com/unity3d/player/UnityPlayer");
        IntPtr currentActivityField = AndroidJNI.GetStaticFieldID(unityPlayerClass, "currentActivity", "Landroid/app/Activity;");
        IntPtr activity = AndroidJNI.GetStaticObjectField(unityPlayerClass, currentActivityField);
        Activity = AndroidJNI.NewGlobalRef(activity);
        AndroidJNI.DeleteLocalRef(activity);
        AndroidJNI.DeleteLocalRef(unityPlayerClass);

        // VideoPlayer
        IntPtr myVideoPlayerClass = AndroidJNI.FindClass("com/tutandev/exoplayerunity/ExoPlayerUnity");
        VideoPlayerClass = AndroidJNI.NewGlobalRef(myVideoPlayerClass);
        AndroidJNI.DeleteLocalRef(myVideoPlayerClass);

    }

    private void PopulateMethodDictionary()
    {
        enumToMethod = new Dictionary<ExoPlayerMethod, JNIMethod>();

        enumToMethod.Add(ExoPlayerMethod.PlayVideo, new JNIMethod(VideoPlayerClass, "Play", "()V"));
        enumToMethod.Add(ExoPlayerMethod.PauseVideo, new JNIMethod(VideoPlayerClass, "Pause", "()V"));
        enumToMethod.Add(ExoPlayerMethod.StopVideo, new JNIMethod(VideoPlayerClass, "Stop", "()V"));

        enumToMethod.Add(ExoPlayerMethod.SetLooping, new JNIMethod(VideoPlayerClass, "SetLooping", "(Z)V"));
        enumToMethod.Add(ExoPlayerMethod.SetPlaybackPosition, new JNIMethod(VideoPlayerClass, "SetPlaybackPosition", "(D)V"));
        enumToMethod.Add(ExoPlayerMethod.SetPlaybacSpeed, new JNIMethod(VideoPlayerClass, "SetPlaybacSpeed", "(F)V"));
    }

    public void PrepareVideo(string url)
    {
        if (androidTexture.ID > 0)
            return;

        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Init", "(Landroid/content/Context;Ljava/lang/String;Lcom/tutandev/exoplayerunity/IUnityMessage;)V");
        jvalue[] prepareVideoParams = new jvalue[3];
        prepareVideoParams[0].l = Activity;
        prepareVideoParams[1].l = AndroidJNI.NewStringUTF(url);
        prepareVideoParams[2].l = AndroidJNIHelper.CreateJavaProxy(new AndroidUnityMessage());
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, prepareVideoParams);

        //start rendering updates
        if (UpdateTextureRoutine == null)
        {
            UpdateTextureRoutine = StartCoroutine(androidTexture.CallPluginAtEndOfFrames());
        }
    }

    public void CallJavaMethod(ExoPlayerMethod method, params object[] args)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        if (!enumToMethod.TryGetValue(method, out var jniMethod))
            return;

        jniMethod.Call(args);
    }

    internal void CreateOESTexture(int externalID)
    {
        androidTexture.CreateOESTexture(externalID);
    }
    internal Texture GetVideoTexture()
    {
        return androidTexture.OESTex;
    }
}

public enum ExoPlayerMethod
{
    PlayVideo,
    PauseVideo,
    StopVideo,
    SetLooping,
    SetPlaybackPosition,
    SetPlaybacSpeed,
}
