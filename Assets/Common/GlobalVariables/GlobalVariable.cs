using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Game.Player;
using JSF.Database;
using JSF.Game;
using JSF.Game.Tutorial;
using JSF.Game.CPU;
using JSF.Common;

namespace JSF
{
    public static class GlobalVariable
    {
        #region DebugInfo
        public static bool DebugMode = true;
        #endregion

        #region Loader
        public static Loader Loader;
        #endregion

        #region Tutorial
        public static Tutorial? Tutorial = null; 
        #endregion

        #region BoardInfo
        public static readonly int MIN_BOARD_W = 3;
        public static readonly int MAX_BOARD_W = 5; // TODO: ボード部分がスクロールできるようになったら10に
        public static readonly int MIN_BOARD_H = 3;
        public static readonly int MAX_BOARD_H = 5; // TODO: ボード部分がスクロールできるようになったら10に

        public static int BoardW = 4;
        public static int BoardH = 4;
        public static int BoardRealmHeight = 1; // 自陣領域
        public static int FriendsCount = 3;
        public static int InitialSandstar = 0;
        public static int GettingSandstarPerTurn = 1;
        public static int GettingSandstarOnWait = 1;
        public static int NeededSandstarForPlacingNewFriend = 5;
        public static int MaxSandstar = 15;
        #endregion

        #region PlayerInfo
        public static PlayerInfo[] Players = new PlayerInfo[]{
            new PlayerInfo(){
                ID=0,
                Name="Player1",
                PlayerType=PlayerType.User,
                PlayerColor=Color.magenta,
                Friends=null,
                CPUDifficulty=CPUDifficulty.Easy,
            },
            new PlayerInfo(){
                ID=1,
                Name="Player2",
                PlayerType=PlayerType.User,
                PlayerColor=Color.magenta,
                Friends=null,
                CPUDifficulty=CPUDifficulty.Easy,
            },
        };
        #endregion

        #region GameResultInfo
        public static PlayerInfo? Winner;
        #endregion

        #region Config
        public static float BGMVolume = 0.5f;
        public static float SEVolume = 0.5f;
        #endregion
    }

    public struct PlayerInfo : IEqualityComparer<PlayerInfo>
    {
        public int ID;
        public string Name;
        public PlayerType PlayerType;
        public Color PlayerColor;
        public RotationDirection Direction;
        public Friend Leader { get => (Friends!=null && Friends.Length>0) ? Friends[0] : null; }
        public Friend[] Friends;

        public CPUStrategy CPUStrategy;
        private CPUDifficulty _cpu_difficulty;
        public CPUDifficulty CPUDifficulty { get => _cpu_difficulty; set => SetCPUDifficulty(value); }

        public bool Equals(PlayerInfo x, PlayerInfo y)
        {
            return x.ID==y.ID;
        }

        public int GetHashCode(PlayerInfo obj)
        {
            return obj.ID;
        }

        private void SetCPUDifficulty(CPUDifficulty diff)
        {
            _cpu_difficulty = diff;
            switch (_cpu_difficulty)
            {
                case CPUDifficulty.Easy:
                    CPUStrategy.Overall = CPUStrategyOverall.MoveOnly100;
                    CPUStrategy.Defense = CPUStrategyDefense.None;
                    CPUStrategy.DefenseFor = CPUStrategyDefenseFor.LeaderOnly;
                    CPUStrategy.Select = CPUStrategySelect.Random;
                    CPUStrategy.Move = CPUStrategyMove.Random;
                    break;
                case CPUDifficulty.Normal:
                    CPUStrategy.Overall = CPUStrategyOverall.TryLounge50;
                    CPUStrategy.Defense = CPUStrategyDefense.AlwaysEscape;
                    CPUStrategy.DefenseFor = CPUStrategyDefenseFor.All;
                    CPUStrategy.Select = CPUStrategySelect.Backline;
                    CPUStrategy.Move = CPUStrategyMove.ApproachToLeader;
                    break;
                case CPUDifficulty.Hard:
                    CPUStrategy.Overall = CPUStrategyOverall.BestEvaluation;
                    break;
            }
        }
    }

}