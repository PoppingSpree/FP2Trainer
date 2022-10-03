using System;
using System.Collections;
using System.Threading;
using UnityEngine;

// This is heavily based on the script given here: https://blog.unity.com/technology/precise-framerates-in-unity
public class ForceRenderRate : MonoBehaviour
{
    public float Rate = 60.0f;
    float currentFrameTime;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 9999;
        currentFrameTime = Time.realtimeSinceStartup;
        StartCoroutine("WaitForNextFrame");
    }

    void OnLevelWasLoaded(int level)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 9999;
    }

    IEnumerator WaitForNextFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            currentFrameTime += 1.0f / Rate;
            var t = Time.realtimeSinceStartup;
            var sleepTime = currentFrameTime - t - 0.01f;
            if (sleepTime > 0)
                Thread.Sleep((int)(sleepTime * 1000));
            while (t < currentFrameTime)
                t = Time.realtimeSinceStartup;
        }
    }
}