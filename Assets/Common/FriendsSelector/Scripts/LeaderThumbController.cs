using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace JSF.Common.FriendsSelector
{
    public delegate IEnumerator OnClickFriend(int pos);
    public class LeaderThumbController : UI.Button
    {
        public OnClickFriend OnClickFriend;
        public RectTransform ImageTF;
        private RectTransform TF;

        public override void OnClick()
        {
            if (OnClickFriend != null)
            {
                StartCoroutine(OnClickFriend.Invoke(0));
            }
        }
        public new void Start()
        {
            base.Start();
            TF = GetComponent<RectTransform>();
        }
        private void Update()
        {
            ImageTF.sizeDelta = Vector2.one * Mathf.Min(TF.rect.size.x, TF.rect.size.y);

        }
    }

}