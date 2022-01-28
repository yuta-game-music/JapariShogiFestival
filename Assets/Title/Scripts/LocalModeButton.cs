using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JSF.Title
{
    public class LocalModeButton : Common.UI.Button
    {
        public TitlePageController Controller;
        public override void OnClick()
        {
            StartCoroutine(SceneTransitionCoroutine());
        }

        private IEnumerator SceneTransitionCoroutine()
        {
            yield return Controller.PlayWhiteOutEffect();
            SceneManager.LoadScene("SettingPage");
        }
    }

}