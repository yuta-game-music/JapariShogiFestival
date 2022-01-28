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
                    // ���̍ۂ͂��̕\��������(TODO:�v����)
                    Destroy(gameObject);
                    _init = true;
                    return;
                }
                var playerInfo = PlayerInfo.Value;
                BackgroundImage.color = Color.Lerp(playerInfo.PlayerColor, Color.white, 0.8f);

                // ���[�h�ɂ���ăv���C���[�����ҏW�\���ǂ������ς��
                switch (Situation)
                {
                    case Situation.Setting:
                        // �v���C���[�����ҏW�\�ȃV�[��
                        PlayerNameEditor.SetActive(true);
                        PlayerNameViewer.SetActive(false);
                        break;
                    case Situation.Result:
                        // �v���C���[�����ҏW�s�ȃV�[��
                        PlayerNameEditor.SetActive(false);
                        PlayerNameViewer.SetActive(true);
                        break;
                }

                // �叫�t�����Y
                //LeaderBackgroundTF.sizeDelta = new Vector2(LeaderBackgroundTF.rect.height / LeaderBackgroundTF.rect.width, 1);
                LeaderThumbImage.sprite = playerInfo.Leader?.CutInImage;
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
                // �����o�[�t�����Y
                for(var i = 1; i < playerInfo.Friends.Length; i++)
                {
                    // i=1����n�܂�̂�i=0�̑叫�𔲂�����
                    var obj = Instantiate(MemberFriendPrefab);
                    obj.transform.SetParent(MemberListParent, false);
                    MemberFriendController memberFriendController = obj.GetComponent<MemberFriendController>();
                    if (memberFriendController)
                    {
                        memberFriendController.FriendImage.sprite = playerInfo.Friends[i].CutInImage;
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