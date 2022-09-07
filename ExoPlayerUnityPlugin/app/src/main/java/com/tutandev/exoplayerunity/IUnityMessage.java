package com.tutandev.exoplayerunity;

public interface IUnityMessage
{
    default void CreateOESTexture(String textureID) {}
    default void OnVideoPrepared() {}
}
