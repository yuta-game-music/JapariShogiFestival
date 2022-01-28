using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JSF.Common.PlayerView
{
    public class PlayerNameInputController : MonoBehaviour
    {
        public PlayerViewController Controller;
        public InputField InputField;

        private bool _init = false;

        private void Update()
        {
            Init();
        }

        private void Init()
        {
            if (!_init)
            {
                if (!Controller) { return; }
                if (!Controller.PlayerInfo.HasValue) { return; }
                var playerInfo = Controller.PlayerInfo.Value;

                InputField.text = playerInfo.Name;
                _init = true;
            }
        }

        public void OnValueChanged()
        {

            GlobalVariable.Players[Controller.PlayerInfo.Value.ID].Name = InputField.text;
        }
    }

}