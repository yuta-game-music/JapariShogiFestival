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
        public virtual IEnumerator MoveNormal(Vector2Int from, Vector2Int to, FriendOnBoard friendsOnBoard)
        {
            var animation_name = (RotationDirectionUtil.IsStraight(friendsOnBoard.Dir)) ? "MoveForward" : "MoveForward1.41";
            
            friendsOnBoard.Animator.SetBool(animation_name, true);
            int layer_id = friendsOnBoard.Animator.GetLayerIndex("Movement");
            yield return new WaitUntil(() => friendsOnBoard.Animator.GetCurrentAnimatorStateInfo(layer_id).IsName("WaitingForControl"));
            friendsOnBoard.GameManager.MoveFriend(friendsOnBoard, to);
            friendsOnBoard.Animator.SetBool(animation_name, false);
        }
        public abstract IEnumerator MoveWithSkill(Vector2Int from, Vector2Int to, FriendOnBoard friendsOnBoard);

        public Sprite OnBoardImage;

    }

}