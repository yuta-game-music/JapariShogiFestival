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
            // 一度盤上のフレンズを全員消す
            manager.ClearAllFriends();
            foreach (var p in manager.Players)
            {
                // サンドスター量はここで戻してしまう
                p.SandstarAmount = SandstarAmounts[(p.PlayerInfo?.ID * 2) ?? 0];

            }
            // 再配置
            foreach (var f in Friends)
            {
                if (f.Pos.HasValue)
                {
                    // 盤上に置く
                    manager.PlaceFriend(f.Pos.Value, f.Dir, f.Friend, manager.Players[2 * f.PossessorID], f.IsLeader);
                }
                else
                {
                    // 所持フレンズに置く
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
        /// 盤面の評価を返します。
        /// </summary>
        /// <param name="PlayerID">どのプレイヤーについての評価を計算するか(GameManager.PlayersのID)</param>
        /// <returns>盤面の評価結果</returns>
        
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

            // 自分の持つフレンズについて相手の大将に近いほど+向きが大将のほうに向いているほど高スコア
            foreach (var opponentLeader in opponentLeaders)
            {
                score += myFriendsOnBoard
                    .Select((f) =>
                    {
                        PositionUtil.CalcCirclePos((opponentLeader.Pos.Value - f.Pos.Value), out int r, out int rot);
                        int rot_max = r * 8;
                        return 
                            Mathf.Max(5-r,1)*1f // 位置
                            + (1-Mathf.Abs((float)(r+rot_max/2)%rot_max-rot_max/2)/(rot_max/2))*0.3f// 向き(0に近いほうがいい)
                            ;
                    })
                    .Sum();
            }

            // 自分がフレンズを多数持っているほど高スコア
            score += Friends.Where((f) => f.PossessorID == PlayerID).Count() * 5f;

            // 相手が大将を持っていない状態なら最高スコア
            if(Friends.Where((f)=>f.PossessorID!=PlayerID && f.IsLeader && f.Pos.HasValue).Count() == 0)
            {
                score += 10000;
            }
            // 自分が大将を持っていない状態ならスコアをどん底に
            if (Friends.Where((f) => f.PossessorID == PlayerID && f.IsLeader && f.Pos.HasValue).Count() == 0)
            {
                score -= 100000;
            }

            // 自分が次の手でフレンズを多数取れれば高スコア
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
                            // 大将を取れるなら高スコア
                            score += 100;
                        }
                        else
                        {
                            // 大将でなくても取れるならスコアアップ
                            score += 2;
                        }
                    }
                }
                foreach(var Skill in Friend.Friend.Skills)
                {
                    if(SandstarAmounts[PlayerID] < Skill.NeededSandstar)
                    {
                        // サンドスター不足のためスキップ
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
        public int PossessorID; // -1でセルリアン
        public Friend Friend;
        public RotationDirection Dir;
        public Vector2Int? Pos; // 駒置き場のフレンズはPos=null
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