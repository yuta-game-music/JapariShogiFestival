using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JSF.SettingPage
{
    public class ShowSettingsPageButton : Common.UI.Button
    {
        public SettingPageController SettingPageController;
        public override void OnClick()
        {
            StartCoroutine(StartButtonCoroutine());
        }

        IEnumerator StartButtonCoroutine()
        {
            yield return SettingPageController.SetGameSettingsPageVisible(true);
        }
    }

}