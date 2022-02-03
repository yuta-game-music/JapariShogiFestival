using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Database;
using UnityEngine.UI;

namespace JSF.Game.Effect
{
    public class CutInEffectController : MonoBehaviour
    {
        public Animator Animator;

        public RectTransform SizeBaseTF;
        public RectTransform ImageTF;
        public Image Image;

        private void Start()
        {
            if(SizeBaseTF && ImageTF)
            {
                var scale = Mathf.Min(SizeBaseTF.rect.height, SizeBaseTF.rect.width) / 200f;
                ImageTF.localScale = new Vector3(scale, scale, 1);
            }
        }

        public void SetFriend(Friend friend)
        {
            if (Image) { Image.sprite = friend.ThumbImage; }
        }
        public bool AnimationEnd { get => Animator.GetCurrentAnimatorStateInfo(0).IsName("CutInEnd"); }
    }

}