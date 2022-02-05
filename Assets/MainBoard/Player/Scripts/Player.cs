using JSF.Common;
using JSF.Game.Board;
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
                if (_leader != null)
                {
                    LeaderImage.enabled = true;
                    LeaderImage.sprite = _leader.Friend.ThumbImage;
                }
                else
                {
                    LeaderImage.enabled = false;
                }
            }
        }

        public Color PlayerColor;

        public int SandstarAmount = 0;

        public Transform LeaderPos;
        public Image LeaderBackgroundImage;
        public Image LeaderImage;
        public RectTransform LeaderImageTF;
        public Transform Lounge;
        public HorizontalListSizeAdjuster LoungeSizeAdjuster;
        public TMPro.TMP_Text NameText;
        public SandstarGaugeController SandstarGaugeController;

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
        private void Update()
        {
            LoungeSizeAdjuster.HeightDelta = Cell.SizeReferenceCellTF?.rect.height ?? 50f;
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
                LeaderBackgroundImage.enabled = false;
                LeaderImage.enabled = false;
            }
            else
            {
                NameText.enabled = true;
            }
        }

        public void PlaySandstarGaugeAnimation(SandstarGaugeStatus status, int amount, float length)
        {
            SandstarGaugeController?.PlayGaugeAnimation(status, amount, length);
        }

        public FriendOnBoard GetFriendsOnLoungeById(int id)
        {
            return Lounge?.GetChild(id)?.GetChild(0)?.GetComponent<FriendOnBoard>();
        }
    }

    public enum PlayerType
    {
        User, CPU, Cellien, TutorialUser, TutorialCPU
    }
}