using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Common.PlayerView;
using JSF.Database;
using JSF.Database.Friends;

namespace JSF.SettingPage
{
    public class SettingPageController : MonoBehaviour
    {
        public PlayerViewController PlayerViewController1;
        public PlayerViewController PlayerViewController2;
        // Start is called before the first frame update
        void Start()
        {
            GlobalVariable.InitialSandstar = 0;
            for(var i = 0; i < 2; i++)
            {
                GlobalVariable.Players[i].Name = "ƒvƒŒƒCƒ„["+(i+1);
                GlobalVariable.Players[i].Friends = new Friend[]{
                    FriendsDatabase.Get().GetFriend<Serval>(),
                    FriendsDatabase.Get().GetFriend<Serval>(),
                    FriendsDatabase.Get().GetFriend<Serval>(),
                };
            }
            GlobalVariable.BoardH = 7;
            GlobalVariable.BoardW = 7;

            PlayerViewController1.PlayerInfo = GlobalVariable.Players[0];
            PlayerViewController2.PlayerInfo = GlobalVariable.Players[1];
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}