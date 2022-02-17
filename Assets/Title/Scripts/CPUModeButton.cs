using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JSF.Title
{
    public class CPUModeButton : Common.UI.Button
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

            // TODO: デバッグ用なので消す
            /*
            {
                Time.timeScale = 4;
                GlobalVariable.Players[0].PlayerType = Game.Player.PlayerType.CPU;
                GlobalVariable.Players[0].CPUStrategy = new Game.CPU.CPUStrategy()
                {
                    Overall = Game.CPU.CPUStrategyOverall.BestEvaluation,
                    Defense = Game.CPU.CPUStrategyDefense.AlwaysEscape,
                    DefenseFor = Game.CPU.CPUStrategyDefenseFor.LeaderOnly,
                    Select = Game.CPU.CPUStrategySelect.NearestToLeader,
                    Move = Game.CPU.CPUStrategyMove.ApproachToLeader
                };
            }*/

            GlobalVariable.Players[1].PlayerType = Game.Player.PlayerType.CPU;
            GlobalVariable.Players[1].CPUStrategy = new Game.CPU.CPUStrategy()
            {
                Overall = Game.CPU.CPUStrategyOverall.BestEvaluation,
                Defense = Game.CPU.CPUStrategyDefense.AlwaysEscape,
                DefenseFor = Game.CPU.CPUStrategyDefenseFor.LeaderOnly,
                Select = Game.CPU.CPUStrategySelect.NearestToLeader,
                Move = Game.CPU.CPUStrategyMove.ApproachToLeader
            };
            SceneManager.LoadScene("SettingPage");
        }
    }

}