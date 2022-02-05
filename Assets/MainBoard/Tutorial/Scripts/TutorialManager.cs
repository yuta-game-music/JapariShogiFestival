using JSF.Database;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using JSF.Common;
using JSF.Game.Board;
using JSF.Game.Logger;
using UnityEngine.SceneManagement;

namespace JSF.Game.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public GameManager GameManager;
        public GameObject StencilRectPrefab;
        public RectTransform StencilsTF;
        public Animator BackgroundAnimator;
        public RectTransform MessageObjectTF;
        public TMP_Text Text;
        private FriendOnBoard[] TutorialFriendsOnBoard = new FriendOnBoard[0];
        private TutorialFriend[] TutorialFriendsPositionData = new TutorialFriend[0];
        Tutorial Tutorial;
        int Index = 0;
        bool GoToNext = false;
        bool WaitForPrevThread = false;

        private static readonly Color BackgroundColorHidden = new Color(0, 0, 0, 0);
        private static readonly Color BackgroundColor = new Color(0, 0, 0, 0.6392157f);

        bool BackgroundShown = false;

        public IEnumerator OnStartTutorial(Tutorial tutorial)
        {
            Vector2Int boardSize = tutorial.InitialBoardStatus.Size;
            // Boardの初期化を待つ
            yield return new WaitUntil(() => GameManager.Map.Keys.Count >= (boardSize.x+2)*(boardSize.y+2));
            this.Tutorial = tutorial;

            var friends = new List<FriendOnBoard>();
            for (var bci = 0; bci < tutorial.InitialBoardStatus.Cells.Length; bci++)
            {
                var f = tutorial.InitialBoardStatus.Cells[bci];
                // 既に他のフレンズが置かれていたらそれはリーダーではない
                bool isLeader = !friends.Any((fob) => (fob.Possessor?.PlayerInfo?.ID ?? -1) == f.PlayerID);
                GameManager.PlaceFriend(
                    f.Pos,
                    f.Dir,
                    FriendsDatabase.Get().GetFriend(f.FriendName),
                    GameManager.Players[2 * f.PlayerID],
                    isLeader
                    );
                friends.Add(GameManager.Map[f.Pos].Friends);
                yield return new WaitForSeconds(0.5f);
            }
            this.TutorialFriendsOnBoard = friends.ToArray();
            for(var pi = 0; pi < GameManager.Players.Length; pi++)
            {
                GameManager.Players[pi].SandstarAmount = tutorial.InitialSandstarAmount;
            }
            Index = 0;
            yield return GameManager.OnTurnStart();
        }
        public IEnumerator OnCPUTurnStart()
        {
            yield return OnTurnStart();
        }

        public IEnumerator OnUserTurnStart()
        {
            yield return OnTurnStart();
        }

        public IEnumerator OnTurnStart()
        {
            yield return new WaitWhile(() => WaitForPrevThread);
            WaitForPrevThread = true;
            for (var i=Index; i<Tutorial.Nodes.Length;i++)
            {
                Index = i;
                var node = Tutorial.Nodes[i];
                Debug.Log("Play Tutorial id=" + i +"("+node.Text+")");
                if (string.IsNullOrEmpty(node.Text))
                {
                    // メッセージ表示なし
                    yield return HideCharacter();
                    if ((node.FocusedCell==null || node.FocusedCell.Length==0)
                        && string.IsNullOrEmpty(node.FocusedGameObjectName))
                    {
                        // 表示一切なし
                        yield return HideBackground();
                        if (node.AutoMoveStrategy != null || node.TutorialCondition != null)
                        {
                            // 自動移動・手動移動がある →実行してから次へ
                            yield return PlayActionPart(node);
                        }
                        else
                        {
                            // 自動移動・手動移動もない →ターンを経過させて次へ
                            yield return GameManager.OnTurnPass();
                        }
                    }
                    else
                    {
                        // 注目表示あり 

                        yield return ShowBackground(speed:3);
                        yield return PlaceStencils(node);

                        if (node.AutoMoveStrategy != null || node.TutorialCondition != null)
                        {
                            // 自動移動・手動移動がある →実行して次へ
                            yield return PlayActionPart(node);
                        }
                        else
                        {
                            // 自動移動・手動移動がない →1秒間だけ表示して次へ
                            yield return new WaitForSeconds(1f);
                        }

                        yield return RemoveStencils();
                    }
                }
                else
                {
                    // メッセージ表示あり
                    Text.text = node.Text;
                    yield return ShowBackground();

                    Vector2 anchor = node.TextPosAnchor ?? Vector2.one * 0.5f;
                    MessageObjectTF.anchorMin = MessageObjectTF.anchorMax = anchor;
                    yield return ShowCharacter();

                    yield return PlaceStencils(node);

                    if (node.TutorialCondition != null || node.AutoMoveStrategy != null)
                    {
                        yield return PlayActionPart(node);
                        Index++;
                        WaitForPrevThread = false;
                        yield break;
                    }
                    else
                    {
                        GoToNext = false;
                        yield return new WaitUntil(() => GoToNext);
                    }

                    GoToNext = false;

                    yield return RemoveStencils();
                }

            }
            // 終了したらタイトル画面に戻す
            yield return GameManager.GameUI.whiteOutEffectController.PlayWhiteIn();
            SceneManager.LoadScene("TitlePage");

        }
        private IEnumerator PlayActionPart(TutorialNode node)
        {
            int now_player_id = GameManager.PlayerInTurnID;
            var snap = new Snapshot(GameManager);
            do
            {
                StoreTutorialFriendsPositionData();
                yield return new WaitForSeconds(0.8f);
                yield return HideBackground(speed: 2f);
                RemoveStencilsImmediately();
                yield return GameManager.GameUI.OnPlayerTurnStart(GameManager.PlayerInTurn);
                if (node.AutoMoveStrategy != null)
                {
                    yield return node.AutoMoveStrategy(GameManager, this);
                    StartCoroutine(GameManager.OnTurnPass());
                }
                else
                {
                    yield return new WaitUntil(() =>
                    {
                        return now_player_id != GameManager.PlayerInTurnID;
                    });
                }
                yield return ShowBackground(speed: 2f);
                if (node.TutorialCondition == null) { break; } // 条件なし
                if (node.TutorialCondition(GameManager, this))
                {
                    break;
                }
                else
                {
                    snap.Restore(GameManager);
                    RestoreTutorialFriendsFromPositionData();
                }
            } while (true);
        }
        // 暗転
        private IEnumerator ShowBackground(float speed=1f)
        {
            if (!BackgroundShown)
            {
                int layer_id = BackgroundAnimator.GetLayerIndex("ShowHide");
                BackgroundAnimator.SetBool("Show", true);
                BackgroundAnimator.SetFloat("Speed", speed);
                yield return new WaitUntil(() => BackgroundAnimator.GetCurrentAnimatorStateInfo(layer_id).IsName("Shown"));
                BackgroundShown = true;
            }
        }
        // 明転
        private IEnumerator HideBackground(float speed=1f)
        {
            if (BackgroundShown)
            {
                int layer_id = BackgroundAnimator.GetLayerIndex("ShowHide");
                BackgroundAnimator.SetBool("Show", false);
                BackgroundAnimator.SetFloat("Speed", speed);
                yield return new WaitUntil(() => BackgroundAnimator.GetCurrentAnimatorStateInfo(layer_id).IsName("Hidden"));
                BackgroundShown = false;
            }
            RemoveStencilsImmediately();
        }
        private IEnumerator ShowCharacter()
        {
            MessageObjectTF.gameObject.SetActive(true);
            yield return null;
        }
        private IEnumerator HideCharacter()
        {
            MessageObjectTF.gameObject.SetActive(false);
            yield return null;
        }
        private IEnumerator ShowMessage(TutorialNode Node)
        {

            yield return null;
        }

        private IEnumerator PlaceStencils(TutorialNode node)
        {
            if (node.FocusedCell != null)
            {
                foreach (var cell in node.FocusedCell)
                {
                    GameObject StencilRectObject = Instantiate(StencilRectPrefab);
                    RectTransform tf = StencilRectObject.GetComponent<RectTransform>();
                    tf.SetParent(StencilsTF, false);
                    yield return PlaceStencilRect(tf, cell);
                }
            }
            if (!string.IsNullOrEmpty(node.FocusedGameObjectName))
            {
                GameObject StencilRectObject = Instantiate(StencilRectPrefab);
                RectTransform tf = StencilRectObject.GetComponent<RectTransform>();
                tf.SetParent(StencilsTF, false);
                yield return PlaceStencilRect(tf, node.FocusedGameObjectName);
            }
        }
        private IEnumerator RemoveStencils()
        {
            for(var i = StencilsTF.childCount - 1; i >= 0; i--)
            {
                Transform StencilTF = StencilsTF.GetChild(i);
                GameObject StencilGameObject = StencilTF.gameObject;
                Animator animator = StencilTF.GetComponent<Animator>();

                int layer_id = animator.GetLayerIndex("ShowHide");
                animator.SetBool("Show", false);
                yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(layer_id).IsName("Hidden"));
                Destroy(StencilTF.gameObject);
            }
        }
        private void RemoveStencilsImmediately()
        {
            for (var i = StencilsTF.childCount - 1; i >= 0; i--)
            {
                Transform StencilTF = StencilsTF.GetChild(i);
                Destroy(StencilTF.gameObject);
            }
        }
        private IEnumerator PlaceStencilRect(RectTransform tf, Vector2Int cell_pos)
        {
            if(GameManager.Map.TryGetValue(cell_pos, out Cell cell))
            {
                RectTransform ref_tf = cell.GetComponent<RectTransform>();
                if (ref_tf == null)
                {
                    Debug.LogError("Invalid cell rectTransform!");
                }
                else
                {
                    Rect r = ref_tf.rect;
                    Vector3 tmppos = ref_tf.position;
                    r.center = new Vector2(tmppos.x, tmppos.y);
                    yield return PlaceStencilRect(tf, r);
                }
            }
            else
            {
                Debug.LogWarning("No cell at "+cell_pos+"!");
            }
        }

        private IEnumerator PlaceStencilRect(RectTransform tf, string gameobject_name)
        {
            GameObject obj;
            if (gameobject_name.Contains("/"))
            {
                string[] pathnames = gameobject_name.Split("/".ToCharArray());

                Transform now = null;
                for (var i = 0; i < pathnames.Length; i++)
                {
                    if (i == 0)
                    {
                        GameObject[] objects = SceneManager.GetActiveScene().GetRootGameObjects();
                        foreach(var _obj in objects)
                        {
                            if (_obj.name == pathnames[0])
                            {
                                now = _obj.transform;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (now == null) { obj = null; break; }
                        for(var ci = 0; ci < now.childCount; ci++)
                        {
                            if (now.GetChild(ci).name == pathnames[i])
                            {
                                now = now.GetChild(ci);
                                break;
                            }
                        }
                    }
                    Debug.Log(now?.name);
                }
                obj = now?.gameObject;
            }
            else
            {
                obj = GameObject.Find(gameobject_name);
            }
            if (obj)
            {
                RectTransform ref_tf = obj.GetComponent<RectTransform>();
                if (ref_tf == null)
                {
                    Debug.LogError("Invalid object rectTransform!");
                }
                else
                {
                    Rect r = ref_tf.rect;
                    Vector3 tmppos = ref_tf.position;
                    r.center = new Vector2(tmppos.x, tmppos.y);
                    yield return PlaceStencilRect(tf, r);
                }
            }
            else
            {
                Debug.LogWarning("No such object " + gameobject_name+ "!");
            }
        }

        private IEnumerator PlaceStencilRect(RectTransform tf, Rect area)
        {
            tf.anchorMin = tf.anchorMax = tf.pivot = Vector2.one * 0.5f;
            tf.position = new Vector3(area.center.x,area.center.y,0);
            tf.sizeDelta = area.size;
            Animator animator = tf.GetComponent<Animator>();
            int layer_id = animator.GetLayerIndex("ShowHide");
            animator.SetBool("Show", true);
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(layer_id).IsName("Shown"));
        }

        public FriendOnBoard GetFriendById(int id)
        {
            if(id<0 || TutorialFriendsOnBoard.Length <= id) { return null; }
            return TutorialFriendsOnBoard[id];
        }
        public void OnClickNext()
        {
            if (!GoToNext)
            {
                Util.PlaySE(SE.SEType.Click);
                GoToNext = true;
            }
        }

        public void StoreTutorialFriendsPositionData()
        {
            TutorialFriendsPositionData = new TutorialFriend[TutorialFriendsOnBoard.Length];
            for (var i = 0; i < TutorialFriendsOnBoard.Length; i++)
            {
                FriendOnBoard fob = TutorialFriendsOnBoard[i];
                TutorialFriendsPositionData[i] = new TutorialFriend()
                {
                    Pos = fob.Pos,
                    PlayerId = fob.Possessor?.PlayerInfo?.ID ?? -1,
                    LoungeId = fob.LoungeID ?? -1
                };
            }
        }

        public void RestoreTutorialFriendsFromPositionData()
        {
            for(var i = 0; i < TutorialFriendsOnBoard.Length; i++)
            {
                var rec = TutorialFriendsPositionData[i];
                if (rec.Pos.HasValue)
                {
                    // 盤上のフレンズ
                    TutorialFriendsOnBoard[i] = GameManager.Map[rec.Pos.Value].Friends;
                }
                else
                {
                    // 駒置き場のフレンズ
                    TutorialFriendsOnBoard[i] = GameManager.Players[2*rec.PlayerId].GetFriendsOnLoungeById(rec.LoungeId);
                }

                if (TutorialFriendsOnBoard[i] != null)
                {
                    Debug.Log("Restored [" + i + "]", TutorialFriendsOnBoard[i]);
                }
                else
                {

                    Debug.LogError("Cannot restore [" + i + "] @("+rec.Pos+",["+rec.PlayerId+"]["+rec.LoungeId+"])");
                }
            }
        }
    }

    struct TutorialFriend
    {
        public Vector2Int? Pos; // 駒置き場のフレンズの場合はnullにして↓2つを設定
        public int PlayerId; // どちらのプレイヤーの駒置き場にいるか
        public int LoungeId; // 駒置き場のどこにいるか
    }
}