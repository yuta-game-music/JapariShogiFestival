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
                // CPU以外でCPUBehaviourを使おうとした
                throw new System.Exception("Not a CPU!");
            }

            CPUStrategy strategy = Player.PlayerInfo?.CPUStrategy ?? new CPUStrategy();

            // 評価値の計算を行うものは全く別の処理のため別関数にて定義
            if(strategy.Overall == CPUStrategyOverall.BestEvaluation)
            {
                yield return ExecForEvaluation(GameManager, PlayerID);
                yield break;
            }

            FriendOnBoard being_moved_friend = null;

            // 味方フレンズの情報収集と相手の動きうる範囲の計算

            // 味方フレンズの一覧
            HashSet<FriendOnBoard> our_friends = new HashSet<FriendOnBoard>();
            // 相手の攻撃が命中するセル範囲の検索(key=場所、value=相手の攻撃フレンズ)
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
                        // 通常移動
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
                        // スキル
                        // TODO：スキル巻き添え検知
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

            // 守り戦略
            bool defensing = false;
            if (strategy.Defense != CPUStrategyDefense.None)
            {
                FriendOnBoard defendee = Player.Leader;
                if (strategy.DefenseFor == CPUStrategyDefenseFor.All
                    && !opponent_movable_pos.TryGetValue(Player.Leader.Pos.Value, out var aim_tmp))
                {
                    // リーダーが狙われていない、かつ他のフレンズが狙われているとき
                    // 一番多くのフレンズから狙われているフレンズを対象にする
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

            // 攻め戦略

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
                            // 使い切ったので待機
                            GameManager.StartCoroutine(GameManager.SkipTurn());
                            yield break;
                        }
                        tryMove = true;
                        break;
                    case CPUStrategyOverall.MoveOnly100:
                        // 先にforループを抜けてしまうのでここで抜けることはない
                        tryMove = true;
                        break;
                    case CPUStrategyOverall.TryLoungeHalf:
                        // 50%の確率で駒置き場のフレンズを出す、その際に置く場所をランダムに決め、置けなければ再度50%の抽選から 外れたら移動試行、それも5回ダメなら待機
                        if (Random.Range(0, 2) == 0)
                        {
                            tryLounge = true;
                        }
                        else
                        {
                            if (trial >= 5)
                            {
                                // 使い切ったので待機
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
                        // 駒置き場にフレンズがいたらランダムに置き場を決定 50回まで試行しダメなら配置
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
                    // 駒置き場のフレンズを出すよう努力
                    if (Player.SandstarAmount < GlobalVariable.NeededSandstarForPlacingNewFriend)
                    {
                        // サンドスター不足
                        continue;
                    }
                    int lounge_size = Player.LoungeSize;
                    if (lounge_size == 0) {
                        // 駒置き場にフレンズがいない
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
                    // 駒を動かすよう努力
                    #region CommonValue
                    Player.Player opponent = GameManager.Players[(PlayerID + 2) % GameManager.Players.Length];
                    Vector2Int opponentPos = opponent.Leader.Pos ?? Vector2Int.zero;
                    if (!opponent.Leader.Pos.HasValue)
                    {
                        // 相手のリーダーが盤面上にいないという状況なので本来はありえない
                        throw new System.Exception("Invalid leader position (player " + opponent + ")!");
                    }
                    FriendOnBoard[] OpponentFriendsOnBoard = GameManager.Map.Values
                        .Select((cell) => cell.Friends)
                        .Where((friend) => friend != null && friend.Possessor == opponent)
                        .ToArray();
                    #endregion

                    #region FriendsSelection
                    // フレンズ選択
                    // 既に守り部分で選択されていればスキップ
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
                                    // 最適解で移動法が見つからなかった場合に最適解以外を使うための変数
                                    int internal_trial = 0;
                                    for (var r = 1; r < 10; r++)
                                    {
                                        for (var pii = 0; pii < r * 8; pii++)
                                        {
                                            // 直線向きが最も最初に確認されるべき
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
                                    // 最適解で移動法が見つからなかった場合に最適解以外を使うための変数
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
                        // 見つからなかったらやり直し
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
                                // 通常移動
                                Vector2Int[] moveMap = being_moved_friend.Friend.NormalMoveMap;
                                System.Array.Sort(moveMap, (a, b) => Random.Range(-1, 1));
                                for(var mi = 0; mi < moveMap.Length; mi++)
                                {
                                    if(GameManager.Map.TryGetValue(RotationDirectionUtil.GetRotatedVector(moveMap[mi],being_moved_friend.Dir)+being_moved_friend.Pos.Value, out var cell))
                                    {
                                        if(cell.Friends && cell.Friends == Player.Leader)
                                        {
                                            // 自分のリーダーを狩ろうとしたため中止
                                            continue;
                                        }
                                        if(strategy.Move==CPUStrategyMove.RandomExceptMinus && cell.Friends?.Possessor == Player)
                                        {
                                            // RandomExceptMinusのときに自分のフレンズを狩ろうとしたため中止
                                            continue;
                                        }
                                        if (cell.RotationOnly)
                                        {
                                            // 回転のみのセルに移動しようとした
                                            continue;
                                        }
                                        if(opponent_movable_pos.TryGetValue(cell.SelfPos, out _))
                                        {
                                            // あえて取られる場所に移動しようとした
                                            continue;
                                        }
                                        GameManager.StartCoroutine(GameManager.MoveFriendWithAnimation(being_moved_friend, cell,true));
                                        yield break;
                                    }
                                }
                            }
                            else if(strategyDice < 0.7f)
                            {
                                // 回転移動
                                // 守り行動時には中止
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
                                // スキル使用
                                SkillMap[] skillMaps = being_moved_friend.Friend.Skills;
                                System.Array.Sort(skillMaps, (a, b) => Random.Range(-1, 1));
                                foreach(var skillMap in skillMaps)
                                {
                                    if(skillMap.NeededSandstar > Player.SandstarAmount)
                                    {
                                        // サンドスター不足により中止
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
                                                    // 自分のリーダーを狩ろうとしたため中止
                                                    continue;
                                                }
                                                if (strategy.Move == CPUStrategyMove.RandomExceptMinus && info.GettingFriends.Any((fob) => fob.Possessor == Player))
                                                {
                                                    // RandomExceptMinusのときに自分のフレンズを狩ろうとしたため中止
                                                    continue;
                                                }
                                                if (opponent_movable_pos.TryGetValue(info.LastPos, out var attackers))
                                                {
                                                    // あえて取られる場所に移動しようとした
                                                    // 自身のスキル使用中に駒置き場に行くフレンズはチェック対象外
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
                                // 通常移動
                                Vector2Int[] moveMap = being_moved_friend.Friend.NormalMoveMap;
                                for (var mi = 0; mi < moveMap.Length; mi++)
                                {
                                    if (GameManager.Map.TryGetValue(RotationDirectionUtil.GetAbsolutePos(being_moved_friend.Pos.Value, being_moved_friend.Dir, moveMap[mi]), out var cell))
                                    {
                                        if (cell.Friends && cell.Friends == Player.Leader)
                                        {
                                            // 自分のリーダーを狩ろうとしたため中止
                                            continue;
                                        }
                                        if (cell.RotationOnly)
                                        {
                                            // 回転のみのセルに移動しようとした
                                            continue;
                                        }
                                        if (opponent_movable_pos.TryGetValue(cell.SelfPos, out _))
                                        {
                                            // あえて取られる場所に移動しようとした
                                            continue;
                                        }
                                        float now_dist;
                                        if(strategy.Move == CPUStrategyMove.ApproachToAnyone)
                                        {
                                            now_dist = OpponentFriendsOnBoard.Select((fob) => (fob.Pos.Value - cell.SelfPos).magnitude).Min();
                                        }
                                        else
                                        {
                                            // 相手のリーダーのみ
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
                                // スキル使用
                                SkillMap[] skillMaps = being_moved_friend.Friend.Skills;
                                System.Array.Sort(skillMaps, (a, b) => Random.Range(-1, 1));
                                foreach (var skillMap in skillMaps)
                                {
                                    if (skillMap.NeededSandstar > Player.SandstarAmount)
                                    {
                                        // サンドスター不足により中止
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
                                                    // 自分のリーダーを狩ろうとしたため中止
                                                    continue;
                                                }
                                                if (opponent_movable_pos.TryGetValue(info.LastPos, out var attackers))
                                                {
                                                    // あえて取られる場所に移動しようとした
                                                    // 自身のスキル使用中に駒置き場に行くフレンズはチェック対象外
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
                                                    // 相手のリーダーのみ
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
                                // 移動先が見つからなかった：回転

                                // 守り状態なら回転などしていられないので中止
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
                            // 定義されていない動作
                            Debug.LogError("Unknown strategy: "+strategy.Move);
                            break;
                    }
                    #endregion
                }
            }
            // 戦略がなくなったので待機
            GameManager.StartCoroutine(GameManager.SkipTurn());
            yield break;
        }

        private static IEnumerator ExecForEvaluation(GameManager GameManager, int PlayerID)
        {
            var snapshot_tmp = new Snapshot(GameManager);
            // 予め他のプレイヤーのサンドスター量を増やしておく
            var snapshot = new Snapshot(
                snapshot_tmp.PlayerInTurnID,
                snapshot_tmp.SandstarAmounts.Select((v,id)=>((id%2==0&&id!=PlayerID) ? v+GlobalVariable.GettingSandstarPerTurn : v)).ToArray(),
                snapshot_tmp.Friends);
            var myFriendsOnBoard = snapshot.Friends.Where((f) => f.PossessorID == PlayerID && f.Pos.HasValue);
            var myFriendsOnLobby = snapshot.Friends.Where((f) => f.PossessorID == PlayerID && !f.Pos.HasValue);

            // 自分の持つ手について検証
            // 初期値は待機時のスコア
            float best_score = snapshot_tmp.GetEvaluation(PlayerID,GameManager)-10;
            Vector2Int? best_pos_from = null;
            Vector2Int? best_pos_to = null;
            RotationDirection? best_dir = null;
            bool isSkill = false;

            // 既に盤面にあるフレンズを動かす場合
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
                        // サンドスター不足のためスキップ
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
                // 負荷対策
                yield return new WaitForEndOfFrame();
            }

            // 新たに盤面にフレンズを出す場合
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

            // 実際に行動
            FriendOnBoard fob = (best_pos_from.HasValue && GameManager.Map.TryGetValue(best_pos_from.Value, out _))?GameManager.Map[best_pos_from.Value].Friends : null;
            Cell cell_to = (best_pos_to.HasValue && GameManager.Map.TryGetValue(best_pos_to.Value, out _)) ? GameManager.Map[best_pos_to.Value] : null;
            Debug.Log($"Pattern: {best_pos_from}->{best_pos_to}[{best_dir}] (skill={isSkill}) :Score={best_score}");
            if (best_pos_from != null)
            {
                if (best_pos_to != null)
                {
                    if (!isSkill)
                    {
                        // 通常移動
                        GameManager.StartCoroutine(GameManager.MoveFriendWithAnimation(fob, cell_to, true));
                        yield break;
                    }
                    else
                    {
                        // スキル発動
                        GameManager.StartCoroutine(GameManager.UseSkill(fob, cell_to));
                        yield break;
                    }
                }
                else
                {
                    if (best_dir.HasValue)
                    {
                        // その場回転
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
                    // 配置
                    FriendOnBoard fob_placing = GameManager.Players[PlayerID].GetFriendsOnLoungeById(Random.Range(0,GameManager.Players[PlayerID].LoungeSize));
                    GameManager.StartCoroutine(GameManager.PlaceFriendFromLounge(fob_placing, cell_to));
                    yield break;
                }
                else
                {
                    // 待機
                    GameManager.StartCoroutine(GameManager.SkipTurn());
                    yield break;
                }
            }
            Debug.LogError("No such pattern!");
            // 待機
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
    // 全体の行動
    public enum CPUStrategyOverall
    {
        MoveOnly3, // 駒置き場のフレンズは一切出さず基本的に移動 3回試行してどれもダメなら待機
        MoveOnly100, // 駒置き場のフレンズは一切出さず基本的に移動 100回試行してどれもダメなら待機
        TryLoungeHalf, // 50%の確率で駒置き場のフレンズを出す、その際に置く場所をランダムに決め、置けなければ再度50%の抽選から 外れたら移動試行、それも5回ダメなら待機
        TryLounge50, // 駒置き場にフレンズがいたらランダムに置き場を決定 50回まで試行しダメなら出さない
        BestEvaluation, // 評価が最も高くなる行動をする
    }
    public enum CPUStrategyDefense
    {
        None, // 守る行動を一切しない
        AlwaysEscape, // 逃げるのみ(逃げられなかったら守る行動をしない)
        RemoveAttacker, // 攻撃してくるフレンズを駒置き場に送るよう努力する
        RemoveAttackerWithDraw, // ↑のためなら引き分けになっても構わない
    }
    public enum CPUStrategyDefenseFor
    {
        LeaderOnly, // リーダーが狙われているときのみ対処
        All // 誰かが狙われていたら対処
    }
    // 動かすフレンズの選び方
    public enum CPUStrategySelect
    {
        Random, // 完全ランダム
        NearestToLeader, // 相手のリーダーに最も近い味方
        Backline, // 後ろにいるフレンズを優先(確率を高めに設定)
    }
    // フレンズの動かし方
    public enum CPUStrategyMove
    {
        Random, // 完全ランダムな行動 味方が狩られようがお構いなし(リーダーが狩られる場合を除く)
        RandomExceptMinus, // ランダムだが味方は狩らない
        ApproachToAnyone, // 相手の誰かに最も近づく行動をする
        ApproachToLeader, // 相手のリーダーに最も近づく行動をする
    }


    public enum CPUDifficulty
    {
        Easy, // Overall=MoveOnly100, Defense=None, DefenseFor=LeaderOnly, Select=Random, Move=Random
        Normal, // Overall=TryLounge50, Defense=AlwaysEscape, DefenseFor=All, Select=Backline, Move=ApproachToLeader
        Hard, // Overall=BestEvaluation
    }
}