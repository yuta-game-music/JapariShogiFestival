using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using JSF.Game;

namespace JSF.Database
{
    
    public abstract class Friend : ScriptableObject
    {
        public abstract string Name { get; }
        public abstract Dictionary<Vector2,MovementSettings> GetMovementSheet();
        public abstract IEnumerator MoveNormal(Vector2 from, Vector2 to, FriendOnBoard friendsOnBoard);
        public abstract IEnumerator MoveWithSkill(Vector2 from, Vector2 to, FriendOnBoard friendsOnBoard);

        public Sprite OnBoardImage;

    }

}