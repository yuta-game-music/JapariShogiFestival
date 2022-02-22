using JSF.Common.FriendsSelector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JSF.Common.PlayerView
{
    public delegate IEnumerator OnClickFriend(int FriendsPos);
    public class PlayerViewController : MonoBehaviour
    {
        public Situation Situation;

        public PlayerInfo? PlayerInfo;
        public OnClickFriend OnClickFriend;

        public Image BackgroundImage;
        public LeaderThumbController LeaderThumbController;
        public RectTransform LeaderBackgroundTF;
        public Image LeaderThumbImage;
        public Transform MemberListParent;
        public GameObject MemberFriendPrefab;

        public GameObject PlayerNameEditor;
        public GameObject PlayerNameViewer;
        public TMPro.TMP_Text PlayerNameText;
        public TMPro.TMP_Text PlayerTypeText;

        #region LeaderRight
        public GameObject LeaderMessageObject;
        public TMPro.TMP_Text LeaderMessageText;

        public GameObject CPUDifficultySelectorObject;
        #endregion

        private bool _init = false;

        // Start is called before the first frame update
        void Start()
        {
            var cpu_difficulty_select_buttons = GetComponentsInChildren<CPUDifficultySelectButton>();
            foreach(var button in cpu_difficulty_select_buttons)
            {
                button.PlayerID = PlayerInfo?.ID ?? -1;
            }
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
                    // ���̍ۂ͂��̕\��������(TODO:�v����)
                    Destroy(gameObject);
                    _init = true;
                    return;
                }
                var playerInfo = PlayerInfo.Value;
                BackgroundImage.color = Color.Lerp(playerInfo.PlayerColor, Color.white, 0.8f);

                var CPUDifficultySelectButtons = GetComponentsInChildren<CPUDifficultySelectButton>();
                // ���[�h�ɂ���ăv���C���[�����ҏW�\���ǂ������ς��
                switch (Situation)
                {
                    case Situation.Setting:
                        // �ҏW�\�ȃV�[��
                        PlayerNameEditor.SetActive(true);
                        PlayerNameViewer.SetActive(false);
                        foreach(var CPUDifficultySelectButton in CPUDifficultySelectButtons)
                        {
                            CPUDifficultySelectButton.CanInteract = true;
                        }
                        break;
                    case Situation.Result:
                        // �ҏW�s�ȃV�[��
                        PlayerNameEditor.SetActive(false);
                        PlayerNameViewer.SetActive(true);
                        foreach (var CPUDifficultySelectButton in CPUDifficultySelectButtons)
                        {
                            CPUDifficultySelectButton.CanInteract = false;
                        }
                        break;
                }

                // �叫�t�����Y
                //LeaderBackgroundTF.sizeDelta = new Vector2(LeaderBackgroundTF.rect.height / LeaderBackgroundTF.rect.width, 1);
                LeaderThumbImage.sprite = playerInfo.Leader?.ThumbImage;
                if (LeaderThumbController)
                {
                    //LeaderThumbController.SetAllColor(Color.Lerp(playerInfo.PlayerColor, Color.white, 0.7f));
                    LeaderThumbController.OnClickFriend = OnClickFriendFunc;
                }

                // �����o�[�t�����Y����x�S������
                for(var i = MemberListParent.childCount - 1; i >= 0; i--)
                {
                    Destroy(MemberListParent.GetChild(i).gameObject);
                }

                PlayerNameText.text = playerInfo.Name;
                switch (playerInfo.PlayerType)
                {
                    case Game.Player.PlayerType.User:
                        PlayerTypeText.text = "";
                        break;
                    case Game.Player.PlayerType.CPU:
                        PlayerTypeText.text = "[CPU]";
                        break;
                    case Game.Player.PlayerType.Cellien:
                        PlayerTypeText.text = "[Cellien]";
                        break;
                }
                // �����o�[�t�����Y
                for(var i = 1; i < playerInfo.Friends.Length; i++)
                {
                    // i=1����n�܂�̂�i=0�̑叫�𔲂�����
                    var obj = Instantiate(MemberFriendPrefab);
                    obj.transform.SetParent(MemberListParent, false);
                    MemberFriendController memberFriendController = obj.GetComponent<MemberFriendController>();
                    if (memberFriendController)
                    {
                        memberFriendController.FriendImage.sprite = playerInfo.Friends[i].ThumbImage;
                        //memberFriendController.SetAllColor(Color.Lerp(playerInfo.PlayerColor, Color.white, 0.7f));
                        memberFriendController.PosID = i;
                        memberFriendController.OnClickFriend = OnClickFriendFunc;
                    }
                }


                string[] Messages = new string[0];
                switch (Situation)
                {
                    case Situation.Setting:
                        Messages = playerInfo.Leader?.SettingsMessage;
                        break;
                    case Situation.Result:
                        {
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
                        }
                        break;
                    default:
                        Debug.LogWarning("Unknown Situation: "+Situation);
                        break;
                }

                switch (playerInfo.PlayerType)
                {
                    case Game.Player.PlayerType.User:
                        {
                            PlayerTypeText.text = "";

                            LeaderMessageObject.SetActive(true);
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

                            CPUDifficultySelectorObject.SetActive(false);
                        }
                        break;
                    case Game.Player.PlayerType.CPU:
                        {
                            PlayerTypeText.text = "[CPU]";

                            LeaderMessageObject.SetActive(false);

                            CPUDifficultySelectorObject.SetActive(true);
                        }
                        break;
                    case Game.Player.PlayerType.Cellien:
                        {
                            PlayerTypeText.text = "[Cellien]";

                            LeaderMessageObject.SetActive(false);

                            CPUDifficultySelectorObject.SetActive(false);
                        }
                        break;
                }

                _init = true;
            }
        }

        private IEnumerator OnClickFriendFunc(int id)
        {
            if (OnClickFriend != null)
            {
                yield return OnClickFriend(id);
            }
            else
            {
                yield return null;
            }
        }

        public void Refresh()
        {
            _init = false;
        }
    }

    public enum Situation
    {
        Setting, Result
    }
}