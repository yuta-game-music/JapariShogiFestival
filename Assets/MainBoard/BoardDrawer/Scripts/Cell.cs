using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using JSF.Database;
using JSF.Database.Friends;
using JSF.Game.UI;

namespace JSF.Game.Board
{
    public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        CellDrawStatus status = CellDrawStatus.Normal;
        Image ImageRenderer;

        // 回転操作でのみ利用できるセル（そこに行くことはできない）にはtrueを入れる
        public bool RotationOnly = false;

        // マウス状態による色変化
        public static readonly Color COLOR_NORMAL = Color.white;
        public static readonly Color COLOR_HOVERED = new Color(0.7f,0.7f,0.7f);
        public static readonly Color COLOR_SELECTED = new Color(1f,0.7f,0.7f);
        private Color ColorByMouseOver = COLOR_NORMAL;

        // UIによる動的な色変化
        public static readonly Color COLOR_CANNOT_USE = new Color(0.2f, 0.2f, 0.2f);
        public static readonly Color COLOR_CAN_MOVE = new Color(0.4f, 1f, 0.55f);
        public static readonly Color COLOR_CAN_ROTATE = new Color(0.4f, 0.4f, 1f);
        public static readonly Color COLOR_CAN_EFFECT_BY_SKILL = new Color(1f, 0.7f, 0.6f);
        private Color ColorByStatus = COLOR_NORMAL;

        public Vector2Int SelfPos;
        public FriendOnBoard Friends;

        private GameManager GameManager;

        // Start is called before the first frame update
        void Start()
        {
            ImageRenderer = GetComponent<Image>();
            GameManager = GetComponentInParent<GameManager>();
        }

        // Update is called once per frame
        void Update()
        {
            bool disabled = false;
            status = GameManager?.GameUI?.GetCellDrawStatus(this, out disabled) ?? CellDrawStatus.Normal;
            switch (status)
            {
                case CellDrawStatus.Normal:
                    ColorByStatus = COLOR_NORMAL;
                    break;
                case CellDrawStatus.CannotUse:
                    ColorByStatus = COLOR_CANNOT_USE;
                    break;
                case CellDrawStatus.CanMove:
                    ColorByStatus = COLOR_CAN_MOVE;
                    break;
                case CellDrawStatus.CanRotate:
                    ColorByStatus = COLOR_CAN_ROTATE;
                    break;
                case CellDrawStatus.CanEffectBySkill:
                    ColorByStatus = COLOR_CAN_EFFECT_BY_SKILL;
                    break;
                case CellDrawStatus.Selected:
                    ColorByStatus = COLOR_SELECTED;
                    break;
            }
            Color tmp = Color.Lerp(ColorByStatus, ColorByMouseOver, 0.5f);
            if (disabled)
            {
                tmp = Color.Lerp(tmp, Color.black, 0.2f);
            }
            ImageRenderer.color = tmp;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ColorByMouseOver = COLOR_SELECTED;

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
            ColorByMouseOver = COLOR_HOVERED;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ColorByMouseOver = COLOR_NORMAL;
        }
    }

}