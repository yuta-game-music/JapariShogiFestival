using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Common
{
    [CreateAssetMenu(fileName = "AppVersion.asset", menuName = "JSF/Common/AppVersion")]
    public class AppVersion : ScriptableObject
    {
        public string VersionName = "Ver.0.1.0";
        public int VersionNumber = 0001000; // x.yyy.zzz

    }

}