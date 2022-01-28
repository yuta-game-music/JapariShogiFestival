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
        public RectTransform ThumbTF;
        public TMPro.TMP_Text Text;
        public RectTransform TextTF;
        public Friend Friend { set => SetFriend(value); }
        private Friend _friend;
        public FriendsSelectorController Controller;
        private RectTransform TF;

        private void Start()
        {
            TF = GetComponent<RectTransform>();
        }

        private void Update()
        {
            TF.sizeDelta = Vector2.one * TF.rect.size.y;
            
        }

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