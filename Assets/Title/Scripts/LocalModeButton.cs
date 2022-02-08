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
            GlobalVariable.Tutorial = null;
            GlobalVariable.Players[1].PlayerType = Game.Player.PlayerType.User;
            SceneManager.LoadScene("SettingPage");
        }
    }

}