package com.tutandev.exoplayerunity;

import android.content.Context;
import android.net.Uri;
import android.os.Handler;
import android.os.Looper;
import android.os.storage.OnObbStateChangeListener;
import android.os.storage.StorageManager;
import android.util.Log;
import android.view.Surface;

import com.google.android.exoplayer2.C;
import com.google.android.exoplayer2.C.ContentType;
import com.google.android.exoplayer2.Format;
import com.google.android.exoplayer2.MediaItem;
import com.google.android.exoplayer2.PlaybackParameters;
import com.google.android.exoplayer2.Player;
import com.google.android.exoplayer2.SimpleExoPlayer;
import com.google.android.exoplayer2.Timeline;
import com.google.android.exoplayer2.audio.AudioSink;
import com.google.android.exoplayer2.drm.FrameworkMediaDrm;
import com.google.android.exoplayer2.source.MediaSource;
import com.google.android.exoplayer2.source.ProgressiveMediaSource;
import com.google.android.exoplayer2.source.dash.DashMediaSource;
import com.google.android.exoplayer2.source.dash.DefaultDashChunkSource;
import com.google.android.exoplayer2.source.hls.HlsMediaSource;
import com.google.android.exoplayer2.source.smoothstreaming.DefaultSsChunkSource;
import com.google.android.exoplayer2.source.smoothstreaming.SsMediaSource;
import com.google.android.exoplayer2.trackselection.AdaptiveTrackSelection;
import com.google.android.exoplayer2.trackselection.DefaultTrackSelector;
import com.google.android.exoplayer2.trackselection.TrackSelection;
import com.google.android.exoplayer2.upstream.BandwidthMeter;
import com.google.android.exoplayer2.upstream.DataSource;
import com.google.android.exoplayer2.upstream.DefaultBandwidthMeter;
import com.google.android.exoplayer2.upstream.DefaultDataSourceFactory;
import com.google.android.exoplayer2.upstream.DefaultHttpDataSourceFactory;
import com.google.android.exoplayer2.upstream.FileDataSourceFactory;
import com.google.android.exoplayer2.upstream.HttpDataSource;
import com.google.android.exoplayer2.upstream.cache.Cache;
import com.google.android.exoplayer2.upstream.cache.CacheDataSource;
import com.google.android.exoplayer2.upstream.cache.CacheDataSourceFactory;
import com.google.android.exoplayer2.upstream.cache.NoOpCacheEvictor;
import com.google.android.exoplayer2.upstream.cache.SimpleCache;
import com.google.android.exoplayer2.util.Util;
import com.twobigears.audio360.AudioEngine;
import com.twobigears.audio360.SpatDecoderQueue;

import java.io.File;

public class ExoPlayerUnity
{
    //Unity Class Defaults
    private static final String TAG = "NativeVideoPlayer";

    private static Context myContext ;

    static final float SAMPLE_RATE = 48000.f;
    static final int BUFFER_SIZE = 1024;
    static final int QUEUE_SIZE_IN_SAMPLES = 40960;

    static Handler handler;
    static File downloadDirectory;
    static Cache downloadCache;

    static IUnityMessage unityMessage;

    private static class VideoPlayer {
        private SimpleExoPlayer exoPlayer;
        private AudioEngine engine;
        private SpatDecoderQueue spat;
        private AudioSink audio360Sink;
        private FrameworkMediaDrm mediaDrm;
        private Surface mSurface;
        private String filePath;
        private boolean readyToPlay;
        private volatile boolean isPlaying;
        private volatile int currentPlaybackState;
        private volatile int stereoMode = -1;
        private volatile int width;
        private volatile int height;
        private volatile long duration;
        private volatile long lastPlaybackPosition;
        private volatile long lastPlaybackUpdateTime;
        private volatile float lastPlaybackSpeed;
    }

    static VideoPlayer currVideoPlayer;

    private static void updatePlaybackState(VideoPlayer currPlayer)
    {
        currPlayer.duration = currPlayer.exoPlayer.getDuration();
        currPlayer.lastPlaybackPosition = currPlayer.exoPlayer.getCurrentPosition();
        currPlayer.lastPlaybackSpeed = currPlayer.isPlaying ? currPlayer.exoPlayer.getPlaybackParameters().speed : 0;
        currPlayer.lastPlaybackUpdateTime = System.currentTimeMillis();
        Format format = currPlayer.exoPlayer.getVideoFormat();
        if (format != null)
        {
            currPlayer.stereoMode = format.stereoMode;
            currPlayer.width = format.width;
            currPlayer.height = format.height;
        } else
        {
            currPlayer.stereoMode = -1;
            currPlayer.width = 0;
            currPlayer.height = 0;
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

    private static File getDownloadDirectory(Context context)
    {
        if (downloadDirectory == null)
        {
            downloadDirectory = context.getExternalFilesDir(null);
            if (downloadDirectory == null)
            {
                downloadDirectory = context.getFilesDir();
            }
        }
        return downloadDirectory;
    }

    private static synchronized Cache getDownloadCache(Context context)
    {
        if (downloadCache == null)
        {
            File downloadContentDirectory = new File(getDownloadDirectory(context), "downloads");
            downloadCache = new SimpleCache(downloadContentDirectory, new NoOpCacheEvictor());
        }
        return downloadCache;
    }

    private static CacheDataSourceFactory buildReadOnlyCacheDataSource(
            DefaultDataSourceFactory upstreamFactory, Cache cache)
    {
        return new CacheDataSourceFactory(
                cache,
                upstreamFactory,
                new FileDataSourceFactory(),
                /* cacheWriteDataSinkFactory= */ null,
                CacheDataSource.FLAG_IGNORE_CACHE_ON_ERROR,
                /* eventListener= */ null);
    }

    /**
     * Returns a {@link DataSource.Factory}.
     */
    public static DataSource.Factory buildDataSourceFactory(Context context, VideoPlayer currPlayer)
    {
        DefaultDataSourceFactory upstreamFactory = new DefaultDataSourceFactory(context, null, buildHttpDataSourceFactory(context));
        return buildReadOnlyCacheDataSource(upstreamFactory, getDownloadCache(context));
    }

    /**
     * Returns a {@link HttpDataSource.Factory}.
     */
    public static HttpDataSource.Factory buildHttpDataSourceFactory(Context context)
    {
        return new DefaultHttpDataSourceFactory(Util.getUserAgent(context, "NativeVideoPlayer"));
    }

    @SuppressWarnings("unchecked")
    private static MediaSource buildMediaSource(Context context, Uri uri, /*@Nullable*/ String overrideExtension, DataSource.Factory dataSourceFactory)
    {
        @ContentType int type = Util.inferContentType(uri, overrideExtension);
        switch (type)
        {
            case C.TYPE_DASH:
                return new DashMediaSource.Factory(new DefaultDashChunkSource.Factory(dataSourceFactory), dataSourceFactory).createMediaSource(MediaItem.fromUri(uri));
            case C.TYPE_SS:
                return new SsMediaSource.Factory(new DefaultSsChunkSource.Factory(dataSourceFactory), dataSourceFactory).createMediaSource(MediaItem.fromUri(uri));
            case C.TYPE_HLS:
                return new HlsMediaSource.Factory(dataSourceFactory).createMediaSource(MediaItem.fromUri(uri));
            case C.TYPE_OTHER:
                return new ProgressiveMediaSource.Factory(dataSourceFactory).createMediaSource(MediaItem.fromUri(uri));
            default:
            {
                throw new IllegalStateException("Unsupported type: " + type);
            }
        }
    }

    public static void Log(String message) {
        Log.d(TAG, message);
    }

    public static void CreateSurface(Surface surface, String textureID)
    {
        if (currVideoPlayer == null)
        {
            return;
        }

        //send videoID and textureID back to unity to create external texture
        unityMessage.CreateOESTexture(textureID);
        currVideoPlayer.mSurface = surface;

        // set up exoplayer on main thread
        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                // 1. AudioEngine
                if (currVideoPlayer.engine == null)
                {
                    currVideoPlayer.engine = AudioEngine.create(SAMPLE_RATE, BUFFER_SIZE, QUEUE_SIZE_IN_SAMPLES, myContext);
                    currVideoPlayer.spat = currVideoPlayer.engine.createSpatDecoderQueue();
                    currVideoPlayer.engine.start();
                }

                // 2. VideoSource type
                DataSource.Factory dataSourceFactory = buildDataSourceFactory(myContext, currVideoPlayer);
                Uri uri = ParseFilePath();
                MediaSource videoSource = buildMediaSource(myContext, uri, null, dataSourceFactory);
                Log.d(TAG, "Requested play of " + currVideoPlayer.filePath + " uri: " + uri.toString());

                // 3. Exoplayer
                if (currVideoPlayer.exoPlayer != null)
                {
                    currVideoPlayer.exoPlayer.release();
                }

                currVideoPlayer.exoPlayer = new SimpleExoPlayer.Builder(myContext).build(); // Pasarle trackSelector
                AddPlayerListener();
                currVideoPlayer.exoPlayer.setVideoSurface(currVideoPlayer.mSurface);
                currVideoPlayer.exoPlayer.setMediaSource(videoSource);
                currVideoPlayer.exoPlayer.prepare();

                currVideoPlayer.exoPlayer.setRepeatMode(Player.REPEAT_MODE_ONE);
                currVideoPlayer.exoPlayer.setPlayWhenReady(false);
            }
        });
    }

    private static void AddPlayerListener()
    {
        currVideoPlayer.exoPlayer.addListener(new Player.Listener() {
            @Override
            public void onPlayWhenReadyChanged(boolean playWhenReady, int reason)
            {
                currVideoPlayer.isPlaying = playWhenReady && (currVideoPlayer.currentPlaybackState == Player.STATE_READY
                                                            || currVideoPlayer.currentPlaybackState == Player.STATE_BUFFERING);
                updatePlaybackState(currVideoPlayer);
            }

            @Override
            public void onPlaybackStateChanged(int playbackState)
            {
                //call on prepared from unity
                if (!currVideoPlayer.readyToPlay && playbackState == Player.STATE_READY)
                {
                    currVideoPlayer.readyToPlay = true;
                    unityMessage.OnVideoPrepared();
                }

                currVideoPlayer.currentPlaybackState = playbackState;
                updatePlaybackState(currVideoPlayer);
            }

            @Override
            public void onPlaybackParametersChanged(PlaybackParameters params)
            {
                updatePlaybackState(currVideoPlayer);
            }

            @Override
            public void onPositionDiscontinuity(Player.PositionInfo oldPosition, Player.PositionInfo newPosition, int reason)
            {
                updatePlaybackState(currVideoPlayer);
            }

        });
    }

    private static Uri ParseFilePath()
    {
        Uri uri = Uri.parse(currVideoPlayer.filePath);

        if (currVideoPlayer.filePath.startsWith("jar:file:"))
        {
            if (currVideoPlayer.filePath.contains(".apk"))
            { // APK
                uri = new Uri.Builder().scheme("asset").path(currVideoPlayer.filePath.substring(currVideoPlayer.filePath.indexOf("/assets/") + "/assets/".length())).build();
            }
            else if (currVideoPlayer.filePath.contains(".obb"))
            { // OBB
                String obbPath = currVideoPlayer.filePath.substring(11, currVideoPlayer.filePath.indexOf(".obb") + 4);

                StorageManager sm = (StorageManager) myContext.getSystemService(Context.STORAGE_SERVICE);
                if (!sm.isObbMounted(obbPath))
                {
                    sm.mountObb(obbPath, null, new OnObbStateChangeListener() {
                        @Override
                        public void onObbStateChange(String path, int state) {
                            super.onObbStateChange(path, state);
                        }
                    });
                }

                uri = new Uri.Builder().scheme("file").path(sm.getMountedObbPath(obbPath) + currVideoPlayer.filePath.substring(currVideoPlayer.filePath.indexOf(".obb") + 5)).build();
            }
        }

        return uri;
    }


    public static void Prepare(Context context, String filePath, IUnityMessage _unityMessage)
    {
        if (currVideoPlayer == null)
        {
            myContext = context;
            VideoPlayer videoPlayer = new VideoPlayer();
            videoPlayer.filePath = filePath;
            currVideoPlayer = videoPlayer;
            Log.d(TAG, "Added video player");
            unityMessage = _unityMessage;
        }
    }

    public static void Play()
    {
        if (currVideoPlayer == null)
        {
            return;
        }

        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                if (currVideoPlayer.exoPlayer != null)
                {
                    currVideoPlayer.exoPlayer.setPlayWhenReady(true);
                }
            }
        });
    }
    public static void Pause()
    {
        if (currVideoPlayer == null)
        {
            return;
        }

        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                if (currVideoPlayer.exoPlayer != null)
                {
                    currVideoPlayer.exoPlayer.setPlayWhenReady(false);
                }
            }
        });
    }
    public static void Stop()
    {
        if (currVideoPlayer == null)
        {
            return;
        }

        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                if (currVideoPlayer.exoPlayer != null)
                {
                    currVideoPlayer.exoPlayer.stop();
                    currVideoPlayer.exoPlayer.release();
                    currVideoPlayer.exoPlayer = null;
                }
                if (currVideoPlayer.mediaDrm != null)
                {
                    currVideoPlayer.mediaDrm.release();
                    currVideoPlayer.mediaDrm = null;
                }
                if (currVideoPlayer.engine != null)
                {
                    currVideoPlayer.engine.destroySpatDecoderQueue(currVideoPlayer.spat);
                    currVideoPlayer.engine.delete();
                    currVideoPlayer.spat = null;
                    currVideoPlayer.engine = null;
                }

                currVideoPlayer = null;
            }
        });
    }


    ///// SETTERS //////
    public static void SetLooping(final boolean looping)
    {
        if (currVideoPlayer == null)
        {
            return;
        }

        getHandler().post(new Runnable()
        {
            @Override
            public void run() {
                if (currVideoPlayer.exoPlayer != null)
                {
                    if (looping)
                    {
                        currVideoPlayer.exoPlayer.setRepeatMode(Player.REPEAT_MODE_ONE);
                    }
                    else
                    {
                        currVideoPlayer.exoPlayer.setRepeatMode(Player.REPEAT_MODE_OFF);
                    }
                }
            }
        });
    }
    public static void SetPlaybackPosition(final double percent)
    {
        if (currVideoPlayer == null)
        {
            return;
        }

        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                if (currVideoPlayer.exoPlayer != null)
                {
                    Timeline timeline = currVideoPlayer.exoPlayer.getCurrentTimeline();
                    if (timeline != null)
                    {

                        long timeInMilliseconds = (long)(currVideoPlayer.duration * percent);

                        int windowIndex = timeline.getFirstWindowIndex(false);
                        long windowPositionUs = timeInMilliseconds * 1000L;
                        Timeline.Window tmpWindow = new Timeline.Window();
                        for (int i = timeline.getFirstWindowIndex(false);
                             i < timeline.getLastWindowIndex(false); i++)
                        {
                            timeline.getWindow(i, tmpWindow);

                            if (tmpWindow.durationUs > windowPositionUs)
                            {
                                break;
                            }

                            windowIndex++;
                            windowPositionUs -= tmpWindow.durationUs;
                        }

                        currVideoPlayer.exoPlayer.seekTo(windowIndex, windowPositionUs / 1000L);
                    }
                }
            }
        });
    }
    public static void SetPlaybackSpeed(final float speed)
    {
        if (currVideoPlayer == null)
        {
            return;
        }

        getHandler().post(new Runnable()
        {
            @Override
            public void run()
            {
                if (currVideoPlayer.exoPlayer != null)
                {
                    PlaybackParameters param = new PlaybackParameters(speed);
                    currVideoPlayer.exoPlayer.setPlaybackParameters(param);
                }
            }
        });
    }

    ///// GETTERS //////
    public static int GetWidth()
    {
        if (currVideoPlayer == null)
        {
            return 0;
        }

        return currVideoPlayer.width;
    }
    public static int GetHeight()
    {
        if (currVideoPlayer == null)
        {
            return 0;
        }

        return currVideoPlayer.height;
    }
    public static boolean GetIsPlaying()
    {
        if (currVideoPlayer == null)
        {
            return false;
        }

        return currVideoPlayer.isPlaying;
    }
    public static int GetCurrentPlaybackState()
    {
        if (currVideoPlayer == null)
        {
            return 0;
        }

        return currVideoPlayer.currentPlaybackState;
    }
    public static long GetLength()
    {
        if (currVideoPlayer == null)
        {
            return 0;
        }

        return currVideoPlayer.duration;
    }
    public static double GetPlaybackPosition()
    {
        if (currVideoPlayer == null)
        {
            return 0;
        }

        long currPosition = Math.max(0, Math.min(currVideoPlayer.duration, currVideoPlayer.lastPlaybackPosition + (long) ((System.currentTimeMillis() - currVideoPlayer.lastPlaybackUpdateTime) * currVideoPlayer.lastPlaybackSpeed)));
        double percent = (double)currPosition / currVideoPlayer.duration;
        return percent;
    }
}