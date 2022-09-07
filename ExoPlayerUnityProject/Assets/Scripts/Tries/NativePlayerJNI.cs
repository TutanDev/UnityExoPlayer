using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NativePlayerJNI
{
    public enum PlabackState
    {
        Idle = 1,
        Preparing = 2,
        Buffering = 3,
        Ready = 4,
        Ended = 5
    }

    private static IntPtr Activity
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
    private static IntPtr VideoPlayerClass
    {
        get
        {
            if (_VideoPlayerClass.HasValue)
            {
                return _VideoPlayerClass.GetValueOrDefault();
            }

            try
            {
                System.IntPtr myVideoPlayerClass = AndroidJNI.FindClass("com/example/myandroidlib/MyExoPlayer");

                if (myVideoPlayerClass != System.IntPtr.Zero)
                {
                    _VideoPlayerClass = AndroidJNI.NewGlobalRef(myVideoPlayerClass);

                    AndroidJNI.DeleteLocalRef(myVideoPlayerClass);
                }
                else
                {
                    Debug.LogError("Failed to find MyExoPlayer class");
                    _VideoPlayerClass = System.IntPtr.Zero;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to find MyExoPlayer class");
                Debug.LogException(ex);
                _VideoPlayerClass = System.IntPtr.Zero;
            }
            return _VideoPlayerClass.GetValueOrDefault();
        }
    }
    private static IntPtr SurfaceRendererClass
    {
        get
        {
            if (_SurfaceRendererClass.HasValue)
            {
                return _SurfaceRendererClass.GetValueOrDefault();
            }

            try
            {
                System.IntPtr myVideoPlayerClass = AndroidJNI.FindClass("com/example/myandroidlib/MyExoPlayer");

                if (myVideoPlayerClass != System.IntPtr.Zero)
                {
                    _SurfaceRendererClass = AndroidJNI.NewGlobalRef(myVideoPlayerClass);

                    AndroidJNI.DeleteLocalRef(myVideoPlayerClass);
                }
                else
                {
                    Debug.LogError("Failed to find MyExoPlayer class");
                    _SurfaceRendererClass = System.IntPtr.Zero;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to find MyExoPlayer class");
                Debug.LogException(ex);
                _SurfaceRendererClass = System.IntPtr.Zero;
            }
            return _SurfaceRendererClass.GetValueOrDefault();
        }
    }
    public static Texture2D Texture { get => texture; set => texture = value; }


    private static IntPtr? _Activity;
    private static IntPtr? _VideoPlayerClass;
    private static IntPtr? _SurfaceRendererClass;

    private static readonly jvalue[] EmptyParams = new jvalue[0];

    private static Texture2D texture;



    // --------------------------------------------------------------------------
    private static IntPtr playVideoMethodID;
    private static jvalue[] playVideoParams;
    public static void Build(string path)
    {
        if (playVideoMethodID == IntPtr.Zero)
        {
            playVideoMethodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "buildPlayer", "(Landroid/content/Context;Ljava/lang/String;I)V");
            playVideoParams = new jvalue[3];
        }

        IntPtr filePathJString = AndroidJNI.NewStringUTF(path);

        texture = new Texture2D(1920, 1080, TextureFormat.ARGB32, false, false);
        texture.filterMode = FilterMode.Point;
        IntPtr intptrTextureID = texture.GetNativeTexturePtr();

        playVideoParams[0].l = Activity;
        playVideoParams[1].l = filePathJString;
        playVideoParams[2].i = intptrTextureID.ToInt32();
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, playVideoMethodID, playVideoParams);

        AndroidJNI.DeleteLocalRef(filePathJString);
    }

    // --------------------------------------------------------------------------
    private static IntPtr stopMethodID;
    public static void Stop()
    {
        if (stopMethodID == IntPtr.Zero)
        {
            stopMethodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "stop", "()V");
        }

        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, stopMethodID, EmptyParams);
    }

    // --------------------------------------------------------------------------
    private static IntPtr resumeMethodID;
    public static void Play()
    {
        if (resumeMethodID == IntPtr.Zero)
        {
            resumeMethodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "resume", "()V");
        }

        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, resumeMethodID, EmptyParams);
    }

    // --------------------------------------------------------------------------
    private static IntPtr pauseMethodID;
    public static void Pause()
    {
        if (pauseMethodID == IntPtr.Zero)
        {
            pauseMethodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "pause", "()V");
        }

        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, pauseMethodID, EmptyParams);
    }

    // --------------------------------------------------------------------------
    private static IntPtr updateSurfaceTextureID;
    public static void UpdateSurfaceTexture()
    {
        if (updateSurfaceTextureID == IntPtr.Zero)
        {
            updateSurfaceTextureID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "updateSurfaceTexture", "()V");
        }

        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, updateSurfaceTextureID, EmptyParams);
    }

}
