using JSF.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Game.Board
{
    public class BoardManager : MonoBehaviour
    {
        public GameObject FriendOnBoardPrefab;

        public Dictionary<Vector2Int, FriendOnBoard> Map { get; private set; }

        public BoardRenderer BoardRenderer;

        private void Update()
        {
            BoardRenderer?.SetBoard();
            enabled = false;
        }

        public bool PlaceFriend(Vector2Int pos, RotationDirection dir, Friend friend)
        {
            if(BoardRenderer.Cells.TryGetValue(pos, out Cell Cell))
            {
                if (Cell.Friends == null)
                {
                    GameObject friendOnBoardObject = Instantiate(FriendOnBoardPrefab);
                    friendOnBoardObject.transform.SetParent(Cell.transform, false);
                    friendOnBoardObject.transform.localPosition = Vector3.zero;
                    FriendOnBoard friendOnBoard = friendOnBoardObject.GetComponent<FriendOnBoard>();
                    friendOnBoard.Friend = friend;

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
    }
}