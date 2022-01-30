using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JSF.SettingPage
{
    public class BackButton : Common.UI.Button
    {
        public SettingPageController SettingPageController;
        public override void OnClick()
        {
            StartCoroutine(StartButtonCoroutine());
        }

        IEnumerator StartButtonCoroutine()
        {
            yield return SettingPageController.PlayWhiteOutEffect(0.2f);
            SceneManager.LoadScene("TitlePage");
            yield return null;
        }
    }

}