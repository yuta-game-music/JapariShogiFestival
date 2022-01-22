using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using JSF.Database;
using JSF.Database.Friends;

namespace JSF.Game.Board
{
    public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        Image ImageRenderer;
        public readonly Color COLOR_NORMAL = Color.white;
        public readonly Color COLOR_HOVERED = new Color(0.7f,0.7f,0.7f);
        public readonly Color COLOR_SELECTED = new Color(1f,0.7f,0.7f);

        public Vector2Int SelfPos;
        public FriendOnBoard Friends;

        private BoardManager BoardManager;

        // Start is called before the first frame update
        void Start()
        {
            ImageRenderer = GetComponent<Image>();
            BoardManager = GetComponentInParent<BoardManager>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ImageRenderer.color = COLOR_SELECTED;

            BoardManager = BoardManager ?? GetComponentInParent<BoardManager>();

            if (Friends)
            {
                // 本来はここでフレンズを選択
                StartCoroutine(Friends.Friend.MoveNormal(
                    SelfPos,
                    SelfPos+RotationDirectionUtil.GetRotatedVector(Vector2Int.down,Friends.Rot),
                    Friends));
            }
            else
            {
                BoardManager?.PlaceFriend(SelfPos, RotationDirection.FORWARD, FriendsDatabase.Get().GetFriend<Serval>());
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ImageRenderer.color = COLOR_HOVERED;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ImageRenderer.color = COLOR_NORMAL;
        }
    }

}