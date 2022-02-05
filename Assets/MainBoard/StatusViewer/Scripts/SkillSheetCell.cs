using JSF.Database;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using JSF.Common;

namespace JSF.Game.UI
{
    public class SkillSheetCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Image BackgroundImage;
        public Image IconImage;
        private Vector2Int pos;
        private SkillSheetViewer sheetViewer;
        private CellDrawStatus CellDrawStatus;
        private MouseStatus MouseStatus;
        public void Setup(SkillSheetViewer sheetViewer, Vector2Int pos, Friend selectedFriend)
        {
            this.pos = pos;
            this.sheetViewer = sheetViewer;

            if (pos == Vector2Int.zero)
            {
                // 原点のセルにはフレンズを描画
                SetImage(selectedFriend);
            }
            // 色付け
            if (selectedFriend.CanNormalMove(pos))
            {
                CellDrawStatus = CellDrawStatus.CanMove;
            }
            else if (selectedFriend.CanRotateTo(pos))
            {
                CellDrawStatus = CellDrawStatus.CanRotate;
            }
            else if (selectedFriend.CanUseSkillWithoutContext(pos))
            {
                CellDrawStatus = CellDrawStatus.CanEffectBySkill;
            }
            else
            {
                CellDrawStatus = CellDrawStatus.Normal;
            }
        }

        private void Update()
        {
            SetColor(Util.GetCellColor(CellDrawStatus, MouseStatus, false, false));
        }

        public void SetImage(Friend friend)
        {
            if (friend != null)
            {
                IconImage.sprite = friend.OnBoardImage;
                IconImage.enabled = true;
            }
            else
            {
                IconImage.sprite = null;
                IconImage.enabled = false;
            }
        }

        public void SetColor(Color color)
        {
            BackgroundImage.color = color;
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            MouseStatus = MouseStatus.Hovered;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            MouseStatus = MouseStatus.None;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Util.PlaySE(SE.SEType.Click);
            MouseStatus = MouseStatus.Clicked;
            sheetViewer.SelectCell(pos);
        }
    }

}