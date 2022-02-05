using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace JSF.Game.UI
{
    public class StatusViewerText : MonoBehaviour
    {
        public GameManager Manager;

        public TMP_Text Text;

        // Update is called once per frame
        void Update()
        {
            if(Manager && Text)
            {
                StringBuilder sb = new StringBuilder();

                // プレイヤー情報
                foreach(var player in Manager.Players)
                {
                    if (player.PlayerType == Player.PlayerType.Cellien) { continue; }
                    sb.Append(player.PlayerName);
                    if (player.PlayerType == Player.PlayerType.CPU)
                    {
                        sb.Append("<color=#808080>[CPU]</color>");
                    }
                    if(player == Manager.PlayerInTurn)
                    {
                        sb.Append("<color=#b04040> <<ターン</color>");
                    }
                    sb.Append("\n");

                    sb.Append("サンドスター：" + string.Format("{0:  0}", player.SandstarAmount));

                    sb.Append("\n\n");
                }
                Text.text = sb.ToString();
            }
        }
    }

}