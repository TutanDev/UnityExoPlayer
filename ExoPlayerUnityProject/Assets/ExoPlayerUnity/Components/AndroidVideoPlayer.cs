using UnityEngine;

public class AndroidVideoPlayer : MonoBehaviour
{
    bool isPrepared;

    public bool IsPrepared()
    {
        if (!isPrepared)
        {
            Debug.Log("Actions cannot be completed if video is not prepared.");
        }
        return isPrepared;
    }

    public void PrepareVideo(string url)
    {
        ExoPlayerUnity.Instance.PrepareVideo(url);
        isPrepared = true;
    }

    public void PlayVideo()
    {
        if (!IsPrepared())
        {
            return;
        }

        ExoPlayerUnity.Instance.CallJavaMethod(ExoPlayerMethod.PlayVideo);
    }

    public void PauseVideo()
    {
        if (!IsPrepared())
        {
            return;
        }

        ExoPlayerUnity.Instance.CallJavaMethod(ExoPlayerMethod.PauseVideo);
    }

    public void StopVideo()
    {
        if (!IsPrepared())
        {
            return;
        }

        ExoPlayerUnity.Instance.CallJavaMethod(ExoPlayerMethod.StopVideo);
        isPrepared = false;
    }

    public void SetLooping(bool shouldLoop)
    {
        if (!IsPrepared())
        {
            return;
        }

        ExoPlayerUnity.Instance.CallJavaMethod(ExoPlayerMethod.SetLooping, shouldLoop);
    }

    //pass in format 0 - 1
    public void SetPlaybackPosition(float value)
    {
        if (!IsPrepared())
        {
            return;
        }

        ExoPlayerUnity.Instance.CallJavaMethod(ExoPlayerMethod.SetLooping, value);
    }
}
