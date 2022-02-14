using JSF.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Game.UI;
using JSF.Game.Board;
using UnityEngine.SceneManagement;
using JSF.Common;
using JSF.Game.Logger;
using JSF.Game.Tutorial;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JSF.Game
{
    public class GameManager : MonoBehaviour
    {
        public GameObject FriendOnBoardPrefab;

        public Player.Player[] Players;
        
        public int PlayerInTurnID = 0;
        public Player.Player PlayerInTurn { get => Players[PlayerInTurnID]; }

        public Dictionary<Vector2Int, Cell> Map { get; private set; } = new Dictionary<Vector2Int, Cell>();
        public Dictionary<Vector2Int, FriendOnBoard> Friends { get; private set; } = new Dictionary<Vector2Int, FriendOnBoard>();

        public GameUI GameUI;
        public BoardRenderer BoardRenderer;

        public Transform EffectObject;

        public TutorialManager TutorialManager;

        private bool init = false;
        private bool StartNextTurn = false;

        private HashSet<Cell> _affected_cells = new HashSet<Cell>();
        public Cell[] AffectedCells { get => _affected_cells.ToArray(); }
        private bool _clean_affected_cells = true;

        public void Start()
        {
            // (デバッグ用)プレイヤー情報が入っていなければデフォルトを読み込む
#if UNITY_EDITOR
            CheckPlayerData();
#endif

            // 盤面設定
            if (GlobalVariable.Tutorial == null)
            {
                BoardRenderer.W = GlobalVariable.BoardW;
                BoardRenderer.H = GlobalVariable.BoardH;
            }
            else
            {
                Tutorial.Tutorial tutorial = GlobalVariable.Tutorial.Value;
                BoardRenderer.W = tutorial.InitialBoardStatus.Size.x;
                BoardRenderer.H = tutorial.InitialBoardStatus.Size.y;
            }

            for (var i = 0; i < GlobalVariable.Players.Length; i++)
            {
                // 2倍しているのは間にセルリアンを挟むため
                Players[2 * i].PlayerInfo = GlobalVariable.Players[i];
                Players[2 * i].PlayerName = GlobalVariable.Players[i].Name;
                Players[2 * i].PlayerType = GlobalVariable.Players[i].PlayerType;
                Players[2 * i].Direction = GlobalVariable.Players[i].Direction;
                Players[2 * i].SandstarAmount = GlobalVariable.InitialSandstar;
                Players[2 * i].Init();
            }
            // Playersの奇数番目には必ず同一のオブジェクトCellienが入っている
            Players[1].PlayerInfo = null;
            Players[1].PlayerName = "セルリアン軍";
            Players[1].PlayerType = Player.PlayerType.Cellien;
            Players[1].Direction = RotationDirection.LEFT;
            Players[1].Init();
            if (GlobalVariable.Tutorial.HasValue)
            {
                StartCoroutine(TutorialManager.OnStartTutorial(GlobalVariable.Tutorial.Value));
            }
            else {
                StartCoroutine(PlaceFriendsRandomly());
            }
        }

        private IEnumerator PlaceFriendsRandomly()
        {
            yield return new WaitForSeconds(0.5f);
            float delay = Mathf.Min(2.0f/(GlobalVariable.FriendsCount+1),0.3f);
            for(var i = 0; true; i++)
            {
                var player_full_on_downside = false;
                var placed_any_friend = false;
                foreach(var p in Players)
                {
                    if(p.PlayerType == Player.PlayerType.Cellien) { continue; }
                    if (!p.PlayerInfo.HasValue) { continue; }
                    if (p.PlayerInfo.Value.Friends.Length <= i) { continue; }

                    Vector2Int pos;
                    int trial = 0;
                    do
                    {
                        if (i == 0)
                        {
                            // リーダーだけは最下部に置く
                            pos = new Vector2Int(Random.Range(0, GlobalVariable.BoardW), 0);
                        }
                        else
                        {
                            pos = new Vector2Int(Random.Range(0, GlobalVariable.BoardW), Random.Range(0, GlobalVariable.BoardRealmHeight));
                        }
                        if (player_full_on_downside)
                        {
                            pos.y *= -1;
                            pos += new Vector2Int(0, GlobalVariable.BoardH-1);
                        }
                        trial++;
                        if (trial > 10000)
                        {
                            Debug.LogError("Friend cannot be placed!");
                            break;
                        }
                    } while (!Map.TryGetValue(pos, out Cell cell) || cell.Friends != null);
                    PlaceFriend(
                        pos,
                        player_full_on_downside ? RotationDirection.BACKWARD : RotationDirection.FORWARD,
                        p.PlayerInfo.Value.Friends[i],
                        p,
                        i==0);
                    player_full_on_downside = !player_full_on_downside;
                    if (i > 0) {
                        // リーダー以外に遅延表示
                        yield return new WaitForSeconds(delay);
                    }
                    placed_any_friend = true;
                }
                if (!placed_any_friend) { break; }
            }

#if UNITY_EDITOR
            // TODO: デバッグ用機能の削除
            //UnityEditor.EditorApplication.isPaused = true;
#endif
            StartNextTurn = true;
        }

        private void Update()
        {
            if (!init)
            {
                BoardRenderer?.SetBoard();
                init = true;
            }
            if (StartNextTurn)
            {
                StartNextTurn = false;
                StartCoroutine(OnTurnStart());
            }
        }

        public bool PlaceFriend(Vector2Int pos, RotationDirection dir, Friend friend, Player.Player possessor, bool isLeader, bool tryOnly = false)
        {
            if(Map.TryGetValue(pos, out Cell Cell))
            {
                if (Cell.Friends == null)
                {
                    if (!tryOnly)
                    {
                        GameObject friendOnBoardObject = Instantiate(FriendOnBoardPrefab);
                        friendOnBoardObject.transform.SetParent(Cell.transform, false);
                        friendOnBoardObject.transform.localPosition = Vector3.zero;
                        FriendOnBoard friendOnBoard = friendOnBoardObject.GetComponent<FriendOnBoard>();
                        friendOnBoard.Friend = friend;
                        friendOnBoard.InitialSetup(Cell, dir, possessor, isLeader);

                        Cell.Friends = friendOnBoard;

                    }

                    return true;
                }
                else
                {
                    if (!tryOnly) {
                        Debug.LogError("Friends Already Exists!", Cell);
                    }
                    return false;
                }
            }
            else
            {
                if (!tryOnly)
                {
                    Debug.LogError("No such coordinate: " + pos);
                }
                return false;
            }
        }
        public IEnumerator PlaceFriendFromLounge(FriendOnBoard Friend, Cell to)
        {
            // 完全新規のフレンズとして出す
            switch (PlayerInTurn.Direction)
            {
                case RotationDirection.FORWARD:
                    if (to.SelfPos.y >= GlobalVariable.BoardRealmHeight)
                    {
                        // 領域外
                        throw new System.Exception("Out of Realm!");
                    }
                    break;
                case RotationDirection.BACKWARD:
                    if (GlobalVariable.BoardH - 1 - to.SelfPos.y >= GlobalVariable.BoardRealmHeight)
                    {
                        // 領域外
                        throw new System.Exception("Out of Realm!");
                    }
                    break;
                default:
                    // サポート外の向き
                    throw new System.Exception("Player Direction "+PlayerInTurn.Direction+" not supported!");
            }
            if (PlaceFriend(to.SelfPos, Friend.Possessor.Direction, Friend.Friend, Friend.Possessor, false))
            {
                // フレンズをセルごと消す
                Destroy(Friend.Cell.gameObject);
                Friend.Possessor.SandstarAmount -= GlobalVariable.NeededSandstarForPlacingNewFriend;
                Util.PlaySE(SE.SEType.PlaceFriend);
                AddAffectedCell(to);
                yield return new WaitForSeconds(0.3f);
                StartCoroutine(OnTurnPass());
                yield break;
            }
            else
            {
                throw new System.Exception("Cannot Place Friend at "+to.SelfPos+"!");
            }
        }

        public void PlaceFriendAtLounge(Friend friend, Player.Player player)
        {
            GameObject LoungeCellObject = Instantiate(GameUI.LoungeCellPrefab);
            LoungeCellObject.transform.SetParent(player.Lounge, false);
            LoungeCellObject.transform.localPosition = Vector3.zero;
            LoungeCellObject.transform.localRotation = Quaternion.Euler(0, 0, RotationDirectionUtil.GetRotationDegree(player.Direction));

            GameObject fobobject = Instantiate(FriendOnBoardPrefab);
            RectTransform fobTF = fobobject.GetComponent<RectTransform>();
            fobTF.SetParent(LoungeCellObject.transform, false);
            fobTF.localPosition = Vector3.zero;
            // サイズリセット用
            fobTF.anchorMin = Vector2.zero;
            fobTF.anchorMax = Vector2.one;
            fobTF.sizeDelta = Vector2.zero;
            FriendOnBoard fob = fobobject.GetComponent<FriendOnBoard>();
            fob.Friend = friend;
            fob.InitialSetup(null, RotationDirection.FORWARD, player, false);


            LoungeCell LoungeCell = LoungeCellObject.GetComponent<LoungeCell>();
            if (!LoungeCell)
            {
                Debug.LogError("No LoungeCell attached to LoungeCellObject!", GameUI.LoungeCellPrefab);
            }
            else
            {
                LoungeCell.Setup(player, fob);
                fob.MoveToCell(LoungeCell);
                AddAffectedCell(LoungeCell);
            }
            fob.ChangePossessor(player);
            fobobject.transform.SetParent(LoungeCell.transform, false);
            fobobject.transform.localPosition = Vector3.zero;
            fobobject.transform.localRotation = Quaternion.identity;
        }
        public IEnumerator MoveFriend(FriendOnBoard friendOnBoard, Vector2Int to, RotationDirection dir, bool TurnPass=false, Player.Player GoToLoungeOf=null)
        {
            if (Map.TryGetValue(to, out Cell cell_to))
            {
                yield return MoveFriend(friendOnBoard, cell_to, dir, TurnPass, GoToLoungeOf);
            }
            else
            {
                Debug.LogError("No such coordinate: " + to);
            }
            yield return null;
        }
        public IEnumerator MoveFriendWithAnimation(FriendOnBoard friendOnBoard, Cell cell_to, bool TurnPass=false)
        {
            if (friendOnBoard?.Pos == null)
            {
                // friendOnBoardが指定されていない
                throw new System.Exception("Friends not designated!");
            }
            if(!friendOnBoard.Pos.HasValue)
            {
                // 盤上にないフレンズを指定してしまった
                throw new System.Exception("Friends not on any Cell!");
            }
            if (cell_to.RotationOnly)
            {
                // 回転のみのセルに移動しようとした
                throw new System.Exception("Cannot move onto Rotation-Only Cell!");
            }
            if (Map.TryGetValue(friendOnBoard.Pos.Value, out Cell cell_from))
            {
                yield return friendOnBoard.Friend.MoveNormal(friendOnBoard.Pos.Value, cell_to.SelfPos, friendOnBoard);
                if (TurnPass)
                {
                    yield return OnTurnPass();
                }
            }
            else
            {
                Debug.LogError("No such coordinate: " + friendOnBoard.Pos);
            }
            yield return null;
        }
        public IEnumerator MoveFriend(FriendOnBoard friendOnBoard, Cell cell_to, RotationDirection dir, bool TurnPass, Player.Player GoToLoungeOf=null)
        {
            if (friendOnBoard?.Cell == null)
            {
                // friendOnBoardが指定されていない
                throw new System.Exception("Friends not designated!");
            }
            if (!friendOnBoard.Pos.HasValue)
            {
                // 盤上にないフレンズを指定してしまった
                throw new System.Exception("Friends not on any Cell!");
            }
            if (cell_to.RotationOnly)
            {
                // 回転のみのセルに移動しようとした
                throw new System.Exception("Cannot move onto Rotation-Only Cell!");
            }
            if (Map.TryGetValue(friendOnBoard.Pos.Value, out Cell cell_from))
            {
                AddAffectedCell(cell_from);
                cell_from.Friends = null;
                friendOnBoard.MoveToCell(cell_to);
                friendOnBoard.transform.SetParent(cell_to.transform, false);
                friendOnBoard.transform.localPosition = Vector3.zero;
                friendOnBoard.SetDir(dir);
                Util.PlaySE(SE.SEType.PlaceFriend);
                if (cell_to.Friends != null && cell_to.Friends != friendOnBoard)
                {
                    yield return MoveToLounge(cell_to.Friends, GoToLoungeOf);
                }
                cell_to.Friends = friendOnBoard;
                AddAffectedCell(cell_to);
                Debug.Log("Moved friend " + friendOnBoard.Friend.Name + ": " + cell_from.SelfPos + "->" + cell_to.SelfPos);

                if (TurnPass)
                {
                    yield return OnTurnPass();
                }
            }
            else
            {
                Debug.LogError("No such coordinate: " + friendOnBoard.Pos);
            }
            yield return null;
        }
        public IEnumerator MoveToLounge(FriendOnBoard friendOnBoard, Player.Player GoToLoungeOf = null)
        {
            yield return friendOnBoard.GoToLounge(GoToLoungeOf ?? PlayerInTurn);
        }

        public IEnumerator UseSkill(FriendOnBoard friendOnBoard, Cell cell_to)
        {
            if (friendOnBoard?.Pos == null)
            {
                // friendOnBoardが指定されていない
                throw new System.Exception("Friends not designated!");
            }
            if (!friendOnBoard.Pos.HasValue)
            {
                // 盤上にないフレンズを指定してしまった
                throw new System.Exception("Friends not on any Cell!");
            }
            Vector2Int RelativePos = RotationDirectionUtil.GetRelativePos(friendOnBoard.Pos.Value, friendOnBoard.Dir, cell_to.SelfPos);
            SkillMap? _SkillMap = friendOnBoard.Friend.GetSkillMapByPos(RelativePos);
            if (!_SkillMap.HasValue)
            {
                // スキル使用不可の場所を指定してしまった
                throw new System.Exception("This Friends cannot go to "+RelativePos+"!");
            }
            SkillMap SkillMap = _SkillMap.Value;
            if (PlayerInTurn.SandstarAmount < SkillMap.NeededSandstar)
            {
                // サンドスター不足
                throw new System.Exception("Not Enough Sandstar! Needed: " + SkillMap.NeededSandstar + " Having:" + PlayerInTurn.SandstarAmount);
            }
            PlayerInTurn.SandstarAmount -= SkillMap.NeededSandstar;

            if (Map.TryGetValue(friendOnBoard.Pos.Value, out Cell cell_from))
            {
                yield return friendOnBoard.Friend.OnUseSkill(cell_to.SelfPos, friendOnBoard, this);
                if (!friendOnBoard.transform.parent.TryGetComponent<Cell>(out _))
                {
                    // EffectにいるのでCellに戻す
                    cell_from.Friends = null;
                    friendOnBoard.Cell.Friends = friendOnBoard;
                    friendOnBoard.transform.SetParent(friendOnBoard.Cell.transform, false);
                    friendOnBoard.transform.localPosition = Vector3.zero;
                    friendOnBoard.transform.localRotation = Quaternion.identity;

                }
            }
            else
            {
                Debug.LogError("No such coordinate: " + friendOnBoard.Pos);
            }
            yield return null;
        }
        public IEnumerator SkipTurn(bool turnPass = true)
        {
            PlayerInTurn.SandstarAmount += GlobalVariable.GettingSandstarOnWait;
            PlayerInTurn.SandstarAmount = Mathf.Min(PlayerInTurn.SandstarAmount, GlobalVariable.MaxSandstar);
            if (turnPass)
            {
                yield return OnTurnPass();
            }
        }
        public IEnumerator PlayCutIn(Friend friend)
        {
            yield return GameUI.PlayCutIn(friend);
        }

        public IEnumerator OnTurnStart()
        {
            GameUI.ResetView();
            switch (PlayerInTurn.PlayerType)
            {
                case Player.PlayerType.User:
                    if (GlobalVariable.Tutorial != null)
                    {
                        yield return TutorialManager.OnUserTurnStart();
                    }
                    else
                    {
                        yield return GameUI.OnPlayerTurnStart(PlayerInTurn);
                    }
                    yield break;
                case Player.PlayerType.Cellien:
                    // TODO: セルリアンの行動はここに
                    yield return OnTurnPass();
                    break;
                case Player.PlayerType.CPU:
                    if (GlobalVariable.Tutorial != null)
                    {
                        yield return TutorialManager.OnCPUTurnStart();
                    }
                    // TODO: CPUの行動はここに
                    yield return CPU.CPUBehaviour.Exec(this, PlayerInTurnID);
                    break;
                default:
                    Debug.LogWarning("Unknown Player Type "+PlayerInTurn.PlayerType+"! Passing this turn...");
                    yield return OnTurnPass();
                    break;
            }
        }

        public IEnumerator OnTurnPass()
        {

            // サンドスター補給
            PlayerInTurn.SandstarAmount += GlobalVariable.GettingSandstarPerTurn;
            PlayerInTurn.SandstarAmount = Mathf.Min(PlayerInTurn.SandstarAmount, GlobalVariable.MaxSandstar);

            // ターンを次に進める
            PlayerInTurnID = (PlayerInTurnID + 1) % Players.Length;

            // 操作されたセルをリセット（ただし実際に消えるのは次に「操作されたセル」が発生したとき）
            _clean_affected_cells = true;

            // 条件チェック
            if (DoesAnyoneWin(out Player.Player Winner))
            {
                Debug.Log("Winner: " + Winner);
                yield return GameUI.PlayFinish(Winner);
                GlobalVariable.Winner = Winner?.PlayerInfo;
                if (GlobalVariable.Tutorial == null)
                {
                    // チュートリアルではない →通常エンド画面に
                    SceneManager.LoadScene("ResultPage");
                }
                else
                {
                    // チュートリアル中 →処理を戻す
                    yield return TutorialManager.OnTurnStart();
                }
            }
            else
            {
                StartNextTurn = true;
            }
        }

        public bool DoesAnyoneWin(out Player.Player Winner)
        {
            Player.Player Candidate = null;
            foreach(var player in Players)
            {
                if(player.PlayerType == Player.PlayerType.Cellien)
                {
                    // セルリアンは対象外
                    continue;
                }
                if (player.Leader.Pos == null)
                {
                    // 大将フレンズがロビーに移動した：プレイヤーは負けている
                    continue;
                }

                // 勝利候補が2人名乗り出たらまだ続く
                if (Candidate != null)
                {
                    Winner = null;
                    return false;
                }
                Candidate = player;
            }

            // 勝利候補がいればその人の勝ち、いなければ引き分け
            if (Candidate != null)
            {
                Winner = Candidate;
            }
            else
            {
                Winner = null;
            }
            return true;
        }

        private void AddAffectedCell(Cell cell)
        {
            if (_clean_affected_cells)
            {
                _affected_cells.Clear();
                _clean_affected_cells = false;
            }
            if (!_affected_cells.Contains(cell))
            {
                _affected_cells.Add(cell);
            }
        }

#if UNITY_EDITOR
        private void CheckPlayerData()
        {
            if (GlobalVariable.Players == null)
            {
                GlobalVariable.Players = new PlayerInfo[2];
            }
            for (int i= 0; i < 2; i++)
            {
                if (GlobalVariable.Players[i].Friends == null)
                {
                    GlobalVariable.Players[i].Name = "プレイヤー" + (i+1);
                    GlobalVariable.Players[i].ID = i;
                    GlobalVariable.Players[i].PlayerColor = i == 0 ? Color.red : Color.blue;
                    GlobalVariable.Players[i].Direction = i == 0 ? RotationDirection.FORWARD : RotationDirection.BACKWARD;
                    GlobalVariable.Players[i].Friends = new Friend[]
                    {
                        FriendsDatabase.Get().Friends[0],
                        FriendsDatabase.Get().Friends[0],
                        FriendsDatabase.Get().Friends[0],
                    };
                }
                if(GlobalVariable.Players[i].PlayerType != Player.PlayerType.User && GlobalVariable.Players[i].PlayerType != Player.PlayerType.CPU)
                {
                    GlobalVariable.Players[i].PlayerType = Player.PlayerType.User;
                }
            }
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : Editor
    {
        Snapshot snapshot;

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            var manager = target as GameManager;
            if (GUILayout.Button("Snapshot"))
            {
                snapshot = new Snapshot(manager);
            }
            
            using(new EditorGUI.DisabledGroupScope(snapshot == null))
            {
                if (GUILayout.Button("Load"))
                {
                    snapshot.Restore(manager);
                }
            }
        }
    }
#endif
}