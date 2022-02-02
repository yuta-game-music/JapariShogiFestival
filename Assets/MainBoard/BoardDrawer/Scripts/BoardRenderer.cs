using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Game.Board
{

    public class BoardRenderer : MonoBehaviour
    {
        public int W = 3;
        public int H = 3;
        public GameObject RowPrefab;
        public GameObject CellPrefab;

        public GameManager BoardManager;

        public RectTransform SizeReferenceTF;
        private RectTransform tf;

        //public Dictionary<Vector2, Cell> Cells { get; private set; } = new Dictionary<Vector2, Cell>();

        // Start is called before the first frame update
        void Start()
        {
            tf = GetComponent<RectTransform>();
        }

        public void SetBoard(int w, int h)
        {
            W = w;
            H = h;
            SetBoard();
        }

        public void SetBoard()
        {
            if (!BoardManager)
            {
                throw new System.Exception("No BoardManager found!");
            }
            // セルデータのクリーン
            BoardManager.Map.Clear();

            // 全行の消去
            for (var i = tf.childCount - 1; i >= 0; i--)
            {
                Destroy(tf.GetChild(i).gameObject);
            }

            // 行の生成
            for (int r = -1; r <= H; r++)// -1とHはRotationOnly
            {
                GameObject row = Instantiate(RowPrefab);
                row.name = "Row" + r;
                row.transform.SetParent(tf,false);

                // 全セルの消去
                for (var i = row.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(row.transform.GetChild(i).gameObject);
                }

                // セルの生成
                for (int c = -1; c <= W; c++)// -1とWはRotationOnly
                {
                    GameObject cellObject = Instantiate(CellPrefab);
                    cellObject.name = "Cell" + r + "_" + c;
                    cellObject.transform.SetParent(row.transform,false);
                    Cell cell = cellObject.GetComponent<Cell>();
                    if (!cell) { throw new System.Exception("No Cell found in CellPrefab!"); }
                    cell.SelfPos = new Vector2Int(c, r);
                    cell.RotationOnly = !(0 <= c && c < W && 0 <= r && r < H);
                    BoardManager.Map.Add(cell.SelfPos, cell);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 size = new Vector2(W+2, H+2)/Mathf.Max(W+2,H+2);
            tf.sizeDelta = size * (Mathf.Min(SizeReferenceTF.rect.width, SizeReferenceTF.rect.height)*0.98f);
        }
    }

}