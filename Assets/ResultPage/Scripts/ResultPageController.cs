using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Common.PlayerView;
using JSF.Common.UI;

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