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
                        pos = new Vector2Int(Random.Range(0, GlobalVariable.BoardW), Random.Range(0, GlobalVariable.BoardRealmHeight));
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
                    yield return new WaitForSeconds(0.3f);
                    placed_any_friend = true;
                }
                if (!placed_any_friend) { break; }
            }
            yield return OnTurnStart();
        }

        private void Update()
        {
            BoardRenderer?.SetBoard();
            enabled = false;
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
                        Debug.Log("Placed friend " + friendOnBoard.Friend.Name + ": " + pos + " rot=" + dir.ToString());
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
        public bool PlaceFriendFromLounge(FriendOnBoard Friend, Cell to, bool tryOnly = false)
        {
            // 完全新規のフレンズとして出す
            switch (PlayerInTurn.Direction)
            {
                case RotationDirection.FORWARD:
                    if (to.SelfPos.y > GlobalVariable.BoardRealmHeight)
                    {
                        // 領域外
                        if (!tryOnly)
                        {
                            Debug.LogError("Out of Realm!");
                        }
                        return false;
                    }
                    break;
                case RotationDirection.BACKWARD:
                    if (GlobalVariable.BoardH - 1 - to.SelfPos.y > GlobalVariable.BoardRealmHeight)
                    {
                        // 領域外
                        if (!tryOnly)
                        {
                            Debug.LogError("Out of Realm!");
                        }
                        return false;
                    }
                    break;
                default:
                    return false;
            }
            if (PlaceFriend(to.SelfPos, Friend.Possessor.Direction, Friend.Friend, Friend.Possessor, tryOnly))
            {
                if (!tryOnly)
                {
                    // フレンズをセルごと消す
                    Destroy(Friend.Cell.gameObject);
                    Friend.Possessor.SandstarAmount -= GlobalVariable.NeededSandstarForPlacingNewFriend;
                    StartCoroutine(OnTurnPass());
                }
                return true;
            }
            return false;
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
            if (Map.TryGetValue(friendOnBoard.Pos.Value, out Cell cell_from))
            {
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
                //yield return OnTurnPass();
            }
            else
            {
                Debug.LogError("No such coordinate: " + friendOnBoard.Pos);
            }
            yield return null;
        }
        public IEnumerator SkipTurn(bool turnPass = true)
        {
            PlayerInTurn.SandstarAmount += 1;// TODO: SandstarParameterを使う
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
                    yield return OnTurnPass();
                    break;
                default:
                    Debug.LogWarning("Unknown Player Type "+PlayerInTurn.PlayerType+"! Passing this turn...");
                    yield return OnTurnPass();
                    break;
            }
        }

        public IEnumerator OnTurnPass()
        {
            // サンドスター補給 TODO:SandstarParameterを使う
            PlayerInTurn.SandstarAmount += 1;

            // ターンを次に進める
            PlayerInTurnID = (PlayerInTurnID + 1) % Players.Length;
            // 条件チェック
            if(DoesAnyoneWin(out Player.Player Winner))
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
                yield return OnTurnStart();
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
                    GlobalVariable.Players[i].PlayerType = Player.PlayerType.User;
                    GlobalVariable.Players[i].Direction = i == 0 ? RotationDirection.FORWARD : RotationDirection.BACKWARD;
                    GlobalVariable.Players[i].Friends = new Friend[]
                    {
                        FriendsDatabase.Get().Friends[0],
                        FriendsDatabase.Get().Friends[0],
                        FriendsDatabase.Get().Friends[0],
                    };
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