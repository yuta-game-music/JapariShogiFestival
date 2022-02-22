using JSF.Database;
using JSF.Game.Board;
using JSF.Game.Logger;
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

            // �]���l�̌v�Z���s�����̂͑S���ʂ̏����̂��ߕʊ֐��ɂĒ�`
            if(strategy.Overall == CPUStrategyOverall.BestEvaluation)
            {
                yield return ExecForEvaluation(GameManager, PlayerID);
                yield break;
            }

            FriendOnBoard being_moved_friend = null;

            // �����t�����Y�̏����W�Ƒ���̓�������͈͂̌v�Z

            // �����t�����Y�̈ꗗ
            HashSet<FriendOnBoard> our_friends = new HashSet<FriendOnBoard>();
            // ����̍U������������Z���͈͂̌���(key=�ꏊ�Avalue=����̍U���t�����Y)
            Dictionary<Vector2Int, HashSet<FriendOnBoard>> opponent_movable_pos = new Dictionary<Vector2Int, HashSet<FriendOnBoard>>();

            foreach(Cell cell in GameManager.Map.Values)
            {
                if (cell.Friends && cell.Friends.Possessor)
                {
                    if (cell.Friends.Possessor == GameManager.Players[PlayerID])
                    {
                        our_friends.Add(cell.Friends);
                    }
                    else
                    {
                        FriendOnBoard friends_on_board = cell.Friends;
                        Friend friend = friends_on_board.Friend;
                        // �ʏ�ړ�
                        for (var i = 0; i < friend.NormalMoveMap.Length; i++)
                        {
                            var p = RotationDirectionUtil.GetAbsolutePos(friends_on_board.Pos.Value, friends_on_board.Dir, friend.NormalMoveMap[i]);
                            if (GameManager.Map.TryGetValue(p, out Cell pcell))
                            {
                                if (opponent_movable_pos.TryGetValue(p, out var v))
                                {
                                    v.Add(friends_on_board);
                                }
                                else
                                {
                                    opponent_movable_pos.Add(p, new HashSet<FriendOnBoard>() { friends_on_board });
                                }
                            }
                        }
                        // �X�L��
                        // TODO�F�X�L�������Y�����m
                        SkillMap[] skill_maps = friend.Skills;
                        foreach (var skill_map in skill_maps)
                        {
                            if (Player.SandstarAmount < skill_map.NeededSandstar) { continue; }
                            foreach (var pos in skill_map.Pos)
                            {
                                var abs_pos = RotationDirectionUtil.GetAbsolutePos(friends_on_board.Pos.Value, friends_on_board.Dir, pos);
                                var result = friend.SimulateSkill(abs_pos, friends_on_board, GameManager);
                                if (result.CanUseSkill)
                                {
                                    foreach (var p in result.AimedPos)
                                    {
                                        if (GameManager.Map.TryGetValue(p, out Cell pcell))
                                        {

                                            if (opponent_movable_pos.TryGetValue(p, out var v))
                                            {
                                                v.Add(friends_on_board);
                                            }
                                            else
                                            {
                                                opponent_movable_pos.Add(p, new HashSet<FriendOnBoard>() { friends_on_board });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // ���헪
            bool defensing = false;
            if (strategy.Defense != CPUStrategyDefense.None)
            {
                FriendOnBoard defendee = Player.Leader;
                if (strategy.DefenseFor == CPUStrategyDefenseFor.All
                    && !opponent_movable_pos.TryGetValue(Player.Leader.Pos.Value, out var aim_tmp))
                {
                    // ���[�_�[���_���Ă��Ȃ��A�����̃t�����Y���_���Ă���Ƃ�
                    // ��ԑ����̃t�����Y����_���Ă���t�����Y��Ώۂɂ���
                    int max_aimed_by_count = 0;
                    foreach(var fob in our_friends)
                    {
                        if(opponent_movable_pos.TryGetValue(fob.Pos.Value, out var set)){
                            var _cnt = set.Count;
                            if (_cnt > max_aimed_by_count)
                            {
                                defendee = fob;
                                max_aimed_by_count = _cnt;
                            }
                        }
                    }
                }
                if(opponent_movable_pos.TryGetValue(defendee.Pos.Value, out var aimed))
                {
                    defensing = true;
                    being_moved_friend = defendee;
                }
            }

            // �U�ߐ헪

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
                    // �t�����Y�I��
                    // ���Ɏ�蕔���őI������Ă���΃X�L�b�v
                    if (being_moved_friend == null || !being_moved_friend.Pos.HasValue)
                    {
                        switch (strategy.Select)
                        {
                            case CPUStrategySelect.Random:
                                for (var t = 0; t < 100; t++)
                                {
                                    Vector2Int _pos = new Vector2Int(
                                        Random.Range(0, GameManager.BoardRenderer.W),
                                        Random.Range(0, GameManager.BoardRenderer.H)
                                    );
                                    if (GameManager.Map.TryGetValue(_pos, out var cell))
                                    {
                                        if (cell.Friends)
                                        {
                                            if (cell.Friends.Possessor == GameManager.PlayerInTurn)
                                            {
                                                being_moved_friend = cell.Friends;
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
                                            int pi = (pii % 4) * 2 * r + (1 - ((pii / 4) % 2) * 2) * ((pii + 4) / 8);
                                            Vector2Int _pos = opponentPos
                                                + RotationDirectionUtil.GetRotatedVector(PositionUtil.CalcVectorFromCirclePos(r, pi), opponent.Direction);
                                            if (GameManager.Map.TryGetValue(_pos, out var cell))
                                            {
                                                if (cell.Friends)
                                                {
                                                    if (cell.Friends.Possessor == GameManager.PlayerInTurn)
                                                    {
                                                        if (internal_trial >= MoveTrial - 1)
                                                        {
                                                            being_moved_friend = cell.Friends;
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
                                        if (being_moved_friend != null) { break; }
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
                                                            being_moved_friend = cell.Friends;
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
                                        if (being_moved_friend != null)
                                        {
                                            break;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    if (being_moved_friend == null || !being_moved_friend.Pos.HasValue)
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
                                Vector2Int[] moveMap = being_moved_friend.Friend.NormalMoveMap;
                                System.Array.Sort(moveMap, (a, b) => Random.Range(-1, 1));
                                for(var mi = 0; mi < moveMap.Length; mi++)
                                {
                                    if(GameManager.Map.TryGetValue(RotationDirectionUtil.GetRotatedVector(moveMap[mi],being_moved_friend.Dir)+being_moved_friend.Pos.Value, out var cell))
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
                                        if(opponent_movable_pos.TryGetValue(cell.SelfPos, out _))
                                        {
                                            // �����Ď����ꏊ�Ɉړ����悤�Ƃ���
                                            continue;
                                        }
                                        GameManager.StartCoroutine(GameManager.MoveFriendWithAnimation(being_moved_friend, cell,true));
                                        yield break;
                                    }
                                }
                            }
                            else if(strategyDice < 0.7f)
                            {
                                // ��]�ړ�
                                // ���s�����ɂ͒��~
                                if (defensing) { continue; }
                                Debug.Log("Rotation");
                                RotationDirection[] dirs = being_moved_friend.Friend.NormalRotationMap;
                                RotationDirection dir = dirs[Random.Range(0, dirs.Length)];
                                GameManager.StartCoroutine(GameManager.MoveFriend(
                                    being_moved_friend,
                                    being_moved_friend.Cell,
                                    RotationDirectionUtil.Merge(being_moved_friend.Dir, dir),
                                    true));
                                yield break;
                            }
                            else
                            {
                                // �X�L���g�p
                                SkillMap[] skillMaps = being_moved_friend.Friend.Skills;
                                System.Array.Sort(skillMaps, (a, b) => Random.Range(-1, 1));
                                foreach(var skillMap in skillMaps)
                                {
                                    if(skillMap.NeededSandstar > Player.SandstarAmount)
                                    {
                                        // �T���h�X�^�[�s���ɂ�蒆�~
                                        continue;
                                    }

                                    Vector2Int[] poses = skillMap.Pos.Select((p)=>RotationDirectionUtil.GetRotatedVector(p,being_moved_friend.Dir)).ToArray();
                                    System.Array.Sort(poses, (a, b) => Random.Range(-1, 1));
                                    foreach(var pos in poses)
                                    {
                                        if(GameManager.Map.TryGetValue(pos + being_moved_friend.Pos.Value, out var cell))
                                        {
                                            var info = being_moved_friend.Friend.SimulateSkill(
                                                cell.SelfPos,
                                                being_moved_friend,
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
                                                if (opponent_movable_pos.TryGetValue(info.LastPos, out var attackers))
                                                {
                                                    // �����Ď����ꏊ�Ɉړ����悤�Ƃ���
                                                    // ���g�̃X�L���g�p���ɋ�u����ɍs���t�����Y�̓`�F�b�N�ΏۊO
                                                    var copied_attackers = new HashSet<FriendOnBoard>(attackers);
                                                    copied_attackers.RemoveWhere((fob) => {
                                                        return info.AimedPos.Any((fob2)=> fob.Pos==fob2); });
                                                    if (copied_attackers.Count > 0)
                                                    {
                                                        continue;
                                                    }
                                                }
                                                GameManager.StartCoroutine(GameManager.UseSkill(being_moved_friend, cell));
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
                                Vector2Int[] moveMap = being_moved_friend.Friend.NormalMoveMap;
                                for (var mi = 0; mi < moveMap.Length; mi++)
                                {
                                    if (GameManager.Map.TryGetValue(RotationDirectionUtil.GetAbsolutePos(being_moved_friend.Pos.Value, being_moved_friend.Dir, moveMap[mi]), out var cell))
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
                                        if (opponent_movable_pos.TryGetValue(cell.SelfPos, out _))
                                        {
                                            // �����Ď����ꏊ�Ɉړ����悤�Ƃ���
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
                                SkillMap[] skillMaps = being_moved_friend.Friend.Skills;
                                System.Array.Sort(skillMaps, (a, b) => Random.Range(-1, 1));
                                foreach (var skillMap in skillMaps)
                                {
                                    if (skillMap.NeededSandstar > Player.SandstarAmount)
                                    {
                                        // �T���h�X�^�[�s���ɂ�蒆�~
                                        continue;
                                    }

                                    Vector2Int[] poses = skillMap.Pos.Select((p) => RotationDirectionUtil.GetRotatedVector(p, being_moved_friend.Dir)).ToArray();
                                    System.Array.Sort(poses, (a, b) => Random.Range(-1, 1));
                                    foreach (var pos in poses)
                                    {
                                        if (GameManager.Map.TryGetValue(pos + being_moved_friend.Pos.Value, out var cell))
                                        {
                                            var info = being_moved_friend.Friend.SimulateSkill(
                                                cell.SelfPos,
                                                being_moved_friend,
                                                GameManager);
                                            if (info.CanUseSkill)
                                            {
                                                if (info.GettingFriends.Any((fob) => fob == Player.Leader))
                                                {
                                                    // �����̃��[�_�[����낤�Ƃ������ߒ��~
                                                    continue;
                                                }
                                                if (opponent_movable_pos.TryGetValue(info.LastPos, out var attackers))
                                                {
                                                    // �����Ď����ꏊ�Ɉړ����悤�Ƃ���
                                                    // ���g�̃X�L���g�p���ɋ�u����ɍs���t�����Y�̓`�F�b�N�ΏۊO
                                                    var copied_attackers = new HashSet<FriendOnBoard>(attackers);
                                                    copied_attackers.RemoveWhere((fob) => {
                                                        return info.AimedPos.Any((fob2) => fob.Pos == fob2);
                                                    });
                                                    if (copied_attackers.Count > 0)
                                                    {
                                                        continue;
                                                    }
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
                                    GameManager.StartCoroutine(GameManager.MoveFriendWithAnimation(being_moved_friend, min_pos,true));
                                    yield break;
                                }
                                else
                                {
                                    GameManager.StartCoroutine(GameManager.UseSkill(being_moved_friend, min_pos));
                                    yield break;
                                }
                            }
                            else
                            {
                                // �ړ��悪������Ȃ������F��]

                                // ����ԂȂ��]�Ȃǂ��Ă����Ȃ��̂Œ��~
                                if (defensing) { continue; }
                                RotationDirection[] dirs = being_moved_friend.Friend.NormalRotationMap;
                                RotationDirection dir = dirs[Random.Range(0, dirs.Length)];
                                GameManager.StartCoroutine(GameManager.MoveFriend(
                                    being_moved_friend,
                                    being_moved_friend.Cell,
                                    RotationDirectionUtil.Merge(being_moved_friend.Dir, dir),
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

        private static IEnumerator ExecForEvaluation(GameManager GameManager, int PlayerID)
        {
            var snapshot_tmp = new Snapshot(GameManager);
            // �\�ߑ��̃v���C���[�̃T���h�X�^�[�ʂ𑝂₵�Ă���
            var snapshot = new Snapshot(
                snapshot_tmp.PlayerInTurnID,
                snapshot_tmp.SandstarAmounts.Select((v,id)=>((id%2==0&&id!=PlayerID) ? v+GlobalVariable.GettingSandstarPerTurn : v)).ToArray(),
                snapshot_tmp.Friends);
            var myFriendsOnBoard = snapshot.Friends.Where((f) => f.PossessorID == PlayerID && f.Pos.HasValue);
            var myFriendsOnLobby = snapshot.Friends.Where((f) => f.PossessorID == PlayerID && !f.Pos.HasValue);

            // �����̎���ɂ��Č���
            // �����l�͑ҋ@���̃X�R�A
            float best_score = snapshot_tmp.GetEvaluation(PlayerID,GameManager)-10;
            Vector2Int? best_pos_from = null;
            Vector2Int? best_pos_to = null;
            RotationDirection? best_dir = null;
            bool isSkill = false;

            // ���ɔՖʂɂ���t�����Y�𓮂����ꍇ
            foreach (var Friend in myFriendsOnBoard)
            {
                foreach (var Movement in Friend.Friend.NormalMoveMap)
                {
                    var to_pos = RotationDirectionUtil.GetAbsolutePos(Friend.Pos.Value, Friend.Dir, Movement);
                    var simulated_result = Friend.Friend.SimulateNormalMove(snapshot, Friend.Pos.Value, to_pos, GameManager);
                    if (simulated_result != null)
                    {
                        var score = simulated_result.GetEvaluation(PlayerID, GameManager);
                        if (score > best_score)
                        {
                            best_score = score;
                            best_pos_from = Friend.Pos.Value;
                            best_pos_to = to_pos;
                            best_dir = null;
                            isSkill = false;
                        }
                    }
                }
                foreach(var Rotation in Friend.Friend.NormalRotationMap)
                {
                    var to_dir = RotationDirectionUtil.Merge(Friend.Dir, Rotation);
                    var simulated_result = Friend.Friend.SimulateRotation(snapshot, Friend.Pos.Value, to_dir, GameManager);
                    if (simulated_result != null)
                    {
                        var score = simulated_result.GetEvaluation(PlayerID, GameManager);
                        if (score > best_score)
                        {
                            best_score = score;
                            best_pos_from = Friend.Pos.Value;
                            best_pos_to = null;
                            best_dir = to_dir;
                            isSkill = false;
                        }
                    }
                }
                foreach (var Skill in Friend.Friend.Skills)
                {
                    if (snapshot.SandstarAmounts[PlayerID] < Skill.NeededSandstar)
                    {
                        // �T���h�X�^�[�s���̂��߃X�L�b�v
                        continue;
                    }

                    foreach (var to_pos_relative in Skill.Pos)
                    {
                        var to_pos = RotationDirectionUtil.GetAbsolutePos(Friend.Pos.Value, Friend.Dir, to_pos_relative);
                        SkillSimulationResult res = Friend.Friend.SimulateSkill(snapshot, Friend.Pos.Value, to_pos, GameManager);
                        if (res.CanUseSkill)
                        {
                            var score = res.Snapshot.GetEvaluation(PlayerID, GameManager);
                            if (score > best_score)
                            {
                                best_score = score;
                                best_pos_from = Friend.Pos.Value;
                                best_pos_to = to_pos;
                                best_dir = null;
                                isSkill = true;
                            }
                        }
                    }
                }
                // ���ב΍�
                yield return new WaitForEndOfFrame();
            }

            // �V���ɔՖʂɃt�����Y���o���ꍇ
            if (myFriendsOnLobby.Count() > 0 && snapshot.SandstarAmounts[PlayerID]>=GlobalVariable.NeededSandstarForPlacingNewFriend)
            {
                foreach (var cell in GameManager.Players[PlayerID].GetCellsInMyRealm(GameManager))
                {
                    var pos = cell.SelfPos;
                    if (snapshot.GetFriendInformationAt(pos) == null)
                    {
                        var simulated = snapshot.SimulatePlaceFriend(PlayerID, myFriendsOnLobby.First().Friend, pos, GameManager);

                        var score = simulated.GetEvaluation(PlayerID, GameManager);
                        if (score > best_score)
                        {
                            best_score = score;
                            best_pos_from = null;
                            best_pos_to = pos;
                            best_dir = null;
                            isSkill = false;
                        }
                    }
                }
            }

            // ���ۂɍs��
            FriendOnBoard fob = (best_pos_from.HasValue && GameManager.Map.TryGetValue(best_pos_from.Value, out _))?GameManager.Map[best_pos_from.Value].Friends : null;
            Cell cell_to = (best_pos_to.HasValue && GameManager.Map.TryGetValue(best_pos_to.Value, out _)) ? GameManager.Map[best_pos_to.Value] : null;
            Debug.Log($"Pattern: {best_pos_from}->{best_pos_to}[{best_dir}] (skill={isSkill}) :Score={best_score}");
            if (best_pos_from != null)
            {
                if (best_pos_to != null)
                {
                    if (!isSkill)
                    {
                        // �ʏ�ړ�
                        GameManager.StartCoroutine(GameManager.MoveFriendWithAnimation(fob, cell_to, true));
                        yield break;
                    }
                    else
                    {
                        // �X�L������
                        GameManager.StartCoroutine(GameManager.UseSkill(fob, cell_to));
                        yield break;
                    }
                }
                else
                {
                    if (best_dir.HasValue)
                    {
                        // ���̏��]
                        GameManager.StartCoroutine(GameManager.MoveFriend(
                            fob,
                            fob.Cell,
                            best_dir.Value,
                            true));
                        yield break;
                    }
                }
            }
            else
            {
                if (best_pos_to != null)
                {
                    // �z�u
                    FriendOnBoard fob_placing = GameManager.Players[PlayerID].GetFriendsOnLoungeById(Random.Range(0,GameManager.Players[PlayerID].LoungeSize));
                    GameManager.StartCoroutine(GameManager.PlaceFriendFromLounge(fob_placing, cell_to));
                    yield break;
                }
                else
                {
                    // �ҋ@
                    GameManager.StartCoroutine(GameManager.SkipTurn());
                    yield break;
                }
            }
            Debug.LogError("No such pattern!");
            // �ҋ@
            GameManager.StartCoroutine(GameManager.SkipTurn());
        }
    }


    public struct CPUStrategy
    {
        public CPUStrategyOverall Overall;
        public CPUStrategyDefense Defense;
        public CPUStrategyDefenseFor DefenseFor;
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
        BestEvaluation, // �]�����ł������Ȃ�s��������
    }
    public enum CPUStrategyDefense
    {
        None, // ���s������؂��Ȃ�
        AlwaysEscape, // ������̂�(�������Ȃ���������s�������Ȃ�)
        RemoveAttacker, // �U�����Ă���t�����Y����u����ɑ���悤�w�͂���
        RemoveAttackerWithDraw, // ���̂��߂Ȃ���������ɂȂ��Ă��\��Ȃ�
    }
    public enum CPUStrategyDefenseFor
    {
        LeaderOnly, // ���[�_�[���_���Ă���Ƃ��̂ݑΏ�
        All // �N�����_���Ă�����Ώ�
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


    public enum CPUDifficulty
    {
        Easy, // Overall=MoveOnly100, Defense=None, DefenseFor=LeaderOnly, Select=Random, Move=Random
        Normal, // Overall=TryLounge50, Defense=AlwaysEscape, DefenseFor=All, Select=Backline, Move=ApproachToLeader
        Hard, // Overall=BestEvaluation
    }
}