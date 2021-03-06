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
                    // セルリアン戦では対戦相手のPlayerInfoをnullにしておく
                    // その際はこの表示を消す(TODO:要検討)
                    Destroy(gameObject);
                    _init = true;
                    return;
                }
                var playerInfo = PlayerInfo.Value;
                BackgroundImage.color = Color.Lerp(playerInfo.PlayerColor, Color.white, 0.8f);

                var CPUDifficultySelectButtons = GetComponentsInChildren<CPUDifficultySelectButton>();
                // モードによってプレイヤー名が編集可能かどうかが変わる
                switch (Situation)
                {
                    case Situation.Setting:
                        // 編集可能なシーン
                        PlayerNameEditor.SetActive(true);
                        PlayerNameViewer.SetActive(false);
                        foreach(var CPUDifficultySelectButton in CPUDifficultySelectButtons)
                        {
                            CPUDifficultySelectButton.CanInteract = true;
                        }
                        break;
                    case Situation.Result:
                        // 編集不可なシーン
                        PlayerNameEditor.SetActive(false);
                        PlayerNameViewer.SetActive(true);
                        foreach (var CPUDifficultySelectButton in CPUDifficultySelectButtons)
                        {
                            CPUDifficultySelectButton.CanInteract = false;
                        }
                        break;
                }

                // 大将フレンズ
                //LeaderBackgroundTF.sizeDelta = new Vector2(LeaderBackgroundTF.rect.height / LeaderBackgroundTF.rect.width, 1);
                LeaderThumbImage.sprite = playerInfo.Leader?.ThumbImage;
                if (LeaderThumbController)
                {
                    //LeaderThumbController.SetAllColor(Color.Lerp(playerInfo.PlayerColor, Color.white, 0.7f));
                    LeaderThumbController.OnClickFriend = OnClickFriendFunc;
                }

                // メンバーフレンズを一度全部消す
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
                // メンバーフレンズ
                for(var i = 1; i < playerInfo.Friends.Length; i++)
                {
                    // i=1から始まるのはi=0の大将を抜くため
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