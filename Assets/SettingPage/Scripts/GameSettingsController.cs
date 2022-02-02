using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace JSF.SettingPage
{
    public class GameSettingsController : MonoBehaviour, IPointerClickHandler
    {
        public SettingPageController SettingPageController;
        public TMP_Text BoardWText;
        public TMP_Text BoardHText;
        public TMP_Text BoardRealmHeightText;
        public TMP_Text InitialSandstarText;
        public void OnClickBoardWidthChange(int value)
        {
            GlobalVariable.BoardW = Mathf.Clamp(GlobalVariable.BoardW + value, GlobalVariable.MIN_BOARD_W, GlobalVariable.MAX_BOARD_W);
            UpdateText();
        }
        public void OnClickBoardHeightChange(int value)
        {
            GlobalVariable.BoardH = Mathf.Clamp(GlobalVariable.BoardH + value, GlobalVariable.MIN_BOARD_H, GlobalVariable.MAX_BOARD_H);
            GlobalVariable.BoardRealmHeight = Mathf.Clamp(GlobalVariable.BoardRealmHeight, 1, GlobalVariable.BoardH / 2);
            UpdateText();
        }
        public void OnClickBoardRealmHeightChange(int value)
        {
            GlobalVariable.BoardRealmHeight = Mathf.Clamp(GlobalVariable.BoardRealmHeight + value, 1, GlobalVariable.BoardH/2);
            UpdateText();
        }
        public void OnClickInitialSandstarChange(int value)
        {
            GlobalVariable.InitialSandstar = Mathf.Clamp(GlobalVariable.InitialSandstar + value, 0, 15);
            UpdateText();
        }

        private void Start()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            if (BoardWText) { BoardWText.text = GlobalVariable.BoardW.ToString(); }
            if (BoardHText) { BoardHText.text = GlobalVariable.BoardH.ToString(); }
            if (BoardRealmHeightText) { BoardRealmHeightText.text = GlobalVariable.BoardRealmHeight.ToString(); }
            if (InitialSandstarText) { InitialSandstarText.text = GlobalVariable.InitialSandstar.ToString(); }
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            StartCoroutine(SettingPageController.SetGameSettingsPageVisible(false));
        }
    }

}