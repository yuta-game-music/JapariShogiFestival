using JSF.Game.UI;
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

        public static Color GetCellColor(CellDrawStatus CellDrawStatus, MouseStatus MouseStatus, bool Disabled, bool RotationOnly)
        {
            Color ColorByStatus;
            switch (CellDrawStatus)
            {
                case CellDrawStatus.Normal:
                    ColorByStatus = Const.CellColorNormal;
                    break;
                case CellDrawStatus.CannotUse:
                    ColorByStatus = Const.CellColorCannotUse;
                    break;
                case CellDrawStatus.CanMove:
                    ColorByStatus = Const.CellColorCanMove;
                    break;
                case CellDrawStatus.CanRotate:
                    ColorByStatus = Const.CellColorCanRotate;
                    break;
                case CellDrawStatus.CanEffectBySkill:
                    ColorByStatus = Const.CellColorCanEffectBySkill;
                    break;
                case CellDrawStatus.Selected:
                    ColorByStatus = Const.CellColorSelected;
                    break;
                default:
                    Debug.LogWarning("Unknown CellDrawStatus: " + CellDrawStatus);
                    ColorByStatus = Const.CellColorNormal;
                    break;
            }

            Color ColorByMouse;
            switch (MouseStatus)
            {
                case MouseStatus.None:
                    ColorByMouse = Const.CellColorNormal;
                    break;
                case MouseStatus.Hovered:
                    ColorByMouse = Const.CellColorHovered;
                    break;
                case MouseStatus.Clicked:
                    ColorByMouse = Const.CellColorSelected;
                    break;
                default:
                    Debug.LogWarning("Unknown MouseStatus: " + MouseStatus);
                    ColorByMouse = Const.CellColorNormal;
                    break;
            }
            Color tmp = Color.Lerp(ColorByStatus, ColorByMouse, 0.5f);
            if (Disabled)
            {
                tmp = Color.Lerp(tmp, Color.black, 0.2f);
            }
            if (RotationOnly)
            {
                tmp = Color.Lerp(tmp, Color.black, 0.4f);
            }
            return tmp;
        }
    }

}