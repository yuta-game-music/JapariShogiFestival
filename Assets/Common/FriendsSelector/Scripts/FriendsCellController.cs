using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Database;
using UnityEngine.UI;

namespace JSF.Common.FriendsSelector
{
    public class FriendsCellController : UI.Button
    {
        public Image ThumbImage;
        public TMPro.TMP_Text Text;
        public Friend Friend { set => SetFriend(value); }
        private Friend _friend;
        public FriendsSelectorController Controller;

        private void SetFriend(Friend friend)
        {
            ThumbImage.sprite = friend.CutInImage;
            Text.text = friend.Name;
            _friend = friend;
        }

        public override void OnClick()
        {
            Controller.OnSelectFriend(_friend);
        }
    }

}