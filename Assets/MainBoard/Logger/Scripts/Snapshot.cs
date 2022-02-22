using JSF.Database;
using JSF.Game.Board;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JSF.Game.Logger
{
    public class Snapshot
    {
        public int PlayerInTurnID { get; private set; }
        public int[] SandstarAmounts { get; private set; } = new int[4];
        public SnapshotFriend[] Friends { get; private set; }
        public Snapshot(GameManager manager)
        {
            PlayerInTurnID = manager.PlayerInTurnID;

            List<SnapshotFriend> Friends = new List<SnapshotFriend>();

            foreach (var v in manager.Map.Keys)
            {
                if (manager.Map[v].Friends)
                {
                    Friends.Add(new SnapshotFriend()
                    {
                        Pos = v,
                        Friend = manager.Map[v].Friends.Friend,
                        Dir= manager.Map[v].Friends.Dir,
                        PossessorID = (manager.Map[v].Friends.Possessor.PlayerInfo?.ID * 2) ?? -1,
                        IsLeader = manager.Map[v].Friends.IsLeader
                    });
                }
            }
            foreach (var p in manager.Players)
            {
                if (p.PlayerType == Player.PlayerType.Cellien) { continue; }
                foreach (var c in p.Lounge.GetComponentsInChildren<LoungeCell>())
                {
                    Friends.Add(new SnapshotFriend()
                    {
                        Pos = null,
                        Friend = c.Friends.Friend,
                        Dir = RotationDirection.FORWARD,
                        PossessorID = (p.PlayerInfo?.ID * 2) ?? -1
                    });
                }
                SandstarAmounts[(p.PlayerInfo?.ID * 2) ?? 0] = p.SandstarAmount;
            }
            this.Friends = Friends.ToArray();
        }
        public Snapshot(int playerInTurnID, int[] sandstarAmounts, SnapshotFriend[] snapshotFriends)
        {
            PlayerInTurnID = playerInTurnID;
            SandstarAmounts = sandstarAmounts;
            Friends = snapshotFriends;
        }

        public void Restore(GameManager manager)
        {
            manager.PlayerInTurnID = PlayerInTurnID;
            // ��x�Տ�̃t�����Y��S������
            manager.ClearAllFriends();
            foreach (var p in manager.Players)
            {
                // �T���h�X�^�[�ʂ͂����Ŗ߂��Ă��܂�
                p.SandstarAmount = SandstarAmounts[(p.PlayerInfo?.ID * 2) ?? 0];

            }
            // �Ĕz�u
            foreach (var f in Friends)
            {
                if (f.Pos.HasValue)
                {
                    // �Տ�ɒu��
                    manager.PlaceFriend(f.Pos.Value, f.Dir, f.Friend, manager.Players[2 * f.PossessorID], f.IsLeader);
                }
                else
                {
                    // �����t�����Y�ɒu��
                    manager.PlaceFriendAtLounge(f.Friend, manager.Players[2 * f.PossessorID]);
                }
            }
        }

        public SnapshotFriend? GetFriendInformationAt(Vector2Int pos)
        {
            foreach (var f in Friends)
            {
                if (f.Pos == pos)
                {
                    return f;
                }
            }
            return null;
        }
        /// <summary>
        /// �Ֆʂ̕]����Ԃ��܂��B
        /// </summary>
        /// <param name="PlayerID">�ǂ̃v���C���[�ɂ��Ă̕]�����v�Z���邩(GameManager.Players��ID)</param>
        /// <returns>�Ֆʂ̕]������</returns>
        
        public float GetEvaluation(int PlayerID, GameManager manager)
        {
            var score = GetEvaluationForOne(PlayerID, manager);
            score -= GetEvaluationForOne((PlayerID + 2) % manager.Players.Length, manager);
            return score;
        }
        private float GetEvaluationForOne(int PlayerID, GameManager manager)
        {
            float score = 0;

            SnapshotFriend[] opponentLeaders = Friends.Where((f) => f.PossessorID != PlayerID && f.IsLeader && f.Pos.HasValue).ToArray();
            var myFriendsOnBoard = Friends.Where((f) => f.PossessorID == PlayerID && f.Pos.HasValue);

            // �����̎��t�����Y�ɂ��đ���̑叫�ɋ߂��ق�+�������叫�̂ق��Ɍ����Ă���قǍ��X�R�A
            foreach (var opponentLeader in opponentLeaders)
            {
                score += myFriendsOnBoard
                    .Select((f) =>
                    {
                        PositionUtil.CalcCirclePos((opponentLeader.Pos.Value - f.Pos.Value), out int r, out int rot);
                        int rot_max = r * 8;
                        return 
                            Mathf.Max(5-r,1)*1f // �ʒu
                            + (1-Mathf.Abs((float)(r+rot_max/2)%rot_max-rot_max/2)/(rot_max/2))*0.3f// ����(0�ɋ߂��ق�������)
                            ;
                    })
                    .Sum();
            }

            // �������t�����Y�𑽐������Ă���قǍ��X�R�A
            score += Friends.Where((f) => f.PossessorID == PlayerID).Count() * 5f;

            // ���肪�叫�������Ă��Ȃ���ԂȂ�ō��X�R�A
            if(Friends.Where((f)=>f.PossessorID!=PlayerID && f.IsLeader && f.Pos.HasValue).Count() == 0)
            {
                score += 10000;
            }
            // �������叫�������Ă��Ȃ���ԂȂ�X�R�A���ǂ���
            if (Friends.Where((f) => f.PossessorID == PlayerID && f.IsLeader && f.Pos.HasValue).Count() == 0)
            {
                score -= 100000;
            }

            // ���������̎�Ńt�����Y�𑽐�����΍��X�R�A
            foreach (var Friend in myFriendsOnBoard)
            {
                foreach(var Movement in Friend.Friend.NormalMoveMap)
                {
                    var to_pos = RotationDirectionUtil.GetAbsolutePos(Friend.Pos.Value, Friend.Dir, Movement);
                    var friend = GetFriendInformationAt(to_pos);
                    if (friend.HasValue && friend.Value.PossessorID!=PlayerID)
                    {
                        if (friend.Value.IsLeader)
                        {
                            // �叫������Ȃ獂�X�R�A
                            score += 100;
                        }
                        else
                        {
                            // �叫�łȂ��Ă�����Ȃ�X�R�A�A�b�v
                            score += 2;
                        }
                    }
                }
                foreach(var Skill in Friend.Friend.Skills)
                {
                    if(SandstarAmounts[PlayerID] < Skill.NeededSandstar)
                    {
                        // �T���h�X�^�[�s���̂��߃X�L�b�v
                        continue;
                    }

                    foreach(var to_pos_relative in Skill.Pos)
                    {
                        var to_pos = RotationDirectionUtil.GetAbsolutePos(Friend.Pos.Value, Friend.Dir, to_pos_relative);
                        SkillSimulationResult res = Friend.Friend.SimulateSkill(this, Friend.Pos.Value, to_pos, manager);
                        if (res.CanUseSkill)
                        {
                            score += res.AimedPos
                                .Select((p) => GetFriendInformationAt(p))
                                .Where((fi) => fi.HasValue)
                                .Select((fi) =>
                                {
                                    var value = fi.Value.IsLeader ? 100:2;
                                    var pm = fi.Value.PossessorID == PlayerID ? -1 : 1;
                                    return value * pm;
                                })
                                .Sum();
                        }
                    }
                }
            }

            return score;
        }

        public Snapshot SimulatePlaceFriend(int PlayerID, Friend friend, Vector2Int pos, GameManager manager)
        {
            var friends = Friends.ToArray();
            for(var i = 0; i < friends.Length; i++)
            {
                var f = friends[i];
                if (f.PossessorID != PlayerID) { continue; }
                if (f.Pos != null) { continue; }
                if (f.Friend != friend) { continue; }

                friends[i].Pos = pos;
                break;
            }
            return new Snapshot(PlayerInTurnID, SandstarAmounts, friends);
        }
    }

    public struct SnapshotFriend
    {
        public int PossessorID; // -1�ŃZ�����A��
        public Friend Friend;
        public RotationDirection Dir;
        public Vector2Int? Pos; // ��u����̃t�����Y��Pos=null
        public bool IsLeader;

        public override string ToString()
        {
            if (Pos.HasValue)
            {
                return Friend.Name + " @"+Pos.Value+" Possessor=" + PossessorID;
            }
            else
            {
                return Friend.Name + " @<Lounge> Possessor="+PossessorID;
            }
        }
    }
}