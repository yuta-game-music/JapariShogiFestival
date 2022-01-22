using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Database;
using UnityEngine.UI;
using JSF.Game.Board;

namespace JSF.Game
{
    public class FriendOnBoard : MonoBehaviour
    {
        public Friend Friend;
        public Animator Animator { get; private set; }
        public RectTransform ViewerTF;
        public Image ImageViewer;
        public Vector2Int Pos { get; private set; }
        public RotationDirection Rot { get; private set; }

        public BoardManager BoardManager { get; private set; }
        
        // Start is called before the first frame update
        void Start()
        {
            BoardManager = GetComponentInParent<BoardManager>();
            if (Friend == null)
            {
                Debug.LogError("No Friend specified in "+nameof(FriendOnBoard));
                enabled = false;
                return;
            }
            Animator = GetComponent<Animator>();
            if (ImageViewer)
            {
                float zoom = (transform as RectTransform)?.rect.width / 100 ?? 1;
                ViewerTF.localScale = new Vector3(zoom, zoom, 1);

                ImageViewer.sprite = Friend?.OnBoardImage;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void LookAt(Vector2Int from_cell_pos, Vector2Int to_cell_pos)
        {

        }

        // BoardManageråoóRÇ≈ÇÃÇ›åƒÇ—èoÇ∑Ç±Ç∆
        public void MoveToCell(Vector2Int cell_pos)
        {
            Pos = cell_pos;
        }
    }
}