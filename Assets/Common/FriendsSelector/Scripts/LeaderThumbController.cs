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

        public override void OnClick()
        {
            if (OnClickFriend != null)
            {
                StartCoroutine(OnClickFriend.Invoke(0));
            }
        }
    }

}