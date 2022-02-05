using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using JSF.Common;
using JSF.Game.UI;

namespace JSF.Game.Board
{
    public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        protected CellDrawStatus status = CellDrawStatus.Normal;
        protected MouseStatus MouseStatus = MouseStatus.None;
        protected Image ImageRenderer;

        // 回転操作でのみ利用できるセル（そこに行くことはできない）にはtrueを入れる
        public bool RotationOnly = false;

        public Vector2Int SelfPos;
        public FriendOnBoard Friends;

        protected GameManager GameManager;

        // Start is called before the first frame update
        protected void Start()
        {
            ImageRenderer = GetComponent<Image>();
            GameManager = GetComponentInParent<GameManager>();
        }

        public void Setup(Vector2Int SelfPos)
        {
            this.SelfPos = SelfPos;
        }

        // Update is called once per frame
        protected void Update()
        {
            bool disabled = false;
            status = GameManager?.GameUI?.GetCellDrawStatus(this, out disabled) ?? CellDrawStatus.Normal;
            ImageRenderer.color = Util.GetCellColor(status, MouseStatus, disabled, RotationOnly);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            MouseStatus = MouseStatus.Clicked;

            GameManager = GameManager ?? GetComponentInParent<GameManager>();

            if (Friends)
            {
                GameManager?.GameUI?.OnClickFriendsOnBoard(Friends);
            }
            else
            {
                GameManager?.GameUI?.OnClickEmptyCell(this);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            MouseStatus = MouseStatus.Hovered;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MouseStatus = MouseStatus.None;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Friends && Friends.Possessor == GameManager.PlayerInTurn)
            {
                if (GameManager?.GameUI)
                {
                    GameManager.GameUI.OnStartDragFriendOnBoard(Friends, this);
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Friends && Friends.Possessor == GameManager.PlayerInTurn)
            {
                var cell_to = GameManager?.GameUI?.GetCellFromScreenPos(eventData.position);
                GameManager?.GameUI?.OnDragAndDropFriendOnBoard(Friends, this, cell_to);
            }
            MouseStatus = MouseStatus.None;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Friends && Friends.Possessor == GameManager.PlayerInTurn)
            {
                GameManager?.GameUI?.OnDraggingFriendOnBoard(Friends, eventData.position);
            }
        }
    }

}