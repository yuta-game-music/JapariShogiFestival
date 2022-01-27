using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JSF.Common.PlayerView
{
    public class MemberFriendController : MonoBehaviour
    {
        public Image FriendImage;

        private RectTransform tf;

        private void Update()
        {
            tf = tf ?? GetComponent<RectTransform>();
            if (tf)
            {
                tf.localScale = new Vector3(tf.rect.height / tf.rect.width, 1, 1);
            }
        }
    }

}