using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using JSF.Game;
using JSF.Game.Board;

namespace JSF.Database
{
    
    public abstract class Friend : ScriptableObject
    {
        public abstract string Name { get; }
        public abstract Dictionary<Vector2,MovementSettings> GetMovementSheet();
        public virtual IEnumerator MoveNormal(Vector2Int from, Vector2Int to, FriendOnBoard friendsOnBoard)
        {
            var animation_name = "MoveForward";

            friendsOnBoard.transform.SetParent(friendsOnBoard.GameManager.EffectObject, true);
            
            friendsOnBoard.Animator.SetBool(animation_name, true);
            int layer_id = friendsOnBoard.Animator.GetLayerIndex("Movement");
            Vector2Int diff = to - from;
            Debug.Log(diff+": "+ diff.magnitude);
            friendsOnBoard.Animator.SetLayerWeight(layer_id, diff.magnitude * 0.01f);
            friendsOnBoard.ViewerTF.transform.localRotation = Quaternion.FromToRotation(Vector3.up, new Vector3(diff.x,diff.y,0));

            yield return new WaitUntil(() => friendsOnBoard.Animator.GetCurrentAnimatorStateInfo(layer_id).IsName("WaitingForControl"));
            yield return friendsOnBoard.GameManager.MoveFriend(friendsOnBoard, to, RotationDirectionUtil.CalcRotationDegreeFromVector(-(to-from)));
            friendsOnBoard.Animator.SetBool(animation_name, false);
        }
        public abstract IEnumerator OnUseSkill(Vector2Int from, Vector2Int to, FriendOnBoard friendOnBoard, GameManager GameManager);

        public Sprite OnBoardImage;
        public Sprite CutInImage;
        public AnimatorOverrideController AnimatorOverrideController;
    }

}