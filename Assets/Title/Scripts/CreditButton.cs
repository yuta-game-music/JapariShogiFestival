using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JSF.Title
{
    public class CreditButton : Common.UI.Button
    {
        public TitlePageController Controller;
        public override void OnClick()
        {
            StartCoroutine(SceneTransitionCoroutine());
        }

        private IEnumerator SceneTransitionCoroutine()
        {
            yield return Controller.SetCreditPageVisible(true);
        }
    }

}