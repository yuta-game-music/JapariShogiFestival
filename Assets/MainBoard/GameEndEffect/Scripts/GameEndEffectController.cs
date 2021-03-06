using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Database;
using UnityEngine.UI;

namespace JSF.Game.Effect
{
    public class GameEndEffectController : MonoBehaviour
    {
        public Animator Animator;

        public RectTransform SizeBaseTF;
        public RectTransform ImageTF;
        public Image BackgroundImage;
        public Image Image;

        private void Start()
        {
            if(transform is RectTransform ThisTF && transform.parent is RectTransform ParentTF)
            {
                ThisTF.sizeDelta = new Vector2(ParentTF.rect.width, ThisTF.sizeDelta.y);
            }

            if(SizeBaseTF && ImageTF)
            {
                var scale = Mathf.Min(SizeBaseTF.rect.height, SizeBaseTF.rect.width) / 200f;
                ImageTF.localScale = new Vector3(scale, scale, 1);
            }
        }

        public void SetWinner(Player.Player Player)
        {
            if (Player)
            {
                if (BackgroundImage) { BackgroundImage.color = Color.Lerp(Color.white, Player.PlayerColor, 0.2f); }
                if (Image) { Image.sprite = Player.Leader.Friend.ThumbImage; }
            }
        }
        public bool AnimationEnd { get => Animator.GetCurrentAnimatorStateInfo(0).IsName("GameEndEffectEnd"); }
    }

}