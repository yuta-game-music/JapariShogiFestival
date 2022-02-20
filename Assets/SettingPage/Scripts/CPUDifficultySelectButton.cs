using JSF.Game.CPU;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JSF.Common.PlayerView
{
    public class CPUDifficultySelectButton : UI.Button
    {
        public CPUDifficulty Difficulty;
        private int _player_id;
        public int PlayerID { get => _player_id; set => SetPlayerID(value); }

        private void SetPlayerID(int id)
        {
            _player_id = id;
            UpdateStatus();
        }
        public override void Start()
        {
            base.Start();
            UpdateStatus();
        }

        public override void OnClick()
        {
            GlobalVariable.Players[PlayerID].CPUDifficulty = Difficulty;

            var Buttons = transform.parent.GetComponentsInChildren<CPUDifficultySelectButton>();
            foreach(var button in Buttons)
            {
                if (button == this) { continue; }
                button.UpdateStatus();
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (GlobalVariable.Players[PlayerID].CPUDifficulty == Difficulty)
            {
                Image.color = HoveredColor;
            }
            else
            {
                Image.color = NormalColor;
            }
        }
    }

}