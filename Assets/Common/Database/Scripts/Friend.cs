using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Common;
using JSF.Game;
using JSF.Game.Player;
using System.Linq;
using System;
using UnityEngine.Serialization;

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
                                    // 行けないセルに行こうとしたので中止
                                }else if (cell.RotationOnly)
                                {
                                    // 行けないセルに行こうとしたので中止
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

        // スキルが発動できる場所にいるか等を計算
        public SkillSimulationResult SimulateSkill(Vector2Int to, FriendOnBoard friendOnBoard, GameManager GameManager)
        {
            Vector2Int from = friendOnBoard.Pos.Value;
            RotationDirection fromDir = friendOnBoard.Dir;

            Vector2Int RelativePos = RotationDirectionUtil.GetRelativePos(friendOnBoard.Pos.Value, friendOnBoard.Dir, to);
            SkillMap? _SkillMap = friendOnBoard.Friend.GetSkillMapByPos(RelativePos);
            if (_SkillMap.HasValue)
            {
                var SkillMap = _SkillMap.Value;
                if (SkillMap.ActionDescriptor?.FriendAction == null)
                {
                    Debug.LogWarning("Invalid skill detected! " + Name + "/" + SkillMap.Name + " does not have any ActionDescriptor or FriendAction!");
                    return new SkillSimulationResult() { CanUseSkill = false };
                }
                HashSet<Vector2Int> Trace = new HashSet<Vector2Int>() { friendOnBoard.Pos.Value };
                HashSet<Vector2Int> AimedPos = new HashSet<Vector2Int>() { };
                RotationDirection SimulatedDir = friendOnBoard.Dir;
                List<FriendOnBoard> GettingFriends = new List<FriendOnBoard>();
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
                                    // 行けないセルに行こうとしたので中止
                                    return new SkillSimulationResult() { CanUseSkill = false };
                                }
                                else if (cell.RotationOnly)
                                {
                                    // 行けないセルに行こうとしたので中止
                                    return new SkillSimulationResult() { CanUseSkill = false };
                                }
                                else
                                {
                                    Trace.Add(to_pos);
                                    if (!AimedPos.Contains(to_pos))
                                    {
                                        AimedPos.Add(to_pos);
                                        if (cell.Friends && !GettingFriends.Contains(cell.Friends))
                                        {
                                            GettingFriends.Add(cell.Friends);
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
                                        dir = GameManager.PlayerInTurn.Direction;
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
                                    // 行けないセルに行こうとしたのでこのセクションは無視
                                }
                                else if (cell.RotationOnly)
                                {
                                    // 行けないセルに行こうとしたのでこのセクションは無視
                                }
                                else
                                {
                                    if (!AimedPos.Contains(to_pos))
                                    {
                                        AimedPos.Add(to_pos);
                                        if (cell.Friends && !GettingFriends.Contains(cell.Friends))
                                        {
                                            GettingFriends.Add(cell.Friends);
                                        }
                                    }
                                }
                            }
                            break;
                        case FriendActionType.EndTurn:
                            return new SkillSimulationResult()
                            {
                                CanUseSkill = true,
                                GettingFriends = GettingFriends.ToArray(),
                                Trace = Trace.ToArray(),
                                AimedPos = AimedPos.ToArray(),
                                LastDir = SimulatedDir,
                            };
                    }
                }

                return new SkillSimulationResult() {
                    CanUseSkill = true,
                    GettingFriends = GettingFriends.ToArray(),
                    Trace = Trace.ToArray(),
                    AimedPos = AimedPos.ToArray(),
                    LastDir = SimulatedDir,
                };
            }
            else
            {
                // スキルが発動できない箇所で調査しようとした (そのようなケースはよくあるのでコメントアウト)
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
                            // 未定義のプレイヤー(仮に現在のプレイヤーにしておく)
                            Debug.LogError("Unknown Player!",in_turn);
                            return in_turn;
                        }
                    }
                    else
                    {
                        // セルリアンのため未定義(一応セルリアンにしておく)
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
                            // 未定義のプレイヤー(仮に現在のプレイヤーにしておく)
                            Debug.LogError("Unknown Player!", in_turn);
                            return in_turn;
                        }
                    }
                    else
                    {
                        // セルリアンのためセルリアンに
                        return GameManager.Players[1];
                    }
                default:
                    // 未定義の対象(仮に現在のプレイヤーにしておく)
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
        // 指定された場所に通常スキルで移動できるか
        public bool CanNormalMove(Vector2Int diff)
        {
            return NormalMoveMap.Where((p) => p == diff).Any();
        }

        // 指定された方向に向けるか
        public bool CanRotateTo(Vector2Int diff)
        {
            if (diff.magnitude >= 2) { return false; }
            RotationDirection dir = RotationDirectionUtil.CalcRotationDegreeFromVector(-diff);
            return NormalRotationMap.Where((d) => d == dir).Any();
        }
        // 場所からSkillを探す
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
    }
}