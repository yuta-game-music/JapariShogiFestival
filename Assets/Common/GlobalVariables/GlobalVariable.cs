using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Game.Player;
using JSF.Database;
using JSF.Database.Friends;

namespace JSF
{
    public static class GlobalVariable
    {
        #region DebugInfo
        public static bool DebugMode = true;
        #endregion

        #region BoardInfo
        public static int BoardW = 4;
        public static int BoardH = 4;
        public static int InitialSandstar = 0;
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
    }

    public struct PlayerInfo : IEqualityComparer<PlayerInfo>
    {
        public int ID;
        public string Name;
        public PlayerType PlayerType;
        public Color PlayerColor;
        public Friend Leader { get => (Friends!=null && Friends.Length>0) ? Friends[0] : null; }
        public Friend[] Friends;

        public PlayerInfo(int id, string name, PlayerType playerType, Color playerColor, Friend[] friends)
        {
            ID = id;
            Name = name;
            PlayerType = playerType;
            PlayerColor = playerColor;
            Friends = friends;
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