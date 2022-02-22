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
            // ��x�Տ�̃t�����Y��S������

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
                // �T���h�X�^�[�ʂ͂����Ŗ߂��Ă��܂�
                p.SandstarAmount = sandstarAmounts[p.PlayerInfo?.ID ?? 0];

                // ���[�_�[���������ŏ���(�Ĕz�u�̍ۂɓ�d�z�u�G���[���o������)
                p.Leader = null;
            }

            // �Ĕz�u
            foreach (var f in Friends)
            {
                if (f.Pos.HasValue)
                {
                    // �Տ�ɒu��
                    manager.PlaceFriend(f.Pos.Value, f.RotationDirection, f.friend, manager.Players[2 * f.PossessorID], f.IsLeader);
                }
                else
                {
                    // �����t�����Y�ɒu��
                    manager.PlaceFriendAtLounge(f.friend, manager.Players[2 * f.PossessorID]);
                }
            }
        }
    }

    public struct SnapshotFriend
    {
        public int PossessorID; // -1�ŃZ�����A��
        public Friend friend;
        public RotationDirection RotationDirection;
        public Vector2Int? Pos; // ��u����̃t�����Y��Pos=null
        public bool IsLeader;
    }
}