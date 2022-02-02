using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Common.PlayerView;
using JSF.Database;
using JSF.Common.FriendsSelector;
using JSF.Common.UI;
using JSF.Game;
using JSF.Game.Player;

namespace JSF.SettingPage
{
    public class SettingPageController : MonoBehaviour
    {
        public WhiteOutEffectController WhiteOutEffectController;
        public PlayerViewController PlayerViewController1;
        public PlayerViewController PlayerViewController2;

        public FriendsSelectorController FriendsSelectorController;
        public Animator GameSettingsPageAnimation;
        // Start is called before the first frame update
        void Start()
        {
            CheckPlayerData();

            PlayerViewController1.PlayerInfo = GlobalVariable.Players[0];
            PlayerViewController2.PlayerInfo = GlobalVariable.Players[1];

            PlayerViewController1.OnClickFriend = (friendPos)=>OnClickFriend(0,friendPos);
            PlayerViewController2.OnClickFriend = (friendPos)=>OnClickFriend(1,friendPos);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private IEnumerator OnClickFriend(int playerID, int friendsPos)
        {
            yield return FriendsSelectorController.ShowSelector(playerID, friendsPos);
            (playerID == 0 ? PlayerViewController1 : PlayerViewController2).Refresh();
        }
        public IEnumerator PlayWhiteOutEffect(float? whiteOutEffectLengthOverride = null)
        {
            yield return WhiteOutEffectController.PlayWhiteIn(whiteOutEffectLengthOverride);
        }
        public IEnumerator SetGameSettingsPageVisible(bool visible)
        {
            var layer_id = GameSettingsPageAnimation.GetLayerIndex("ShowHide");
            GameSettingsPageAnimation.SetBool("Show", visible);

            yield return new WaitUntil(() => GameSettingsPageAnimation.GetCurrentAnimatorStateInfo(layer_id).IsName(visible ? "Shown" : "Hidden"));
        }
        private void CheckPlayerData()
        {
            if (GlobalVariable.Players == null)
            {
                GlobalVariable.Players = new PlayerInfo[2];
            }
            for (int i = 0; i < 2; i++)
            {
                if (GlobalVariable.Players[i].Friends == null)
                {
                    GlobalVariable.Players[i].Name = "ƒvƒŒƒCƒ„[" + i;
                    GlobalVariable.Players[i].ID = i;
                    GlobalVariable.Players[i].PlayerColor = i == 0 ? Color.red : Color.blue;
                    GlobalVariable.Players[i].PlayerType = PlayerType.User;
                    GlobalVariable.Players[i].Direction = i == 0 ? RotationDirection.FORWARD : RotationDirection.BACKWARD;
                    GlobalVariable.Players[i].Friends = new Friend[]
                    {
                        FriendsDatabase.Get().Friends[0],
                        FriendsDatabase.Get().Friends[0],
                        FriendsDatabase.Get().Friends[0],
                    };
                }
            }
        }
    }

}