using JSF.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Game.UI;
using JSF.Game.Board;
using JSF.Database.Friends;
using UnityEngine.SceneManagement;

namespace JSF.Game
{
    public class GameManager : MonoBehaviour
    {
        public GameObject FriendOnBoardPrefab;

        public Player.Player[] Players;
        [SerializeField]
        private int PlayerInTurnID = 0;
        public Player.Player PlayerInTurn { get => Players[PlayerInTurnID]; }

        public Dictionary<Vector2Int, Cell> Map { get; private set; } = new Dictionary<Vector2Int, Cell>();
        public Dictionary<Vector2Int, FriendOnBoard> Friends { get; private set; } = new Dictionary<Vector2Int, FriendOnBoard>();

        public GameUI GameUI;
        public BoardRenderer BoardRenderer;

        public Transform EffectObject;

        public void Start()
        {
            // 盤面設定
            BoardRenderer.W = GlobalVariable.BoardW;
            BoardRenderer.H = GlobalVariable.BoardH;

            for (var i = 0; i < GlobalVariable.Players.Length; i++)
            {
                // 2倍しているのは間にセルリアンを挟むため
                Players[2 * i].PlayerInfo = GlobalVariable.Players[i];
                Players[2 * i].PlayerName = GlobalVariable.Players[i].Name;
                Players[2 * i].PlayerType = GlobalVariable.Players[i].PlayerType;
                Players[2 * i].Init();
            }
            // Playersの奇数番目には必ず同一のオブジェクトCellienが入っている
            Players[1].PlayerInfo = null;
            Players[1].PlayerName = "セルリアン軍";
            Players[1].PlayerType = Player.PlayerType.Cellien;
            Players[1].Init();
            if (GlobalVariable.DebugMode)
            {
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
                    do
                    {
                        pos = new Vector2Int(Random.Range(0, BoardRenderer.W), Random.Range(0, BoardRenderer.H / 2));
                        if (player_full_on_downside)
                        {
                            pos += new Vector2Int(0, (BoardRenderer.H + 1) / 2);
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

        public bool PlaceFriend(Vector2Int pos, RotationDirection dir, Friend friend, Player.Player possessor, bool isLeader)
        {
            if(Map.TryGetValue(pos, out Cell Cell))
            {
                if (Cell.Friends == null)
                {
                    GameObject friendOnBoardObject = Instantiate(FriendOnBoardPrefab);
                    friendOnBoardObject.transform.SetParent(Cell.transform, false);
                    friendOnBoardObject.transform.localPosition = Vector3.zero;
                    FriendOnBoard friendOnBoard = friendOnBoardObject.GetComponent<FriendOnBoard>();
                    friendOnBoard.Friend = friend;
                    friendOnBoard.InitialSetup(Cell, dir, possessor, isLeader);

                    Cell.Friends = friendOnBoard;
                    Debug.Log("Placed friend " + friendOnBoard.Friend.Name + ": " + pos +" rot="+dir.ToString());

                    return true;
                }
                else
                {
                    Debug.LogError("Friends Already Exists!", Cell);
                    return false;
                }
            }
            else
            {
                Debug.LogError("No such coordinate: " + pos);
                return false;
            }
        }
        public IEnumerator MoveFriend(FriendOnBoard friendOnBoard, Vector2Int to, RotationDirection dir, bool TurnPass=false)
        {
            if (Map.TryGetValue(to, out Cell cell_to))
            {
                yield return MoveFriend(friendOnBoard, cell_to, dir, TurnPass);
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
                if (TurnPass) { yield return OnTurnPass(); }
            }
            else
            {
                Debug.LogError("No such coordinate: " + friendOnBoard.Pos);
            }
            yield return null;
        }
        public IEnumerator MoveFriend(FriendOnBoard friendOnBoard, Cell cell_to, RotationDirection dir, bool TurnPass)
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
            if (Map.TryGetValue(friendOnBoard.Pos.Value, out Cell cell_from))
            {
                if (cell_to.Friends != null && cell_to.Friends != friendOnBoard)
                {
                    yield return cell_to.Friends.GoToLounge(PlayerInTurn);
                }
                cell_from.Friends = null;
                friendOnBoard.MoveToCell(cell_to);
                cell_to.Friends = friendOnBoard;
                friendOnBoard.transform.SetParent(cell_to.transform, false);
                friendOnBoard.transform.localPosition = Vector3.zero;
                friendOnBoard.SetDir(dir);
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
            if (!friendOnBoard.Friend.GetMovementSheet().TryGetValue(RelativePos, out MovementSettings movementSettings))
            {
                // スキル使用不可の場所を指定してしまった
                throw new System.Exception("This Friends cannot go to "+RelativePos+"!");
            }
            if (!movementSettings.CanEffectBySkill)
            {
                // スキル使用不可の場所を指定してしまった
                throw new System.Exception("This Friends cannot use skill to " + RelativePos + "!");
            }
            if (PlayerInTurn.SandstarAmount < movementSettings.NeededSandstarForSkill)
            {
                // サンドスター不足
                throw new System.Exception("Not Enough Sandstar! Needed: " + movementSettings.NeededSandstarForSkill + " Having:" + PlayerInTurn.SandstarAmount);
            }
            PlayerInTurn.SandstarAmount -= movementSettings.NeededSandstarForSkill;

            if (Map.TryGetValue(friendOnBoard.Pos.Value, out Cell cell_from))
            {
                yield return friendOnBoard.Friend.OnUseSkill(friendOnBoard.Pos.Value, cell_to.SelfPos, friendOnBoard, this);
                yield return OnTurnPass();
            }
            else
            {
                Debug.LogError("No such coordinate: " + friendOnBoard.Pos);
            }
            yield return null;
        }
        public IEnumerator SkipTurn()
        {
            PlayerInTurn.SandstarAmount += 1;// TODO: SandstarParameterを使う
            yield return OnTurnPass();
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
                    yield return GameUI.OnPlayerTurnStart(PlayerInTurn);
                    yield break;
                case Player.PlayerType.Cellien:
                    // TODO: セルリアンの行動はここに
                    yield return OnTurnPass();
                    break;
                case Player.PlayerType.CPU:
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
            PlayerInTurnID = (PlayerInTurnID + 1) % Players.Length;
            // 条件チェック
            if(DoesAnyoneWin(out Player.Player Winner))
            {
                Debug.Log("Winner: "+Winner);
                yield return GameUI.PlayFinish(Winner);
                GlobalVariable.Winner = Winner.PlayerInfo;
                SceneManager.LoadScene("ResultPage");
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
                if (player.Leader.Cell == null)
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

            // 勝利候補がいればその人の勝ち、いなければまだ続く
            if (Candidate != null)
            {
                Winner = Candidate;
                return true;
            }
            else
            {
                Winner = null;
                return false;
            }
        }
    }
}