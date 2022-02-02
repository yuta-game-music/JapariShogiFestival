using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace JSF.Game.Player
{
    public class SandstarTextController : MonoBehaviour
    {
        public Player Player;
        public TMP_Text text;

        void Update()
        {
            text.text = $"{Player.SandstarAmount}/{GlobalVariable.MaxSandstar}";
        }
    }

}