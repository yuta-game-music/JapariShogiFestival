using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Game.Player;
using JSF.Database;
using JSF.Game;

namespace JSF
{
    public static class GlobalVariable
    {
        #region DebugInfo
        public static bool DebugMode = true;
        #endregion

        #region BoardInfo
        public static readonly int MIN_BOARD_W = 3;
        public static readonly int MAX_BOARD_W = 10;
        public static readonly int MIN_BOARD_H = 3;
        public static readonly int MAX_BOARD_H = 10;

        public static int BoardW = 5;
        public static int BoardH = 5;
        public static int BoardRealmHeight = 1;//é©êwóÃàÊ
        public static int InitialSandstar = 0;
        public static int NeededSandstarForPlacingNewFriend = 5;
        public static int MaxSandstar = 15;
        #endregion

        #region PlayerInfo
        public static PlayerInfo[] Players = new PlayerInfo[]{
            new PlayerInfo(0,"Player1",PlayerType.User,Color.magenta,null),
            new PlayerInfo(1,"Player2",PlayerType.User,Color.blue,null)
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


        public PlayerInfo(int id, string name, PlayerType playerType, Color playerColor, Friend[] friends)
        {
            ID = id;
            Name = name;
            PlayerType = playerType;
            PlayerColor = playerColor;
            Friends = friends;
            Direction = id == 0 ? RotationDirection.FORWARD : RotationDirection.BACKWARD;
        }

        public bool Equals(PlayerInfo x, PlayerInfo y)
        {
            return x.ID==y.ID;
        }

        public int GetHashCode(PlayerInfo obj)
        {
            return obj.ID;
        }
    }

}