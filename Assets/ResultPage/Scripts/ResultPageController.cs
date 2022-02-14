using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Common.PlayerView;
using JSF.Common.UI;
using System;

namespace JSF.Result
{
    public class ResultPageController : MonoBehaviour
    {
        public WhiteOutEffectController WhiteOutEffectController;

        public PlayerViewController PlayerViewController1;
        public PlayerViewController PlayerViewController2;

        private bool _init = false;
        // Start is called before the first frame update
        void Start()
        {
            Init();
#if UNITY_EDITOR
            // TODO: デバッグ用のため削除
            
            for(var i = 0; i < 2; i++)
            {
                var strategy = GlobalVariable.Players[i].CPUStrategy = new Game.CPU.CPUStrategy()
                {
                    Overall = (Game.CPU.CPUStrategyOverall)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Game.CPU.CPUStrategyOverall)).Length),
                    Select = Game.CPU.CPUStrategySelect.NearestToLeader,//(Game.CPU.CPUStrategySelect)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Game.CPU.CPUStrategySelect)).Length),
                    Move = Game.CPU.CPUStrategyMove.ApproachToLeader//(Game.CPU.CPUStrategyMove)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Game.CPU.CPUStrategyMove)).Length),
                };
                Debug.Log("Player"+(i+1)+" "+strategy.Overall+" "+strategy.Select+" "+strategy.Move);
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainBoard");
            
#endif
        }

        // Update is called once per frame
        void Update()
        {
            Init();
        }

        private void Init()
        {
            if (!_init)
            {
                PlayerViewController1.PlayerInfo = GlobalVariable.Players[0];
                PlayerViewController2.PlayerInfo = GlobalVariable.Players[1];
                _init = true;
            }
        }
    }

}