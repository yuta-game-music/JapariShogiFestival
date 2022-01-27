using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JSF.Result
{
    public class PlayerViewController : MonoBehaviour
    {
        public PlayerInfo? PlayerInfo;

        public Image BackgroundImage;
        public Image LeaderThumbImage;

        public TMPro.TMP_Text PlayerNameText;
        public TMPro.TMP_Text LeaderMessageText;

        private bool _init = false;

        // Start is called before the first frame update
        void Start()
        {
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

                if (!PlayerInfo.HasValue)
                {
                    // セルリアン戦では対戦相手のPlayerInfoをnullにしておく
                    // その際はこの表示を消す
                    Destroy(gameObject);
                    return;
                }
                var playerInfo = PlayerInfo.Value;
                BackgroundImage.color = Color.Lerp(playerInfo.PlayerColor, Color.white, 0.8f);
                LeaderThumbImage.sprite = playerInfo.Leader?.CutInImage;
                PlayerNameText.text = playerInfo.Name;

                string[] Messages = new string[0];
                if (GlobalVariable.Winner.HasValue)
                {
                    if (GlobalVariable.Winner.Value.Equals(playerInfo))
                    {
                        // 勝利時
                        Messages = playerInfo.Leader?.WinnersMessage;
                    }
                    else
                    {
                        // 敗北時
                        Messages = playerInfo.Leader?.LosersMessage;
                    }
                }
                else
                {
                    // 引き分け時
                    Messages = playerInfo.Leader?.DrawMessage;
                }
                string Message;
                if (Messages == null || Messages.Length == 0)
                {
                    Message = "";
                }
                else
                {
                    Message = Messages[Random.Range(0, Messages.Length)];
                }
                LeaderMessageText.text = Message;

                _init = true;
            }
        }
    }

}