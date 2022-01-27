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
                    // �Z�����A����ł͑ΐ푊���PlayerInfo��null�ɂ��Ă���
                    // ���̍ۂ͂��̕\��������
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
                        // ������
                        Messages = playerInfo.Leader?.WinnersMessage;
                    }
                    else
                    {
                        // �s�k��
                        Messages = playerInfo.Leader?.LosersMessage;
                    }
                }
                else
                {
                    // ����������
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