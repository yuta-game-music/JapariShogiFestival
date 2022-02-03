using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JSF.Database;

namespace JSF.Title
{
    public class CreditFriendsCell : MonoBehaviour
    {
        public TMP_Text FriendsNameText;
        public Image FriendsImage;

        public TMP_Text AuthorText;
        public Image AuthorImage;

        public void SetFriend(Friend f)
        {
            FriendsNameText.text = f.Name;
            FriendsImage.sprite = f.ThumbImage;

            AuthorText.text = "çÏé“ÅF"+(string.IsNullOrEmpty(f.AuthorName) ? "(ìΩñºäÛñ])" : f.AuthorName);
            if (f.AuthorImage == null)
            {
                Destroy(AuthorImage.gameObject);
            }
            else
            {
                AuthorImage.sprite = f.AuthorImage;
            }
        }
    }

}