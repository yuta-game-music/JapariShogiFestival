using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Database;
using UnityEngine.UI;

namespace JSF.Game
{
    public class FriendOnBoard : MonoBehaviour
    {
        public Friend Friend;
        public Animator Animator { get; private set; }
        public Image ImageViewer;
        
        // Start is called before the first frame update
        void Start()
        {
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
                if(ImageViewer.transform is RectTransform tf)
                {
                    tf.localScale = new Vector3(zoom, zoom, 1);
                }
                ImageViewer.sprite = Friend?.OnBoardImage;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void LookAt(Vector2 from_cell_pos, Vector2 to_cell_pos)
        {

        }
        public void MoveToCell(Vector2 cell_pos)
        {

        }
    }
}