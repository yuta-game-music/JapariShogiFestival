using JSF.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Game.UI;
using JSF.Game.Board;
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
            // �Ֆʐݒ�
            BoardRenderer.W = GlobalVariable.BoardW;
            BoardRenderer.H = GlobalVariable.BoardH;

            for (var i = 0; i < GlobalVariable.Players.Length; i++)
            {
                // 2�{���Ă���̂͊ԂɃZ�����A�������ނ���
                Players[2 * i].PlayerInfo = GlobalVariable.Players[i];
                Players[2 * i].PlayerName = GlobalVariable.Players[i].Name;
                Players[2 * i].PlayerType = GlobalVariable.Players[i].PlayerType;
                Players[2 * i].Direction = GlobalVariable.Players[i].Direction;
                Players[2 * i].Init();
            }
            // Players�̊�Ԗڂɂ͕K������̃I�u�W�F�N�gCellien�������Ă���
            Players[1].PlayerInfo = null;
            Players[1].PlayerName = "�Z�����A���R";
            Players[1].PlayerType = Player.PlayerType.Cellien;
            Players[1].Direction = RotationDirection.LEFT;
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
                // friendOnBoard���w�肳��Ă��Ȃ�
                throw new System.Exception("Friends not designated!");
            }
            if(!friendOnBoard.Pos.HasValue)
            {
                // �Տ�ɂȂ��t�����Y���w�肵�Ă��܂���
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
            if (friendOnBoard?.Pos == null)
            {
                // friendOnBoard���w�肳��Ă��Ȃ�
                throw new System.Exception("Friends not designated!");
            }
            if (!friendOnBoard.Pos.HasValue)
            {
                // �Տ�ɂȂ��t�����Y���w�肵�Ă��܂���
                throw new System.Exception("Friends not on any Cell!");
            }
            if (Map.TryGetValue(friendOnBoard.Pos.Value, out Cell cell_from))
            {
                if (cell_to.Friends != null && cell_to.Friends != friendOnBoard)
                {
                    yield return MoveToLounge(cell_to.Friends, GoToLoungeOf);
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
        public IEnumerator MoveToLounge(FriendOnBoard friendOnBoard, Player.Player GoToLoungeOf = null)
        {
            yield return friendOnBoard.GoToLounge(GoToLoungeOf ?? PlayerInTurn);
        }

        public IEnumerator UseSkill(FriendOnBoard friendOnBoard, Cell cell_to)
        {
            if (friendOnBoard?.Pos == null)
            {
                // friendOnBoard���w�肳��Ă��Ȃ�
                throw new System.Exception("Friends not designated!");
            }
            if (!friendOnBoard.Pos.HasValue)
            {
                // �Տ�ɂȂ��t�����Y���w�肵�Ă��܂���
                throw new System.Exception("Friends not on any Cell!");
            }
            Vector2Int RelativePos = RotationDirectionUtil.GetRelativePos(friendOnBoard.Pos.Value, friendOnBoard.Dir, cell_to.SelfPos);
            SkillMap? _SkillMap = friendOnBoard.Friend.GetSkillMapByPos(RelativePos);
            if (!_SkillMap.HasValue)
            {
                // �X�L���g�p�s�̏ꏊ���w�肵�Ă��܂���
                throw new System.Exception("This Friends cannot go to "+RelativePos+"!");
            }
            SkillMap SkillMap = _SkillMap.Value;
            if (PlayerInTurn.SandstarAmount < SkillMap.NeededSandstar)
            {
                // �T���h�X�^�[�s��
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
        public IEnumerator SkipTurn()
        {
            PlayerInTurn.SandstarAmount += 1;// TODO: SandstarParameter���g��
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
                    // TODO: �Z�����A���̍s���͂�����
                    yield return OnTurnPass();
                    break;
                case Player.PlayerType.CPU:
                    // TODO: CPU�̍s���͂�����
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
            // �T���h�X�^�[�⋋ TODO:SandstarParameter���g��
            PlayerInTurn.SandstarAmount += 1;

            // �^�[�������ɐi�߂�
            PlayerInTurnID = (PlayerInTurnID + 1) % Players.Length;
            // �����`�F�b�N
            if(DoesAnyoneWin(out Player.Player Winner))
            {
                Debug.Log("Winner: "+Winner);
                yield return GameUI.PlayFinish(Winner);
                GlobalVariable.Winner = Winner?.PlayerInfo;
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
                    // �Z�����A���͑ΏۊO
                    continue;
                }
                if (player.Leader.Cell == null)
                {
                    // �叫�t�����Y�����r�[�Ɉړ������F�v���C���[�͕����Ă���
                    continue;
                }

                // ������₪2�l�����o����܂�����
                if (Candidate != null)
                {
                    Winner = null;
                    return false;
                }
                Candidate = player;
            }

            // ������₪����΂��̐l�̏����A���Ȃ���Έ�������
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
    }
}