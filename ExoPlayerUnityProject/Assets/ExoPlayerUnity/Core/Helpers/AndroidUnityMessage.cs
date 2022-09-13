using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidUnityMessage : AndroidJavaProxy
{


    public AndroidUnityMessage() : base("com/tutandev/exoplayerunity/IUnityMessage") { }

    public void CreateOESTexture(int textureID) 
    {
        ExoPlayerUnity.Instance.CreateOESTexture(textureID);
    }

    public void OnPlayWhenReadyChanged(bool playWhenReady, int reason) { Debug.LogWarning($"TUTAN EXOPLAYER EVENT OnPlayWhenReadyChanged({playWhenReady}, {(ExoPlayer_PlayWhenReadyChangeReason)reason})"); }
    public void OnPlaybackStateChanged(int playbackState) { Debug.LogWarning($"TUTAN EXOPLAYER EVENT OnPlaybackStateChanged({(ExoPlayer_PlaybackState)playbackState})"); }
}
