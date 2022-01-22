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

        public BoardManager BoardManager;

        private RectTransform tf;

        public Dictionary<Vector2, Cell> Cells { get; private set; } = new Dictionary<Vector2, Cell>();

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
            // セルデータのクリーン
            Cells.Clear();

            // 全行の消去
            for (var i = tf.childCount - 1; i >= 0; i--)
            {
                Destroy(tf.GetChild(i).gameObject);
            }

            // 行の生成
            for (int r = 0; r < H; r++)
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
                for (int c = 0; c < W; c++)
                {
                    GameObject cellObject = Instantiate(CellPrefab);
                    cellObject.name = "Cell" + r + "_" + c;
                    cellObject.transform.SetParent(row.transform,false);
                    Cell cell = cellObject.GetComponent<Cell>();
                    if (!cell) { throw new System.Exception("No Cell found in CellPrefab!"); }
                    cell.SelfPos = new Vector2Int(c, r);

                    Cells.Add(cell.SelfPos, cell);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}