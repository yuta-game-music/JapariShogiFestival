using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JSF.Result
{
    public class ReturnButton : Common.UI.Button
    {
        public override void OnClick()
        {
            SceneManager.LoadScene("SettingPage");
        }
    }

}