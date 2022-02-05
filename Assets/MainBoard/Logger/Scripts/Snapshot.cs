using JSF.Database;
using JSF.Game.Board;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Game.Logger
{
    public class Snapshot
    {
        int playerInTurnID;
        int[] sandstarAmounts = new int[2];
        SnapshotFriend[] Friends;
        public Snapshot(GameManager manager)
        {
            playerInTurnID = manager.PlayerInTurnID;

            List<SnapshotFriend> Friends = new List<SnapshotFriend>();

            foreach (var v in manager.Map.Keys)
            {
                if (manager.Map[v].Friends)
                {
                    Friends.Add(new SnapshotFriend()
                    {
                        Pos = v,
                        friend = manager.Map[v].Friends.Friend,
                        RotationDirection= manager.Map[v].Friends.Dir,
                        PossessorID = manager.Map[v].Friends.Possessor.PlayerInfo?.ID ?? -1,
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
                        friend = c.Friends.Friend,
                        RotationDirection = RotationDirection.FORWARD,
                        PossessorID = p.PlayerInfo?.ID ?? -1
                    });
                }
                sandstarAmounts[p.PlayerInfo?.ID ?? 0] = p.SandstarAmount;
            }
            this.Friends = Friends.ToArray();
        }

        public void Restore(GameManager manager)
        {
            manager.PlayerInTurnID = playerInTurnID;
            // 一度盤上のフレンズを全員消す

            foreach (var v in manager.Map.Keys)
            {
                if (manager.Map[v].Friends)
                {
                    GameObject.Destroy(manager.Map[v].Friends.gameObject);
                    manager.Map[v].Friends = null;
                }
            }
            foreach (var p in manager.Players)
            {
                if (p.PlayerType == Player.PlayerType.Cellien) { continue; }
                foreach (var c in p.Lounge.GetComponentsInChildren<LoungeCell>())
                {
                    GameObject.Destroy(c.gameObject);
                }
                // サンドスター量はここで戻してしまう
                p.SandstarAmount = sandstarAmounts[p.PlayerInfo?.ID ?? 0];

                // リーダー情報もここで消す(再配置の際に二重配置エラーを出すため)
                p.Leader = null;
            }

            // 再配置
            foreach (var f in Friends)
            {
                if (f.Pos.HasValue)
                {
                    // 盤上に置く
                    manager.PlaceFriend(f.Pos.Value, f.RotationDirection, f.friend, manager.Players[2 * f.PossessorID], f.IsLeader);
                }
                else
                {
                    // 所持フレンズに置く
                    manager.PlaceFriendAtLounge(f.friend, manager.Players[2 * f.PossessorID]);
                }
            }
        }
    }

    public struct SnapshotFriend
    {
        public int PossessorID; // -1でセルリアン
        public Friend friend;
        public RotationDirection RotationDirection;
        public Vector2Int? Pos; // 駒置き場のフレンズはPos=null
        public bool IsLeader;
    }
}