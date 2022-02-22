using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Common;
using JSF.Game;
using JSF.Game.Player;
using System.Linq;
using System;
using UnityEngine.Serialization;
using JSF.Game.Logger;

namespace JSF.Database
{

    [CreateAssetMenu(fileName = "Friend.asset", menuName = "JSF/Friends/FriendData")]
    public class Friend : ScriptableObject
    {
        public string FileName;
        public string Name;
        public IEnumerator MoveNormal(Vector2Int from, Vector2Int to, FriendOnBoard friendsOnBoard)
        {
            var animation_name = "MoveForward";

            friendsOnBoard.transform.SetParent(friendsOnBoard.GameManager.EffectObject, true);
            
            friendsOnBoard.Animator.SetBool(animation_name, true);
            int layer_id = friendsOnBoard.Animator.GetLayerIndex("Movement");
            Vector2Int diff = to - from;
            Debug.Log(diff+": "+ diff.magnitude);
            friendsOnBoard.Animator.SetLayerWeight(layer_id, diff.magnitude * 0.01f);
            Vector3 rot_base = Quaternion.FromToRotation(Vector3.up, new Vector3(diff.x, diff.y, 0)).eulerAngles;
            friendsOnBoard.ViewerTF.transform.localRotation = Quaternion.Euler(0, 0, rot_base.z);

            yield return new WaitUntil(() => friendsOnBoard.Animator.GetCurrentAnimatorStateInfo(layer_id).IsName("WaitingForControl"));
            friendsOnBoard.Animator.SetBool(animation_name, false);
            yield return friendsOnBoard.GameManager.MoveFriend(friendsOnBoard, to, RotationDirectionUtil.CalcRotationDegreeFromVector(-(to-from)));
        }
        public IEnumerator OnUseSkill(Vector2Int to, FriendOnBoard friendOnBoard, GameManager GameManager)
        {
            Vector2Int from = friendOnBoard.Pos.Value;
            RotationDirection fromDir = friendOnBoard.Dir;

            yield return GameManager.PlayCutIn(this);
            friendOnBoard.transform.SetParent(GameManager.EffectObject, true);

            Vector2Int RelativePos = RotationDirectionUtil.GetRelativePos(friendOnBoard.Pos.Value, friendOnBoard.Dir, to);
            SkillMap? _SkillMap = friendOnBoard.Friend.GetSkillMapByPos(RelativePos);
            if (_SkillMap.HasValue)
            {
                var SkillMap = _SkillMap.Value;
                if (SkillMap.ActionDescriptor?.FriendAction == null)
                {
                    throw new Exception("Invalid skill detected! " + Name + "/"+SkillMap.Name+" does not have any ActionDescriptor or FriendAction!");
                }
                foreach (var Action in SkillMap.ActionDescriptor.FriendAction)
                {
                    switch (Action.ActionType)
                    {
                        case FriendActionType.PlayAnimation:
                            {
                                friendOnBoard.Animator.SetBool("Skill", true);
                                int layer_id = friendOnBoard.Animator.GetLayerIndex("Skill");
                                // Animation
                                if (Action.Animation_Animation)
                                {
                                    friendOnBoard.Animator.runtimeAnimatorController = Action.Animation_Animation;
                                }

                                Vector2Int diff = to - friendOnBoard.Pos.Value;

                                // Weight
                                float layerWeight = 1;
                                if (Action.Animation_UseWeight)
                                {
                                    Debug.Log(diff + ": " + diff.magnitude);
                                    layerWeight = diff.magnitude * 0.01f;
                                }
                                friendOnBoard.Animator.SetLayerWeight(layer_id, layerWeight);

                                // Rotation
                                Quaternion rot = Quaternion.FromToRotation(Vector3.up, new Vector3(diff.x, diff.y, 0));
                                switch (Action.Animation_RotationMode)
                                {
                                    case FriendAction.Animation_RotationType.Toward:
                                        rot = Quaternion.FromToRotation(Vector3.up, new Vector3(diff.x, diff.y, 0));
                                        break;
                                    case FriendAction.Animation_RotationType.Now:
                                        rot = Quaternion.identity;
                                        break;
                                    case FriendAction.Animation_RotationType.New:
                                        {
                                            var aim_rot = RotationDirectionUtil.CalcRotationDegreeFromVector(diff);
                                            var rot_diff = RotationDirectionUtil.Merge(aim_rot, RotationDirectionUtil.Invert(friendOnBoard.Dir));
                                            rot = Quaternion.Euler(0, 0, RotationDirectionUtil.GetRotationDegree(rot_diff));
                                        }
                                        break;
                                    case FriendAction.Animation_RotationType.Fixed:
                                        {
                                            var rot_diff = RotationDirectionUtil.Merge(RotationDirection.FORWARD, RotationDirectionUtil.Invert(friendOnBoard.Dir));
                                            rot = Quaternion.Euler(0, 0, RotationDirectionUtil.GetRotationDegree(rot_diff));
                                        }
                                        break;
                                }
                                friendOnBoard.ViewerTF.transform.localRotation = rot;
                                // Wait
                                if (Action.Animation_WaitForEnd)
                                {
                                    yield return new WaitUntil(() => friendOnBoard.Animator.GetCurrentAnimatorStateInfo(2).IsName("SkillEnd"));
                                }
                            }
                            break;
                        case FriendActionType.ResetAnimation:
                            {
                                friendOnBoard.Animator.SetBool("Skill", false);
                            }
                            break;
                        case FriendActionType.MoveToCell:
                            {
                                Vector2Int to_pos = RotationDirectionUtil.GetRotatedVector(Action.MoveToCell_MoveDestinationRelative,friendOnBoard.Dir);
                                if (Action.MoveToCell_AddClickedPos)
                                {
                                    to_pos += to;
                                }
                                else
                                {
                                    to_pos += friendOnBoard.Pos.Value;
                                }
                                if(!GameManager.Map.TryGetValue(to_pos, out var cell))
                                {
                                    // �s���Ȃ��Z���ɍs�����Ƃ����̂Œ��~
                                }else if (cell.RotationOnly)
                                {
                                    // �s���Ȃ��Z���ɍs�����Ƃ����̂Œ��~
                                }
                                else
                                {
                                    yield return GameManager.MoveFriend(
                                        friendOnBoard, to_pos, friendOnBoard.Dir, false,
                                        GetGoToLoungeOf(GameManager, Action, GetCellFriendPossessor(GameManager, to_pos)));
                                }
                            }
                            break;
                        case FriendActionType.Rotate:
                            {
                                RotationDirection dir;
                                switch (Action.Rotate_RelativeTo)
                                {
                                    case FriendAction.Rotation_RelativeTo.Toward:
                                        dir = RotationDirectionUtil.CalcRotationDegreeFromVector(from-to);
                                        break;
                                    case FriendAction.Rotation_RelativeTo.Base:
                                        dir = fromDir;
                                        break;
                                    case FriendAction.Rotation_RelativeTo.Now:
                                        dir = friendOnBoard.Dir;
                                        break;
                                    case FriendAction.Rotation_RelativeTo.Fixed:
                                    default:
                                        dir = GameManager.PlayerInTurn.Direction;
                                        break;
                                }
                                dir = RotationDirectionUtil.Merge(dir, Action.Rotate_RotationDirection);
                                yield return GameManager.MoveFriend(
                                    friendOnBoard, friendOnBoard.Pos.Value, dir, false, null);
                            }
                            break;
                        case FriendActionType.MoveToLounge:
                            {
                                Vector2Int to_pos = RotationDirectionUtil.GetRotatedVector(Action.MoveToLounge_MoveDestinationRelative, friendOnBoard.Dir);
                                if (Action.MoveToLounge_AddClickedPos)
                                {
                                    to_pos += to;
                                }
                                else
                                {
                                    to_pos += friendOnBoard.Pos.Value;
                                }
                                if(GameManager.Map.TryGetValue(to_pos, out var cell) && cell.Friends)
                                {
                                    yield return GameManager.MoveToLounge(
                                        cell.Friends,
                                        GetGoToLoungeOf(
                                            GameManager,
                                            Action,
                                            GetCellFriendPossessor(GameManager,to_pos)
                                        )
                                    );
                                }
                            }
                            break;
                        case FriendActionType.EndTurn:
                            GameManager.StartCoroutine(GameManager.OnTurnPass());
                            yield break;
                    }
                    if (Action.WaitSec > 0)
                    {
                        yield return new WaitForSeconds(Action.WaitSec);
                    }
                }
                yield break;
            }
            else
            {
                throw new Exception("Invalid skill detected! " + Name + " does not have skill on " + RelativePos+"!");
            }
        }

        /// <summary>
        /// �ʏ�ړ����̃V�~�����[�V�������s���֐�
        /// </summary>
        /// <param name="base_status">�V�~�����[�V�����O�̃X�i�b�v�V���b�g</param>
        /// <param name="from">�ǂ̃Z���̃t�����Y���ړ������邩</param>
        /// <param name="to_pos">�ǂ��Ɉړ������邩</param>
        /// <param name="GameManager">���݂�GameManager(�Ֆʏ��ȂǏ�Ԃɂ���ĕς��Ȃ����̂ɂ̂݃A�N�Z�X���܂�)</param>
        /// <returns>�V�~�����[�V��������</returns>
        public Snapshot SimulateNormalMove(Snapshot base_status, Vector2Int from, Vector2Int to_pos, GameManager GameManager)
        {

            var _actor_info = base_status.GetFriendInformationAt(from);
            if (!_actor_info.HasValue)
            {
                Debug.LogWarning("No friend at " + from + "!");
                return null;
            }
            var actor_info = _actor_info.Value;
            if (!actor_info.Pos.HasValue)
            {
                Debug.LogWarning("Friend " + actor_info.Friend.Name + " is at lounge!");
                return null;
            }
            RotationDirection fromDir = actor_info.Dir;

            HashSet<Vector2Int> Trace = new HashSet<Vector2Int>() { actor_info.Pos.Value };
            HashSet<Vector2Int> AimedPos = new HashSet<Vector2Int>() { };
            RotationDirection SimulatedDir = actor_info.Dir;

            // �S�t�����Y�̈ꗗ
            List<SnapshotFriend> Friends = base_status.Friends.ToList();
            // �����瓮���t�����Y�͐�ɔ����Ă���
            Friends.Remove(actor_info);
            // �t�����Y�̈ꗗ�Ɉʒu�C���f�b�N�X��t����������(��u����̃t�����Y�͊܂܂Ȃ�)
            Dictionary<Vector2Int, SnapshotFriend> FriendsOnBoardDatabase = Friends
                .Where((f) => f.Pos.HasValue)
                .ToDictionary((f) => f.Pos.Value);

            // �擾�����t�����Y�̈ꗗ
            List<SnapshotFriend> GettingFriends = new List<SnapshotFriend>();

            {
                if (!GameManager.Map.TryGetValue(to_pos, out var cell))
                {
                    // �s���Ȃ��Z���ɍs�����Ƃ����̂Œ��~
                    return null;
                }
                else if (cell.RotationOnly)
                {
                    // �s���Ȃ��Z���ɍs�����Ƃ����̂Œ��~
                    return null;
                }
                else
                {
                    Trace.Add(to_pos);
                    if (!AimedPos.Contains(to_pos))
                    {
                        AimedPos.Add(to_pos);
                        var _friends_on_destination_cell = base_status.GetFriendInformationAt(to_pos);
                        if (_friends_on_destination_cell.HasValue && !GettingFriends.Contains(_friends_on_destination_cell.Value))
                        {
                            if (FriendsOnBoardDatabase.TryGetValue(to_pos, out var prev_status))
                            {
                                FriendsOnBoardDatabase.Remove(to_pos);

                                Friends.Remove(prev_status);
                                Friends.Add(new SnapshotFriend()
                                {
                                    Pos = null,
                                    Dir = GameManager.Players[actor_info.PossessorID].Direction,
                                    Friend = prev_status.Friend,
                                    IsLeader = prev_status.IsLeader,
                                    PossessorID = actor_info.PossessorID,
                                });

                                GettingFriends.Add(prev_status);
                            }
                        }
                    }
                }
            }
            // ��菜���Ă��������g���Ō�ɉ�����
            Friends.Add(new SnapshotFriend()
            {
                Pos = Trace.Last(),
                Dir = SimulatedDir,
                Friend = actor_info.Friend,
                IsLeader = actor_info.IsLeader,
                PossessorID = actor_info.PossessorID
            });

            // Snapshot�p�f�[�^�쐬
            Snapshot after_snapshot = new Snapshot(
                base_status.PlayerInTurnID,
                base_status.SandstarAmounts.ToArray(),
                Friends.ToArray());

            return after_snapshot;
        }

        /// <summary>
        /// �ʏ��]���̃V�~�����[�V�������s���֐�
        /// </summary>
        /// <param name="base_status">�V�~�����[�V�����O�̃X�i�b�v�V���b�g</param>
        /// <param name="friend_pos">�ǂ̃Z���̃t�����Y���ړ������邩</param>
        /// <param name="rotation">�ǂ̌����ɉ�]�����邩(��Ή�]��)</param>
        /// <param name="GameManager">���݂�GameManager(�Ֆʏ��ȂǏ�Ԃɂ���ĕς��Ȃ����̂ɂ̂݃A�N�Z�X���܂�)</param>
        /// <returns>�V�~�����[�V��������</returns>
        public Snapshot SimulateRotation(Snapshot base_status, Vector2Int friend_pos, RotationDirection rotation, GameManager GameManager)
        {
            SnapshotFriend[] friends = base_status.Friends.ToArray();
            for(var i = 0; i < friends.Length; i++)
            {
                if(friends[i].Pos == friend_pos)
                {
                    friends[i].Dir = rotation;
                }
            }
            return new Snapshot(base_status.PlayerInTurnID, base_status.SandstarAmounts, friends);
        }

        // �X�L���������ł���ꏊ�ɂ��邩�����v�Z
        public SkillSimulationResult SimulateSkill(Vector2Int to, FriendOnBoard friendOnBoard, GameManager GameManager)
        {
            SkillSimulationResult res = SimulateSkill(new Snapshot(GameManager), friendOnBoard.Pos.Value, to, GameManager);
            if (res.CanUseSkill)
            {
                return new SkillSimulationResult()
                {
                    CanUseSkill = true,
                    LastDir = res.LastDir,
                    Trace = res.Trace,
                    AimedPos = res.AimedPos,
                    GettingFriends = res.AimedPos.Select((pos) => GameManager.Map[pos]?.Friends).Where((d) => d != null).ToArray(),
                    Snapshot = res.Snapshot
                };
            }
            else
            {
                return new SkillSimulationResult()
                {
                    CanUseSkill = false
                };
            }
        }

        public SkillSimulationResult SimulateSkill(Snapshot base_status, Vector2Int from, Vector2Int to, GameManager GameManager)
        {

            var _actor_info = base_status.GetFriendInformationAt(from);
            if (!_actor_info.HasValue)
            {
                Debug.LogWarning("No friend at " + from + "!");
                return new SkillSimulationResult() { CanUseSkill = false };
            }
            var actor_info = _actor_info.Value;
            if (!actor_info.Pos.HasValue)
            {
                Debug.LogWarning("Friend " + actor_info.Friend.Name + " is at lounge!");
                return new SkillSimulationResult() { CanUseSkill = false };
            }

            Vector2Int RelativePos = RotationDirectionUtil.GetRelativePos(actor_info.Pos.Value, actor_info.Dir, to);
            RotationDirection fromDir = actor_info.Dir;

            SkillMap? _SkillMap = actor_info.Friend.GetSkillMapByPos(RelativePos);
            if (_SkillMap.HasValue)
            {
                var SkillMap = _SkillMap.Value;
                if (SkillMap.ActionDescriptor?.FriendAction == null)
                {
                    Debug.LogWarning("Invalid skill detected! " + Name + "/" + SkillMap.Name + " does not have any ActionDescriptor or FriendAction!");
                    return new SkillSimulationResult() { CanUseSkill = false };
                }
                HashSet<Vector2Int> Trace = new HashSet<Vector2Int>() { actor_info.Pos.Value };
                HashSet<Vector2Int> AimedPos = new HashSet<Vector2Int>() { };
                RotationDirection SimulatedDir = actor_info.Dir;

                // �S�t�����Y�̈ꗗ
                List<SnapshotFriend> Friends = base_status.Friends.ToList();
                // �����瓮���t�����Y�͐�ɔ����Ă���
                Friends.Remove(actor_info);
                // �t�����Y�̈ꗗ�Ɉʒu�C���f�b�N�X��t����������(��u����̃t�����Y�͊܂܂Ȃ�)
                Dictionary<Vector2Int, SnapshotFriend> FriendsOnBoardDatabase = Friends
                    .Where((f) => f.Pos.HasValue)
                    .ToDictionary((f) => f.Pos.Value);

                // �擾�����t�����Y�̈ꗗ
                List<SnapshotFriend> GettingFriends = new List<SnapshotFriend>();

                foreach (var Action in SkillMap.ActionDescriptor.FriendAction)
                {
                    switch (Action.ActionType)
                    {
                        case FriendActionType.PlayAnimation:
                            break;
                        case FriendActionType.ResetAnimation:
                            break;
                        case FriendActionType.MoveToCell:
                            {
                                Vector2Int to_pos = RotationDirectionUtil.GetRotatedVector(Action.MoveToCell_MoveDestinationRelative, SimulatedDir);
                                if (Action.MoveToCell_AddClickedPos)
                                {
                                    to_pos += to;
                                }
                                else
                                {
                                    to_pos += Trace.Last();
                                }
                                if (!GameManager.Map.TryGetValue(to_pos, out var cell))
                                {
                                    // �s���Ȃ��Z���ɍs�����Ƃ����̂Œ��~
                                    return new SkillSimulationResult() { CanUseSkill = false };
                                }
                                else if (cell.RotationOnly)
                                {
                                    // �s���Ȃ��Z���ɍs�����Ƃ����̂Œ��~
                                    return new SkillSimulationResult() { CanUseSkill = false };
                                }
                                else
                                {
                                    Trace.Add(to_pos);
                                    if (!AimedPos.Contains(to_pos))
                                    {
                                        AimedPos.Add(to_pos);
                                        var _friends_on_destination_cell = base_status.GetFriendInformationAt(to_pos);
                                        if (_friends_on_destination_cell.HasValue && !GettingFriends.Contains(_friends_on_destination_cell.Value))
                                        {
                                            if(FriendsOnBoardDatabase.TryGetValue(to_pos, out var prev_status))
                                            {
                                                FriendsOnBoardDatabase.Remove(to_pos);

                                                Friends.Remove(prev_status);
                                                Friends.Add(new SnapshotFriend()
                                                {
                                                    Pos = null,
                                                    Dir = GameManager.Players[actor_info.PossessorID].Direction,
                                                    Friend = prev_status.Friend,
                                                    IsLeader = prev_status.IsLeader,
                                                    PossessorID = actor_info.PossessorID,
                                                });

                                                GettingFriends.Add(prev_status);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case FriendActionType.Rotate:
                            {
                                RotationDirection dir;
                                switch (Action.Rotate_RelativeTo)
                                {
                                    case FriendAction.Rotation_RelativeTo.Toward:
                                        dir = RotationDirectionUtil.CalcRotationDegreeFromVector(from - to);
                                        break;
                                    case FriendAction.Rotation_RelativeTo.Base:
                                        dir = fromDir;
                                        break;
                                    case FriendAction.Rotation_RelativeTo.Now:
                                        dir = SimulatedDir;
                                        break;
                                    case FriendAction.Rotation_RelativeTo.Fixed:
                                    default:
                                        dir = GameManager.Players[base_status.PlayerInTurnID].Direction;
                                        break;
                                }
                                SimulatedDir = RotationDirectionUtil.Merge(dir, Action.Rotate_RotationDirection);
                            }
                            break;
                        case FriendActionType.MoveToLounge:
                            {
                                Vector2Int to_pos = RotationDirectionUtil.GetRotatedVector(Action.MoveToLounge_MoveDestinationRelative, SimulatedDir);
                                if (Action.MoveToLounge_AddClickedPos)
                                {
                                    to_pos += to;
                                }
                                else
                                {
                                    to_pos += Trace.Last();
                                }
                                if (!GameManager.Map.TryGetValue(to_pos, out var cell))
                                {
                                    // �s���Ȃ��Z���ɍs�����Ƃ����̂ł��̃Z�N�V�����͖���
                                }
                                else if (cell.RotationOnly)
                                {
                                    // �s���Ȃ��Z���ɍs�����Ƃ����̂ł��̃Z�N�V�����͖���
                                }
                                else
                                {
                                    if (!AimedPos.Contains(to_pos))
                                    {
                                        AimedPos.Add(to_pos);
                                        var _friends_on_destination_cell = base_status.GetFriendInformationAt(to_pos);
                                        if (_friends_on_destination_cell.HasValue && !GettingFriends.Contains(_friends_on_destination_cell.Value))
                                        {
                                            if (FriendsOnBoardDatabase.TryGetValue(to_pos, out var prev_status))
                                            {
                                                FriendsOnBoardDatabase.Remove(to_pos);

                                                Friends.Remove(prev_status);
                                                Friends.Add(new SnapshotFriend()
                                                {
                                                    Pos = null,
                                                    Dir = GameManager.Players[actor_info.PossessorID].Direction,
                                                    Friend = prev_status.Friend,
                                                    IsLeader = prev_status.IsLeader,
                                                    PossessorID = actor_info.PossessorID,
                                                });

                                                GettingFriends.Add(prev_status);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case FriendActionType.EndTurn:
                            break;
                    }
                }
                // ��菜���Ă��������g���Ō�ɉ�����
                Friends.Add(new SnapshotFriend()
                {
                    Pos = Trace.Last(),
                    Dir = SimulatedDir,
                    Friend = actor_info.Friend,
                    IsLeader = actor_info.IsLeader,
                    PossessorID = actor_info.PossessorID
                });

                // Snapshot�p�f�[�^�쐬
                int[] sandstar_amounts = base_status.SandstarAmounts.ToArray();
                sandstar_amounts[actor_info.PossessorID] -= SkillMap.NeededSandstar;
                Snapshot after_snapshot = new Snapshot(
                    base_status.PlayerInTurnID,
                    sandstar_amounts,
                    Friends.ToArray());

                return new SkillSimulationResult()
                {
                    CanUseSkill = true,
                    Trace = Trace.ToArray(),
                    AimedPos = AimedPos.ToArray(),
                    LastDir = SimulatedDir,
                    Snapshot = after_snapshot
                };
            }
            else
            {
                // �X�L���������ł��Ȃ��ӏ��Œ������悤�Ƃ��� (���̂悤�ȃP�[�X�͂悭����̂ŃR�����g�A�E�g)
                // Debug.LogWarning("Friend "+friendOnBoard.Friend.Name+" has no skill at "+RelativePos);
                return new SkillSimulationResult()
                {
                    CanUseSkill = false,
                };
            }
        }

        public bool CanUseSkillWithoutContext(Vector2Int diff)
        {
            return GetSkillMapByPos(diff).HasValue;
        }

        private Player GetGoToLoungeOf(GameManager GameManager, FriendAction Action, Player ToCellPossessor)
        {
            var in_turn = GameManager.PlayerInTurn;
            Player LoungePlayer = in_turn;
            switch (Action.LoungeDestinationMode)
            {
                case FriendAction.LoungeDestinationType.Mine:
                    return in_turn;
                case FriendAction.LoungeDestinationType.Opponent:
                    if (in_turn.PlayerInfo.HasValue)
                    {
                        if (in_turn.PlayerInfo.Value.ID == 0)
                        {
                            return GameManager.Players.Where((p) => p.PlayerInfo.HasValue && p.PlayerInfo.Value.ID == 1).First();
                        }
                        else if (in_turn.PlayerInfo.Value.ID == 1)
                        {
                            return GameManager.Players.Where((p) => p.PlayerInfo.HasValue && p.PlayerInfo.Value.ID == 0).First();
                        }
                        else
                        {
                            // ����`�̃v���C���[(���Ɍ��݂̃v���C���[�ɂ��Ă���)
                            Debug.LogError("Unknown Player!",in_turn);
                            return in_turn;
                        }
                    }
                    else
                    {
                        // �Z�����A���̂��ߖ���`(�ꉞ�Z�����A���ɂ��Ă���)
                        Debug.LogWarning("Cellien cannot move to opponent's lounge!");
                        return GameManager.Players[1];
                    }
                case FriendAction.LoungeDestinationType.Possessor:
                    return ToCellPossessor;
                case FriendAction.LoungeDestinationType.OpponentOfPossessor:
                    if (ToCellPossessor.PlayerInfo.HasValue)
                    {
                        if (ToCellPossessor.PlayerInfo.Value.ID == 0)
                        {
                            return GameManager.Players.Where((p) => p.PlayerInfo.HasValue && p.PlayerInfo.Value.ID == 1).First();
                        }
                        else if (ToCellPossessor.PlayerInfo.Value.ID == 1)
                        {
                            return GameManager.Players.Where((p) => p.PlayerInfo.HasValue && p.PlayerInfo.Value.ID == 0).First();
                        }
                        else
                        {
                            // ����`�̃v���C���[(���Ɍ��݂̃v���C���[�ɂ��Ă���)
                            Debug.LogError("Unknown Player!", in_turn);
                            return in_turn;
                        }
                    }
                    else
                    {
                        // �Z�����A���̂��߃Z�����A����
                        return GameManager.Players[1];
                    }
                default:
                    // ����`�̑Ώ�(���Ɍ��݂̃v���C���[�ɂ��Ă���)
                    Debug.LogError("Unknown Destination Type ("+Action.LoungeDestinationMode+")!", in_turn);
                    return in_turn;
            }
        }
        private Player GetCellFriendPossessor(GameManager GameManager, Vector2Int Pos)
        {
            if(GameManager.Map.TryGetValue(Pos, out var cell))
            {
                if (cell.Friends)
                {
                    return cell.Friends.Possessor;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Debug.LogWarning("No Cell at ("+Pos+")!");
                return null;
            }
        }
        // �w�肳�ꂽ�ꏊ�ɒʏ�X�L���ňړ��ł��邩
        public bool CanNormalMove(Vector2Int diff)
        {
            return NormalMoveMap.Where((p) => p == diff).Any();
        }

        // �w�肳�ꂽ�����Ɍ����邩
        public bool CanRotateTo(Vector2Int diff)
        {
            if (diff.magnitude >= 2) { return false; }
            RotationDirection dir = RotationDirectionUtil.CalcRotationDegreeFromVector(-diff);
            return NormalRotationMap.Where((d) => d == dir).Any();
        }
        // �ꏊ����Skill��T��
        public SkillMap? GetSkillMapByPos(Vector2Int pos)
        {
            foreach(var Skill in Skills)
            {
                if (Skill.Pos.Where((p) => p == pos).Any())
                {
                    return Skill;
                }
            }
            return null;
        }

        public Sprite OnBoardImage;
        [FormerlySerializedAs("CutInImage")]
        public Sprite ThumbImage;
        public AnimatorOverrideController AnimatorOverrideController;

        public string[] SettingsMessage;
        public string[] WinnersMessage;
        public string[] LosersMessage;
        public string[] DrawMessage;

        public Vector2Int[] NormalMoveMap;
        public RotationDirection[] NormalRotationMap;
        public SkillMap[] Skills;

        #region Credit
        public string AuthorName;
        public Sprite AuthorImage;
        #endregion
    }

    public struct SkillSimulationResult
    {
        public bool CanUseSkill;
        public FriendOnBoard[] GettingFriends;
        public Vector2Int[] Trace;
        public Vector2Int LastPos { get => Trace[Trace.Length - 1]; }
        public Vector2Int[] AimedPos;
        public RotationDirection LastDir;
        public Snapshot Snapshot;
    }
}