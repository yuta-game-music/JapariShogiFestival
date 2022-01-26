using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Game.Player;

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
            new PlayerInfo("Player1"),
            new PlayerInfo("Player2")
        };
        #endregion

    }

    public struct PlayerInfo
    {
        public string Name;
        public PlayerType PlayerType;

        public PlayerInfo(string name="Player", PlayerType playerType = PlayerType.User)
        {
            Name = name;
            PlayerType = playerType;
        }
    }

}