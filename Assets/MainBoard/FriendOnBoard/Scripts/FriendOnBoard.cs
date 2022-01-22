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
        public static readonly float GoToLoungeTime = 1f;

        public Friend Friend;
        public Animator Animator { get; private set; }
        public RectTransform ViewerTF;
        public Image ImageViewer;
        public Vector2Int Pos { get => (Cell?.SelfPos ?? Vector2Int.zero); }
        public Cell Cell { get; private set; }
        public RotationDirection Dir { get; private set; }

        public GameManager GameManager { get; private set; }
        
        // Start is called before the first frame update
        void Start()
        {
            GameManager = GetComponentInParent<GameManager>();
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
                ViewerTF.transform.localRotation = Quaternion.Euler(0, 0, RotationDirectionUtil.GetRotationDegree(Dir));
                ImageViewer.sprite = Friend?.OnBoardImage;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void Rotate(RotationDirection diff)
        {
            Dir = RotationDirectionUtil.Merge(Dir, diff);
            if (ViewerTF)
            {
                ViewerTF.transform.localRotation = Quaternion.Euler(0, 0, RotationDirectionUtil.GetRotationDegree(Dir));
            }
        }

        // GameManager経由でのみ呼び出すこと
        public void MoveToCell(Cell Cell)
        {
            this.Cell = Cell;
        }

        // GameManager経由でのみ呼び出すこと
        public void SetDir(RotationDirection dir)
        {
            Dir = dir;
            if (ViewerTF)
            {
                ViewerTF.transform.localRotation = Quaternion.Euler(0, 0, RotationDirectionUtil.GetRotationDegree(Dir));
            }
        }

        // GameManager経由でのみ呼び出すこと
        public void GoToLounge(Player.Player player)
        {
            StartCoroutine(GoToLoungeCoroutine(player));
        }
        private IEnumerator GoToLoungeCoroutine(Player.Player player)
        {
            Vector3 StartPos = transform.position;
            Vector3 EndPos = player.LeaderPos.position;
            float StartTime = Time.time;
            transform.SetParent(GameManager.EffectObject, true);

            while (Time.time < StartTime + GoToLoungeTime)
            {
                float rate = (Time.time - StartTime) / GoToLoungeTime;
                transform.position = Vector3.Lerp(StartPos, EndPos, rate);
                transform.localRotation = Quaternion.Euler(0,0,rate*360*3);
                yield return new WaitForFixedUpdate();
            }
            transform.SetParent(player.Lounge, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}