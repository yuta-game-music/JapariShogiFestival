using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using JSF.Game.Tutorial;

namespace JSF.Title
{
    public class TutorialButton : Common.UI.Button
    {
        public TitlePageController Controller;
        public int TutorialID = 0;
        public override void OnClick()
        {
            StartCoroutine(SceneTransitionCoroutine());
        }

        private IEnumerator SceneTransitionCoroutine()
        {
            yield return Controller.PlayWhiteOutEffect();
            GlobalVariable.Tutorial = TutorialDatabase.tutorials?[TutorialID];
            SceneManager.LoadScene("MainBoard");
        }
    }

}