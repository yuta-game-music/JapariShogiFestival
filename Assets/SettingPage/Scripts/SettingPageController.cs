using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Common.PlayerView;
using JSF.Database;
using JSF.Common.FriendsSelector;
using JSF.Common.UI;

namespace JSF.SettingPage
{
    public class SettingPageController : MonoBehaviour
    {
        public WhiteOutEffectController WhiteOutEffectController;
        public PlayerViewController PlayerViewController1;
        public PlayerViewController PlayerViewController2;

        public FriendsSelectorController FriendsSelectorController;
        // Start is called before the first frame update
        void Start()
        {
            GlobalVariable.InitialSandstar = 0;
            for(var i = 0; i < 2; i++)
            {
                GlobalVariable.Players[i].Name = "プレイヤー"+(i+1);
                GlobalVariable.Players[i].Friends = new Friend[]{
                    FriendsDatabase.Get().GetFriend("サーバル"),
                    FriendsDatabase.Get().GetFriend("サーバル"),
                    FriendsDatabase.Get().GetFriend("サーバル"),
                    FriendsDatabase.Get().GetFriend("サーバル"),
                    FriendsDatabase.Get().GetFriend("サーバル"),
                    FriendsDatabase.Get().GetFriend("サーバル"),
                };
            }
            GlobalVariable.BoardH = 5;
            GlobalVariable.BoardW = 5;

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
        public IEnumerator PlayWhiteOutEffect()
        {
            yield return WhiteOutEffectController.PlayWhiteIn();
        }
    }

}