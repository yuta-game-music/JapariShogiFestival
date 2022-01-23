using JSF.Database;
using JSF.Database.Friends;
using JSF.Game.Board;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Game.UI
{
    public class GameUI : MonoBehaviour
    {
        public GameManager GameManager;

        public FriendOnBoard SelectedFriendOnBoard;

        public void OnClickEmptyCell(Cell cell)
        {
            if (!OnClickCell(cell))
            {
                GameManager?.PlaceFriend(cell.SelfPos, RotationDirection.FORWARD, FriendsDatabase.Get().GetFriend<Serval>());
                
            }
        }

        public void OnClickFriendsOnBoard(FriendOnBoard friend)
        {
            if (!OnClickCell(friend.Cell))
            {
                if (SelectedFriendOnBoard == friend)
                {
                    SelectedFriendOnBoard = null;
                }
                else
                {
                    SelectedFriendOnBoard = friend;
                }
            }
        }

        private bool OnClickCell(Cell cell)
        {
            if (SelectedFriendOnBoard == null)
            {
                return false;
            }
            if (SelectedFriendOnBoard.Cell == cell)
            {
                // その場回転
                SelectedFriendOnBoard.Rotate(RotationDirection.FORWARD_LEFT);
                return true;
            }
            Vector2Int diff = cell.SelfPos - SelectedFriendOnBoard.Pos;
            diff = RotationDirectionUtil.GetRotatedVector(diff, RotationDirectionUtil.Invert(SelectedFriendOnBoard.Dir));
            if (SelectedFriendOnBoard.Friend.GetMovementSheet().TryGetValue(diff, out MovementSettings movement_settings))
            {
                if (movement_settings.CanMoveNormal)
                {
                    // フレンズをcellに動かす
                    GameManager.MoveFriendWithAnimation(SelectedFriendOnBoard, cell);
                    return true;
                }
                else if (movement_settings.CanEffectBySkill)
                {
                    // スキル発動？
                    return true;
                }
                else if (movement_settings.CanRotateToward)
                {
                    // 回転？
                }
            }
            return false;
        }

        public void OnDragAndDropFriendsOnBoard(FriendOnBoard friend, Cell from, Cell to)
        {
            SelectedFriendOnBoard = friend;
            Vector2Int diff = to.SelfPos - from.SelfPos;
            diff = RotationDirectionUtil.GetRotatedVector(diff, RotationDirectionUtil.Invert(friend.Dir));
            if(friend.Friend.GetMovementSheet().TryGetValue(diff, out MovementSettings movement_settings))
            {
                if (movement_settings.CanMoveNormal)
                {
                    GameManager.MoveFriendWithAnimation(SelectedFriendOnBoard, to);
                }
            }
        }

        public CellDrawStatus GetCellDrawStatus(Cell cell)
        {
            if (SelectedFriendOnBoard)
            {
                if(SelectedFriendOnBoard.Pos == cell.SelfPos)
                {
                    return CellDrawStatus.Selected;
                }
                Vector2Int diff = cell.SelfPos - SelectedFriendOnBoard.Pos;
                diff = RotationDirectionUtil.GetRotatedVector(diff, RotationDirectionUtil.Invert(SelectedFriendOnBoard.Dir));
                if (SelectedFriendOnBoard.Friend.GetMovementSheet().TryGetValue(diff, out MovementSettings movement_settings))
                {
                    if (movement_settings.CanMoveNormal)
                    {
                        return CellDrawStatus.CanMove;
                    }
                    else if (movement_settings.CanEffectBySkill)
                    {
                        return CellDrawStatus.CanEffectBySkill;
                    }
                    else
                    {
                        return CellDrawStatus.Normal;
                    }
                }
                return CellDrawStatus.Normal;
            }
            else
            {
                return CellDrawStatus.Normal;
            }
        }
    }

    public enum CellDrawStatus
    {
        Normal, CanMove, CanEffectBySkill, Selected
    }
}