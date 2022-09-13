package com.tutandev.exoplayerunity;

import android.content.Context;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.view.Surface;

import com.google.android.exoplayer2.upstream.cache.Cache;

import java.io.File;

public class ExoPlayerUnity
{
    //Unity Class Defaults
    private static final String TAG = "ExoPlayerUnity";

    private static Context myContext ;
    static Handler handler;
    static File downloadDirectory;
    static Cache downloadCache;
    static IUnityMessage unityMessage;

    static VideoPlayer videoPlayer;

    public static void Init(Context context, String filePath, IUnityMessage _unityMessage)
    {
        if (videoPlayer == null)
        {
            myContext = context;
            videoPlayer = new VideoPlayer(context, filePath);
            Log.d(TAG, "Added video player");
            unityMessage = _unityMessage;
        }
    }

    private static Handler getHandler()
    {
        if (handler == null)
        {
            handler = new Handler(Looper.getMainLooper());
        }

        return handler;
    }

    public static void Log(String message)
    {
        Log.d(TAG, message);
    }

    public static void CreateSurface(Surface surface, int textureID)
    {
        if (videoPlayer == null) return;

        //send videoID and textureID back to unity to create external texture
        unityMessage.CreateOESTexture(textureID);

        // set up exoplayer on main thread
        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                videoPlayer.Prepare(surface);
            }
        });
    }

    public static void Play()
    {
        if (videoPlayer == null) return;

        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                videoPlayer.Play();
            }
        });
    }
    public static void Pause()
    {
        if (videoPlayer == null) return;

        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                videoPlayer.Pause();
            }
        });
    }
    public static void Stop()
    {
        if (videoPlayer == null) return;

        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                videoPlayer.Stop();
                videoPlayer = null;
            }
        });
    }


    ///// SETTERS //////
    public static void SetLooping(final boolean looping)
    {
        if (videoPlayer == null) return;

        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                videoPlayer.SetLooping(looping);
            }
        });
    }
    public static void SetPlaybackPosition(final double percent)
    {
        if (videoPlayer == null) return;

        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                videoPlayer.SetPlaybackPosition(percent);
            }
        });
    }
    public static void SetPlaybackSpeed(final float speed)
    {
        if (videoPlayer == null) return;

        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                videoPlayer.SetPlaybackSpeed(speed);
            }
        });
    }

    ///// GETTERS //////
    /*public static int GetWidth()
    {
        if (videoPlayer == null)
        {
            return 0;
        }

        return videoPlayer.width;
    }
    public static int GetHeight()
    {
        if (videoPlayer == null)
        {
            return 0;
        }

        return videoPlayer.height;
    }
    public static boolean GetIsPlaying()
    {
        if (videoPlayer == null)
        {
            return false;
        }

        return videoPlayer.isPlaying;
    }
    public static int GetCurrentPlaybackState()
    {
        if (videoPlayer == null)
        {
            return 0;
        }

        return videoPlayer.currentPlaybackState;
    }
    public static long GetLength()
    {
        if (videoPlayer == null)
        {
            return 0;
        }

        return videoPlayer.duration;
    }
    public static double GetPlaybackPosition()
    {
        if (videoPlayer == null)
        {
            return 0;
        }

        long currPosition = Math.max(0, Math.min(videoPlayer.duration, videoPlayer.lastPlaybackPosition + (long) ((System.currentTimeMillis() - videoPlayer.lastPlaybackUpdateTime) * videoPlayer.lastPlaybackSpeed)));
        double percent = (double)currPosition / videoPlayer.duration;
        return percent;
    }*/
}