using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JSF.Common.PlayerView
{
    public class MemberFriendController : UI.Button
    {
        public int PosID = 1;
        public OnClickFriend OnClickFriend;

        public Image FriendImage;

        private RectTransform tf;
        public override void OnClick()
        {
            if (OnClickFriend != null)
            {
                StartCoroutine(OnClickFriend.Invoke(PosID));
            }
        }

        private void Update()
        {
            /*tf = tf ?? GetComponent<RectTransform>();
            if (tf)
            {
                tf.localScale = new Vector3(tf.rect.height / tf.rect.width, 1, 1);
            }*/
        }
    }

}