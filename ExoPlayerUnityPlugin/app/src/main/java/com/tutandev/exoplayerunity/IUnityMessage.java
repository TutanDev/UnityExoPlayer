package com.tutandev.exoplayerunity;

public interface IUnityMessage
{
    default void CreateOESTexture(int textureID) {}
    default void OnPlayWhenReadyChanged(boolean playWhenReady, int reason){}
    default void OnPlaybackStateChanged(int playbackState){}
}
