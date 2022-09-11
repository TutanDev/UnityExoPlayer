using UnityEngine;

public class AndroidVideoPlayer : MonoBehaviour
{
    [HideInInspector]
    public Renderer rend;
    public Material material;

    bool isPrepared;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void PrepareVideo(string url)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            ExoPlayerUnity.Instance.PrepareVideo(url,  this);
        }

        isPrepared = true;
        rend.material = material;
    }

    public void PlayVideo()
    {
        if (!IsPrepared())
        {
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            ExoPlayerUnity.Instance.CallJavaMethod(ExoPlayerMethod.PlayVideo);
        }
    }

    public void PauseVideo()
    {

        if (!IsPrepared())
        {
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            ExoPlayerUnity.Instance.CallJavaMethod(ExoPlayerMethod.PauseVideo);
        }
    }

    public void StopVideo()
    {

        if (!IsPrepared())
        {
            return;
        }

        isPrepared = false;

        if (Application.platform == RuntimePlatform.Android)
        {
            ExoPlayerUnity.Instance.CallJavaMethod(ExoPlayerMethod.StopVideo);
        }
    }

    //public int GetWidth()
    //{

    //    if (!IsPrepared())
    //    {
    //        return 0;
    //    }

    //    return Application.platform == RuntimePlatform.Android ? ExoPlayerUnity.Instance.GetWidth() : 0;
    //}

    //public int GetHeight()
    //{

    //    if (!IsPrepared())
    //    {
    //        return 0;
    //    }

    //    return Application.platform == RuntimePlatform.Android ? ExoPlayerUnity.Instance.GetHeight() : 0;
    //}

    public void SetLooping(bool shouldLoop)
    {
        if (!IsPrepared())
        {
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            ExoPlayerUnity.Instance.CallJavaMethod(ExoPlayerMethod.SetLooping, shouldLoop);
        }
    }

    //pass in format 0 - 1
    public void SetPlaybackPosition(float value)
    {
        if (!IsPrepared())
        {
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            ExoPlayerUnity.Instance.CallJavaMethod(ExoPlayerMethod.SetLooping, value);
        }
    }

    //returns format 0-1
    //public float GetCurrentPlaybackPercent()
    //{
    //    if (!IsPrepared())
    //    {
    //        return 0;
    //    }

    //    return Application.platform == RuntimePlatform.Android ? ExoPlayerUnity.Instance.GetPlaybackPosition() : (float)0;
    //}

    //public bool IsPlaying()
    //{
    //    if (!IsPrepared())
    //    {
    //        return false;
    //    }

    //    return Application.platform == RuntimePlatform.Android ? ExoPlayerUnity.Instance.GetIsPlaying() : false;
    //}

    public bool IsPrepared()
    {
        if (!isPrepared)
        {
            Debug.Log("Actions cannot be completed if video is not prepared.");
        }
        return isPrepared;
    }
}
