using JSF.Database;
using UnityEngine;

namespace JSF.Game.UI
{
    public class SkillSheetViewer : MonoBehaviour
    {
        public GameManager Manager;
        public RectTransform TF;
        public RectTransform ViewerLayoutSupporter;
        public GameObject RowObjectPrefab;
        public GameObject CellObjectPrefab;
        public Vector2 BaseCellSize = new Vector2(100,100);
        public Vector2 ViewerLayoutSize;
        public Rect CellArea = new Rect(0,0,100,100);
        public Vector2Int? SelectedCell { get; private set; }

        private Friend ShowingFriend = null;


        // Update is called once per frame
        void Update()
        {
            ViewerLayoutSize = TF.rect.size - (Vector2.one * 5f);
            Friend selected = GetSelectedFriend();
            if (selected != ShowingFriend)
            {
                ChangeFriend(selected);
                ShowingFriend = selected;
            }
            AdjustZoom();
        }

        private Friend GetSelectedFriend()
        {
            return Manager?.GameUI?.SelectedFriendOnBoard?.Friend;
        }

        private void ChangeFriend(Friend friend)
        {
            // �S�Z��������
            for(var i = ViewerLayoutSupporter.childCount - 1; i >= 0; i--)
            {
                Destroy(ViewerLayoutSupporter.GetChild(i).gameObject);
            }

            if (friend)
            {
                // �s���\�͈͂����߂�
                RectInt Movable = new RectInt(0, 0, 1, 1);
                foreach(var v in friend.NormalMoveMap)
                {
                    Movable = IncludePoint(Movable, v);
                }
                foreach (var r in friend.NormalRotationMap)
                {
                    Vector2Int v = RotationDirectionUtil.GetRotatedVector(Vector2Int.up, r);
                    Movable = IncludePoint(Movable, v);
                }
                foreach (var skill in friend.Skills)
                {
                    foreach(var v in skill.Pos)
                    {
                        Movable = IncludePoint(Movable, v);
                    }
                }

                // �s���\�͈͂ɉ����ăI�u�W�F�N�g�𐶐�
                for(var y=Movable.yMin; y <= Movable.yMax; y++)
                {
                    var RowObj = Instantiate(RowObjectPrefab);
                    RectTransform RowTF = RowObj.GetComponent<RectTransform>();
                    RowTF.SetParent(ViewerLayoutSupporter, false);
                    RowTF.localPosition = new Vector3(0, y * BaseCellSize.y, 0);
                    
                    for(var x=Movable.xMin; x <= Movable.xMax; x++)
                    {
                        var CellObj = Instantiate(CellObjectPrefab);
                        RectTransform CellTF = CellObj.GetComponent<RectTransform>();
                        CellTF.SetParent(RowTF, false);
                        CellTF.localPosition = new Vector3(x * BaseCellSize.x, 0, 0);

                        SkillSheetCell Cell = CellObj.GetComponent<SkillSheetCell>();
                        Cell.Setup(this, new Vector2Int(x, y), friend);
                    }
                }

                // �Z���͈͂̍Đݒ�
                CellArea = new Rect(new Vector2(BaseCellSize.x * Movable.xMin, BaseCellSize.y * Movable.yMin),
                    new Vector2(BaseCellSize.x * (Movable.width+1), BaseCellSize.y * (Movable.height+1)));
                CellArea.center -= BaseCellSize * 0.5f;
            }
            else
            {
                // �t�����Y�����݂��Ȃ���΋�̂܂܂ɂ���
            }
            
        }

        // �Z�������܂肫��悤�ɃT�C�Y����
        private void AdjustZoom()
        {
            // x,y�̊e������S�Ď��߂悤�Ƃ���̂ɕK�v�ȃY�[���A�E�g��(1=���{�A1�����ŏk��)
            float zoomX = ViewerLayoutSize.x / CellArea.width;
            float zoomY = ViewerLayoutSize.y / CellArea.height;
            // ���ۂ�Scale��x��y�̔䗦��ۂ��߁A�S�̂������邽�߂ɂ͏������ق����̗p����
            float zoom = Mathf.Min(zoomX, zoomY);
            ViewerLayoutSupporter.localScale = Vector3.one * zoom;

            // �Z���S�̂̏d�S��Viewer�̌��_�ɗ���悤Supporter�𒲐�
            Vector2 pos = CellArea.center * (-zoom);
            ViewerLayoutSupporter.localPosition = new Vector3(pos.x, pos.y, 0);
        }
        private RectInt IncludePoint(RectInt base_rect, Vector2Int point)
        {
            RectInt return_rect = base_rect;
            return_rect.xMin = Mathf.Min(base_rect.xMin, point.x);
            return_rect.xMax = Mathf.Max(base_rect.xMax, point.x);
            return_rect.yMin = Mathf.Min(base_rect.yMin, point.y);
            return_rect.yMax = Mathf.Max(base_rect.yMax, point.y);
            return return_rect;
        }
        public void SelectCell(Vector2Int pos)
        {
            SelectedCell = pos;
        }
    }

}