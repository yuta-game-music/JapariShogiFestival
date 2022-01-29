using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JSF.Game.Player
{
    public class Player : MonoBehaviour
    {
        public string PlayerName = "Player";
        public PlayerType PlayerType = PlayerType.User;
        public PlayerInfo? PlayerInfo;

        public RotationDirection Direction = RotationDirection.FORWARD;

        public FriendOnBoard Leader { get => get_leader(); set => set_leader(value); }
        [SerializeField]
        private FriendOnBoard _leader;

        private FriendOnBoard get_leader() { return _leader; }
        private void set_leader(FriendOnBoard leader) {
            _leader = leader;
            if (LeaderImage)
            {
                LeaderImage.sprite = _leader.Friend.CutInImage;
            }
        }

        public Color PlayerColor;

        public int SandstarAmount = 0;

        public Transform LeaderPos;
        public Image LeaderBackgroundImage;
        public Image LeaderImage;
        public RectTransform LeaderImageTF;
        public Transform Lounge;
        public TMPro.TMP_Text NameText;

        private void Start()
        {
            if (LeaderBackgroundImage)
            {
                LeaderBackgroundImage.color = Color.Lerp(PlayerColor,Color.white,0.8f);
            }
            if (LeaderImageTF)
            {
                LeaderImageTF.localScale = new Vector3(LeaderImageTF.rect.height / 100, 1, 1);
            }
        }

        public void Init()
        {
            if (NameText)
            {
                NameText.text = PlayerName;
            }

            if(PlayerType == PlayerType.Cellien)
            {
                NameText.enabled = false;
            }
            else
            {
                NameText.enabled = true;
            }
        }
    }

    public enum PlayerType
    {
        User, CPU, Cellien
    }
}