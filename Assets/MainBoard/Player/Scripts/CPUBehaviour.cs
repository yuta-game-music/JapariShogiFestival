using JSF.Game.Board;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JSF.Game.CPU
{
    public static class CPUBehaviour
    {
        public static IEnumerator Exec(GameManager GameManager, int PlayerID)
        {
            var Player = GameManager.Players[PlayerID];
            if (Player.PlayerType != Game.Player.PlayerType.CPU)
            {
                // CPU�ȊO��CPUBehaviour���g�����Ƃ���
                throw new System.Exception("Not a CPU!");
            }

            CPUStrategy strategy = Player.PlayerInfo?.CPUStrategy ?? new CPUStrategy();
            int MoveTrial = 0;
            for(var trial = 0; trial < 100; trial++)
            {
                bool tryMove = false;
                bool tryLounge = false;
                switch (strategy.Overall)
                {
                    case CPUStrategyOverall.MoveOnly3:
                        if (trial >= 3)
                        {
                            // �g���؂����̂őҋ@
                            GameManager.StartCoroutine(GameManager.SkipTurn());
                            yield break;
                        }
                        tryMove = true;
                        break;
                    case CPUStrategyOverall.MoveOnly100:
                        // ���for���[�v�𔲂��Ă��܂��̂ł����Ŕ����邱�Ƃ͂Ȃ�
                        tryMove = true;
                        break;
                    case CPUStrategyOverall.TryLoungeHalf:
                        // 50%�̊m���ŋ�u����̃t�����Y���o���A���̍ۂɒu���ꏊ�������_���Ɍ��߁A�u���Ȃ���΍ēx50%�̒��I���� �O�ꂽ��ړ����s�A�����5��_���Ȃ�ҋ@
                        if (Random.Range(0, 2) == 0)
                        {
                            tryLounge = true;
                        }
                        else
                        {
                            if (trial >= 5)
                            {
                                // �g���؂����̂őҋ@
                                GameManager.StartCoroutine(GameManager.SkipTurn());
                                yield break;
                            }
                            else
                            {
                                tryMove = true;
                            }
                        }
                        break;
                    case CPUStrategyOverall.TryLounge50:
                        // ��u����Ƀt�����Y�������烉���_���ɒu��������� 50��܂Ŏ��s���_���Ȃ�z�u
                        if (trial < 50)
                        {
                            tryLounge = true;
                        }
                        else
                        {
                            tryMove = true;
                        }
                        break;
                }

                if (tryLounge)
                {
                    // ��u����̃t�����Y���o���悤�w��
                    if (Player.SandstarAmount < GlobalVariable.NeededSandstarForPlacingNewFriend)
                    {
                        // �T���h�X�^�[�s��
                        continue;
                    }
                    int lounge_size = Player.LoungeSize;
                    if (lounge_size == 0) {
                        // ��u����Ƀt�����Y�����Ȃ�
                        continue;
                    }
                    FriendOnBoard fob = Player.GetFriendsOnLoungeById(Random.Range(0, lounge_size));
                    Cell[] cells = Player.GetCellsInMyRealm(GameManager);
                    System.Array.Sort(cells, (a, b) => (Random.Range(-1, 1)));
                    foreach(var cell in cells)
                    {
                        if (cell.Friends) { continue; }
                        if (cell.RotationOnly) { continue; }
                        GameManager.StartCoroutine(GameManager.PlaceFriendFromLounge(fob, cell));
                        yield break;
                    }
                }
                if (tryMove)
                {
                    MoveTrial++;
                    // ��𓮂����悤�w��
                    FriendOnBoard selected = null;
                    #region CommonValue
                    Player.Player opponent = GameManager.Players[(PlayerID + 2) % GameManager.Players.Length];
                    Vector2Int opponentPos = opponent.Leader.Pos ?? Vector2Int.zero;
                    if (!opponent.Leader.Pos.HasValue)
                    {
                        // ����̃��[�_�[���Ֆʏ�ɂ��Ȃ��Ƃ����󋵂Ȃ̂Ŗ{���͂��肦�Ȃ�
                        throw new System.Exception("Invalid leader position (player " + opponent + ")!");
                    }
                    FriendOnBoard[] OpponentFriendsOnBoard = GameManager.Map.Values
                        .Select((cell) => cell.Friends)
                        .Where((friend) => friend != null && friend.Possessor == opponent)
                        .ToArray();
                    #endregion

                    #region FriendsSelection
                    switch (strategy.Select)
                    {
                        case CPUStrategySelect.Random:
                            for(var t = 0; t < 100; t++)
                            {
                                Vector2Int _pos = new Vector2Int(
                                    Random.Range(0, GameManager.BoardRenderer.W),
                                    Random.Range(0, GameManager.BoardRenderer.H)
                                );
                                if(GameManager.Map.TryGetValue(_pos, out var cell))
                                {
                                    if (cell.Friends)
                                    {
                                        if(cell.Friends.Possessor == GameManager.PlayerInTurn)
                                        {
                                            selected = cell.Friends;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        case CPUStrategySelect.NearestToLeader:
                            {
                                // �œK���ňړ��@��������Ȃ������ꍇ�ɍœK���ȊO���g�����߂̕ϐ�
                                int internal_trial = 0;
                                for (var r = 1; r < 10; r++)
                                {
                                    for (var pii = 0; pii < r * 8; pii++)
                                    {
                                        // �����������ł��ŏ��Ɋm�F�����ׂ�
                                        int pi = (pii%4)*2*r + (1-((pii/4)%2)*2)*((pii+4)/8);
                                        Vector2Int _pos = opponentPos
                                            + RotationDirectionUtil.GetRotatedVector(PositionUtil.CalcVectorFromCirclePos(r, pi), opponent.Direction);
                                        if (GameManager.Map.TryGetValue(_pos, out var cell))
                                        {
                                            if (cell.Friends)
                                            {
                                                if (cell.Friends.Possessor == GameManager.PlayerInTurn)
                                                {
                                                    if (internal_trial >= MoveTrial-1)
                                                    {
                                                        selected = cell.Friends;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        internal_trial++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (selected != null) { break; }
                                }
                            }
                            break;
                        case CPUStrategySelect.Backline:
                            {
                                // �œK���ňړ��@��������Ȃ������ꍇ�ɍœK���ȊO���g�����߂̕ϐ�
                                int internal_trial = 0;
                                for (var l = 0; l < GameManager.BoardRenderer.H; l++)
                                {
                                    Vector2Int[] poses = new Vector2Int[GameManager.BoardRenderer.W];
                                    for (var x = 0; x < poses.Length; x++)
                                    {
                                        poses[x] = new Vector2Int(x,
                                            (Player.Direction == RotationDirection.FORWARD) ?
                                            l
                                            : (GameManager.BoardRenderer.H - l - 1)
                                        );
                                    }
                                    System.Array.Sort(poses, (a, b) => Random.Range(-1, 1));
                                    for (var xi = 0; xi < poses.Length; xi++)
                                    {
                                        if (GameManager.Map.TryGetValue(poses[xi], out var cell))
                                        {
                                            if (cell.Friends)
                                            {
                                                if (cell.Friends.Possessor == Player)
                                                {
                                                    if (internal_trial >= MoveTrial - 1)
                                                    {
                                                        selected = cell.Friends;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        internal_trial++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (selected != null)
                                    {
                                        break;
                                    }
                                }
                            }
                            break;
                    }
                    if (selected == null || !selected.Pos.HasValue)
                    {
                        // ������Ȃ��������蒼��
                        continue;
                    }
                    #endregion

                    #region Movement
                    switch (strategy.Move)
                    {
                        case CPUStrategyMove.Random:
                        case CPUStrategyMove.RandomExceptMinus:
                            float strategyDice = Random.Range(0f, 1f);
                            if (strategyDice < 0.6f)
                            {
                                // �ʏ�ړ�
                                Vector2Int[] moveMap = selected.Friend.NormalMoveMap;
                                System.Array.Sort(moveMap, (a, b) => Random.Range(-1, 1));
                                for(var mi = 0; mi < moveMap.Length; mi++)
                                {
                                    if(GameManager.Map.TryGetValue(RotationDirectionUtil.GetRotatedVector(moveMap[mi],selected.Dir)+selected.Pos.Value, out var cell))
                                    {
                                        if(cell.Friends && cell.Friends == Player.Leader)
                                        {
                                            // �����̃��[�_�[����낤�Ƃ������ߒ��~
                                            continue;
                                        }
                                        if(strategy.Move==CPUStrategyMove.RandomExceptMinus && cell.Friends?.Possessor == Player)
                                        {
                                            // RandomExceptMinus�̂Ƃ��Ɏ����̃t�����Y����낤�Ƃ������ߒ��~
                                            continue;
                                        }
                                        if (cell.RotationOnly)
                                        {
                                            // ��]�݂̂̃Z���Ɉړ����悤�Ƃ���
                                            continue;
                                        }
                                        GameManager.StartCoroutine(GameManager.MoveFriendWithAnimation(selected, cell,true));
                                        yield break;
                                    }
                                }
                            }else if(strategyDice < 0.7f)
                            {
                                // ��]�ړ�
                                Debug.Log("Rotation");
                                RotationDirection[] dirs = selected.Friend.NormalRotationMap;
                                RotationDirection dir = dirs[Random.Range(0, dirs.Length)];
                                GameManager.StartCoroutine(GameManager.MoveFriend(
                                    selected,
                                    selected.Cell,
                                    RotationDirectionUtil.Merge(selected.Dir, dir),
                                    true));
                                yield break;
                            }
                            else
                            {
                                // �X�L���g�p
                                SkillMap[] skillMaps = selected.Friend.Skills;
                                System.Array.Sort(skillMaps, (a, b) => Random.Range(-1, 1));
                                foreach(var skillMap in skillMaps)
                                {
                                    if(skillMap.NeededSandstar > Player.SandstarAmount)
                                    {
                                        // �T���h�X�^�[�s���ɂ�蒆�~
                                        continue;
                                    }

                                    Vector2Int[] poses = skillMap.Pos.Select((p)=>RotationDirectionUtil.GetRotatedVector(p,selected.Dir)).ToArray();
                                    System.Array.Sort(poses, (a, b) => Random.Range(-1, 1));
                                    foreach(var pos in poses)
                                    {
                                        if(GameManager.Map.TryGetValue(pos + selected.Pos.Value, out var cell))
                                        {
                                            var info = selected.Friend.SimulateSkill(
                                                cell.SelfPos,
                                                selected,
                                                GameManager);
                                            if (info.CanUseSkill)
                                            {
                                                if (info.GettingFriends.Any((fob) => fob == Player.Leader))
                                                {
                                                    // �����̃��[�_�[����낤�Ƃ������ߒ��~
                                                    continue;
                                                }
                                                if (strategy.Move == CPUStrategyMove.RandomExceptMinus && info.GettingFriends.Any((fob) => fob.Possessor == Player))
                                                {
                                                    // RandomExceptMinus�̂Ƃ��Ɏ����̃t�����Y����낤�Ƃ������ߒ��~
                                                    continue;
                                                }
                                                GameManager.StartCoroutine(GameManager.UseSkill(selected, cell));
                                                yield break;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case CPUStrategyMove.ApproachToAnyone:
                        case CPUStrategyMove.ApproachToLeader:
                            Cell min_pos = null;
                            float min_dist = float.MaxValue;
                            bool min_is_normal_move = false;
                            {
                                // �ʏ�ړ�
                                Vector2Int[] moveMap = selected.Friend.NormalMoveMap;
                                for (var mi = 0; mi < moveMap.Length; mi++)
                                {
                                    if (GameManager.Map.TryGetValue(RotationDirectionUtil.GetRotatedVector(moveMap[mi], selected.Dir) + selected.Pos.Value, out var cell))
                                    {
                                        if (cell.Friends && cell.Friends == Player.Leader)
                                        {
                                            // �����̃��[�_�[����낤�Ƃ������ߒ��~
                                            continue;
                                        }
                                        if (cell.RotationOnly)
                                        {
                                            // ��]�݂̂̃Z���Ɉړ����悤�Ƃ���
                                            continue;
                                        }
                                        float now_dist;
                                        if(strategy.Move == CPUStrategyMove.ApproachToAnyone)
                                        {
                                            now_dist = OpponentFriendsOnBoard.Select((fob) => (fob.Pos.Value - cell.SelfPos).magnitude).Min();
                                        }
                                        else
                                        {
                                            // ����̃��[�_�[�̂�
                                            now_dist = (opponentPos - cell.SelfPos).magnitude;
                                        }
                                        if (now_dist < min_dist)
                                        {
                                            min_dist = now_dist;
                                            min_is_normal_move = true;
                                            min_pos = cell;
                                        }
                                    }
                                }
                            }
                            {
                                // �X�L���g�p
                                SkillMap[] skillMaps = selected.Friend.Skills;
                                System.Array.Sort(skillMaps, (a, b) => Random.Range(-1, 1));
                                foreach (var skillMap in skillMaps)
                                {
                                    if (skillMap.NeededSandstar > Player.SandstarAmount)
                                    {
                                        // �T���h�X�^�[�s���ɂ�蒆�~
                                        continue;
                                    }

                                    Vector2Int[] poses = skillMap.Pos.Select((p) => RotationDirectionUtil.GetRotatedVector(p, selected.Dir)).ToArray();
                                    System.Array.Sort(poses, (a, b) => Random.Range(-1, 1));
                                    foreach (var pos in poses)
                                    {
                                        if (GameManager.Map.TryGetValue(pos + selected.Pos.Value, out var cell))
                                        {
                                            var info = selected.Friend.SimulateSkill(
                                                cell.SelfPos,
                                                selected,
                                                GameManager);
                                            if (info.CanUseSkill)
                                            {
                                                if (info.GettingFriends.Any((fob) => fob == Player.Leader))
                                                {
                                                    // �����̃��[�_�[����낤�Ƃ������ߒ��~
                                                    continue;
                                                }
                                                float now_dist;
                                                if (strategy.Move == CPUStrategyMove.ApproachToAnyone)
                                                {
                                                    now_dist = OpponentFriendsOnBoard.Select((fob) => (fob.Pos.Value - info.LastPos).magnitude).Min();
                                                }
                                                else
                                                {
                                                    // ����̃��[�_�[�̂�
                                                    now_dist = (opponentPos - info.LastPos).magnitude;
                                                }
                                                if (now_dist < min_dist)
                                                {
                                                    min_dist = now_dist;
                                                    min_is_normal_move = false;
                                                    min_pos = cell;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (min_pos != null)
                            {
                                if (min_is_normal_move)
                                {
                                    GameManager.StartCoroutine(GameManager.MoveFriendWithAnimation(selected, min_pos,true));
                                    yield break;
                                }
                                else
                                {
                                    GameManager.StartCoroutine(GameManager.UseSkill(selected, min_pos));
                                    yield break;
                                }
                            }
                            else
                            {
                                // �ړ��悪������Ȃ������F��]
                                RotationDirection[] dirs = selected.Friend.NormalRotationMap;
                                RotationDirection dir = dirs[Random.Range(0, dirs.Length)];
                                GameManager.StartCoroutine(GameManager.MoveFriend(
                                    selected,
                                    selected.Cell,
                                    RotationDirectionUtil.Merge(selected.Dir, dir),
                                    true));
                                yield break;
                            }
                        default:
                            // ��`����Ă��Ȃ�����
                            Debug.LogError("Unknown strategy: "+strategy.Move);
                            break;
                    }
                    #endregion
                }
            }
            // �헪���Ȃ��Ȃ����̂őҋ@
            GameManager.StartCoroutine(GameManager.SkipTurn());
            yield break;
        }
    }


    public struct CPUStrategy
    {
        public CPUStrategyOverall Overall;
        public CPUStrategySelect Select;
        public CPUStrategyMove Move;
    }
    // �S�̂̍s��
    public enum CPUStrategyOverall
    {
        MoveOnly3, // ��u����̃t�����Y�͈�؏o������{�I�Ɉړ� 3�񎎍s���Ăǂ���_���Ȃ�ҋ@
        MoveOnly100, // ��u����̃t�����Y�͈�؏o������{�I�Ɉړ� 100�񎎍s���Ăǂ���_���Ȃ�ҋ@
        TryLoungeHalf, // 50%�̊m���ŋ�u����̃t�����Y���o���A���̍ۂɒu���ꏊ�������_���Ɍ��߁A�u���Ȃ���΍ēx50%�̒��I���� �O�ꂽ��ړ����s�A�����5��_���Ȃ�ҋ@
        TryLounge50, // ��u����Ƀt�����Y�������烉���_���ɒu��������� 50��܂Ŏ��s���_���Ȃ�o���Ȃ�
    }
    // �������t�����Y�̑I�ѕ�
    public enum CPUStrategySelect
    {
        Random, // ���S�����_��
        NearestToLeader, // ����̃��[�_�[�ɍł��߂�����
        Backline, // ���ɂ���t�����Y��D��(�m�������߂ɐݒ�)
    }
    // �t�����Y�̓�������
    public enum CPUStrategyMove
    {
        Random, // ���S�����_���ȍs�� ����������悤�����\���Ȃ�(���[�_�[�������ꍇ������)
        RandomExceptMinus, // �����_�����������͎��Ȃ�
        ApproachToAnyone, // ����̒N���ɍł��߂Â��s��������
        ApproachToLeader, // ����̃��[�_�[�ɍł��߂Â��s��������
    }

}