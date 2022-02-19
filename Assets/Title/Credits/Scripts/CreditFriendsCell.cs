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

        public TMP_Text ImageAuthorText;
        public Image ImageAuthorImage;

        public TMP_Text TextAuthorText;
        public Image TextAuthorImage;
        
        public TMP_Text BehaviourAuthorText;
        public Image BehaviourAuthorImage;

        public void SetFriend(Friend f)
        {
            FriendsNameText.text = f.Name;
            FriendsImage.sprite = f.ThumbImage;

            {
                ImageAuthorText.text = "illust byÅF\n" + (string.IsNullOrEmpty(f.ImageAuthor?.Name) ? "(ìΩñºäÛñ])" : f.ImageAuthor.Name);
                if (f.ImageAuthor?.Image == null)
                {
                    Destroy(ImageAuthorImage.gameObject);
                }
                else
                {
                    ImageAuthorImage.sprite = f.ImageAuthor.Image;
                }
            }
            {
                TextAuthorText.text = "text byÅF\n" + (string.IsNullOrEmpty(f.TextAuthor?.Name) ? "(ìΩñºäÛñ])" : f.TextAuthor.Name);
                if (f.TextAuthor?.Image == null)
                {
                    Destroy(TextAuthorImage.gameObject);
                }
                else
                {
                    TextAuthorImage.sprite = f.TextAuthor.Image;
                }
            }
            {
                BehaviourAuthorText.text = "behaviour byÅF\n" + (string.IsNullOrEmpty(f.BehaviourAuthor?.Name) ? "(ìΩñºäÛñ])" : f.BehaviourAuthor.Name);
                if (f.BehaviourAuthor?.Image == null)
                {
                    Destroy(BehaviourAuthorImage.gameObject);
                }
                else
                {
                    BehaviourAuthorImage.sprite = f.BehaviourAuthor.Image;
                }
            }
        }
    }

}