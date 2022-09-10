package com.tutandev.exoplayerunity;

public interface IUnityMessage
{
    default void CreateOESTexture(int textureID) {}
    default void OnVideoPrepared() {}
}
