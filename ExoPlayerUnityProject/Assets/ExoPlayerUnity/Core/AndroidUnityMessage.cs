using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidUnityMessage : AndroidJavaProxy
{


    public AndroidUnityMessage() : base("com/tutandev/exoplayerunity/IUnityMessage") { }

    public void CreateOESTexture(string textureID) 
    {
        ExoPlayerUnity.Instance.CreateOESTexture(textureID);
    }

    public void OnVideoPrepared() 
    {

    } 
}
