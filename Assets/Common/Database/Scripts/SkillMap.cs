using JSF.Database;
using System;
using UnityEngine;

namespace JSF.Game
{
    [Serializable]
    public struct SkillMap
    {
        public string Name;
        public string Description;
        public Vector2Int[] Pos;
        public int NeededSandstar;
        public FriendActionDescriptor ActionDescriptor;
    }

}