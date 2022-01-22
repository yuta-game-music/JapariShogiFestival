using JSF.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Game.Board
{
    public class BoardManager : MonoBehaviour
    {
        public GameObject FriendOnBoardPrefab;

        public Dictionary<Vector2Int, Cell> Map { get; private set; } = new Dictionary<Vector2Int, Cell>();
        public Dictionary<Vector2Int, FriendOnBoard> Friends { get; private set; } = new Dictionary<Vector2Int, FriendOnBoard>();

        public BoardRenderer BoardRenderer;

        private void Update()
        {
            BoardRenderer?.SetBoard();
            enabled = false;
        }

        public bool PlaceFriend(Vector2Int pos, RotationDirection dir, Friend friend)
        {
            if(Map.TryGetValue(pos, out Cell Cell))
            {
                if (Cell.Friends == null)
                {
                    GameObject friendOnBoardObject = Instantiate(FriendOnBoardPrefab);
                    friendOnBoardObject.transform.SetParent(Cell.transform, false);
                    friendOnBoardObject.transform.localPosition = Vector3.zero;
                    FriendOnBoard friendOnBoard = friendOnBoardObject.GetComponent<FriendOnBoard>();
                    friendOnBoard.Friend = friend;
                    friendOnBoard.MoveToCell(Cell.SelfPos);

                    Cell.Friends = friendOnBoard;

                    return true;
                }
                else
                {
                    Debug.LogError("Friends Already Exists!", Cell);
                    return false;
                }
            }
            else
            {
                Debug.LogError("No such coordinate: " + pos);
                return false;
            }
        }
        public bool MoveFriend(FriendOnBoard friendOnBoard, Vector2Int to)
        {
            if(Map.TryGetValue(friendOnBoard.Pos, out Cell cell_from))
            {
                if(Map.TryGetValue(to, out Cell cell_to))
                {
                    cell_from.Friends = null;
                    friendOnBoard.MoveToCell(to);
                    cell_to.Friends = friendOnBoard;
                    friendOnBoard.transform.SetParent(cell_to.transform, false);
                    friendOnBoard.transform.localPosition = Vector3.zero;
                    Debug.Log("Moved friend " + friendOnBoard.Friend.Name + ": " + cell_from.SelfPos + "->" + cell_to.SelfPos);
                    return true;
                }
                else
                {
                    Debug.LogError("No such coordinate: " + to);
                }
            }
            else
            {
                Debug.LogError("No such coordinate: " + friendOnBoard.Pos);
            }
            return false;
        }
    }
}