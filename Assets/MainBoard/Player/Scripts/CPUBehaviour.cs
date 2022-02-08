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
                // CPU以外でCPUBehaviourを使おうとした
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
                    FriendOnBoard selected = null;
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
                                // 最適解で移動法が見つからなかった場合に最適解以外を使うための変数
                                int internal_trial = 0;
                                for (var r = 1; r < 10; r++)
                                {
                                    for (var pii = 0; pii < r * 8; pii++)
                                    {
                                        // 直線向きが最も最初に確認されるべき
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
                                Vector2Int[] moveMap = selected.Friend.NormalMoveMap;
                                System.Array.Sort(moveMap, (a, b) => Random.Range(-1, 1));
                                for(var mi = 0; mi < moveMap.Length; mi++)
                                {
                                    if(GameManager.Map.TryGetValue(RotationDirectionUtil.GetRotatedVector(moveMap[mi],selected.Dir)+selected.Pos.Value, out var cell))
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
                                        GameManager.StartCoroutine(GameManager.MoveFriendWithAnimation(selected, cell,true));
                                        yield break;
                                    }
                                }
                            }else if(strategyDice < 0.7f)
                            {
                                // 回転移動
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
                                // スキル使用
                                SkillMap[] skillMaps = selected.Friend.Skills;
                                System.Array.Sort(skillMaps, (a, b) => Random.Range(-1, 1));
                                foreach(var skillMap in skillMaps)
                                {
                                    if(skillMap.NeededSandstar > Player.SandstarAmount)
                                    {
                                        // サンドスター不足により中止
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
                                                    // 自分のリーダーを狩ろうとしたため中止
                                                    continue;
                                                }
                                                if (strategy.Move == CPUStrategyMove.RandomExceptMinus && info.GettingFriends.Any((fob) => fob.Possessor == Player))
                                                {
                                                    // RandomExceptMinusのときに自分のフレンズを狩ろうとしたため中止
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
                                // 通常移動
                                Vector2Int[] moveMap = selected.Friend.NormalMoveMap;
                                for (var mi = 0; mi < moveMap.Length; mi++)
                                {
                                    if (GameManager.Map.TryGetValue(RotationDirectionUtil.GetRotatedVector(moveMap[mi], selected.Dir) + selected.Pos.Value, out var cell))
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
                                SkillMap[] skillMaps = selected.Friend.Skills;
                                System.Array.Sort(skillMaps, (a, b) => Random.Range(-1, 1));
                                foreach (var skillMap in skillMaps)
                                {
                                    if (skillMap.NeededSandstar > Player.SandstarAmount)
                                    {
                                        // サンドスター不足により中止
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
                                                    // 自分のリーダーを狩ろうとしたため中止
                                                    continue;
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
                                // 移動先が見つからなかった：回転
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
    }


    public struct CPUStrategy
    {
        public CPUStrategyOverall Overall;
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

}