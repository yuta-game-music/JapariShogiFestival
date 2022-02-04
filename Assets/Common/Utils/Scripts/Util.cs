using JSF.SE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace JSF.Common
{
    public static class Util
    {
        public static AudioSource BGMSource { get; private set; }
        public static void SetBGMSource(AudioSource BGMSource)
        {
            if (Util.BGMSource != null)
            {
                Debug.LogWarning("Double " + nameof(BGMSource) + " Designation!");
            }
            Util.BGMSource = BGMSource;
        }
        public static SEController SESource { get; private set; }
        public static void SetSESource(SEController SESource)
        {
            if (Util.SESource != null)
            {
                Debug.LogWarning("Double "+nameof(SESource)+" Designation!");
            }
            Util.SESource = SESource;
        }

        public static void PlaySE(AudioClip clip)
        {
            SESource?.PlaySE(clip);
        }

        public static void PlaySE(SEType SEType)
        {
            SESource?.PlaySE(SEType);
        }

        public static string GetSavedFileDirectoryPath()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var getFilesDir = currentActivity.Call<AndroidJavaObject>("getFilesDir"))
        {
            return getFilesDir.Call<string>("getCanonicalPath");
        }
#else
            return Application.persistentDataPath;
#endif
        }
    }

}