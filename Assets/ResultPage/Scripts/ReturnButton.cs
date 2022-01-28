using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JSF.Result
{
    public class ReturnButton : Common.UI.Button
    {
        public ResultPageController ResultPageController;
        public override void OnClick()
        {
            StartCoroutine(BackTransitionCoroutine());
        }

        private IEnumerator BackTransitionCoroutine()
        {
            yield return ResultPageController.WhiteOutEffectController.PlayWhiteIn();
            SceneManager.LoadScene("SettingPage");
        }
    }

}