using JSF.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Game.UI;

namespace JSF.Game.Board
{
    public class GameManager : MonoBehaviour
    {
        public GameObject FriendOnBoardPrefab;

        public Player.Player[] Players;

        public Player.Player PlayerInTurn;

        public Dictionary<Vector2Int, Cell> Map { get; private set; } = new Dictionary<Vector2Int, Cell>();
        public Dictionary<Vector2Int, FriendOnBoard> Friends { get; private set; } = new Dictionary<Vector2Int, FriendOnBoard>();

        public GameUI GameUI;
        public BoardRenderer BoardRenderer;

        public Transform EffectObject;

        public void Start()
        {

        }

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
                    friendOnBoard.MoveToCell(Cell);
                    friendOnBoard.SetDir(dir);

                    Cell.Friends = friendOnBoard;
                    Debug.Log("Placed friend " + friendOnBoard.Friend.Name + ": " + pos +" rot="+dir.ToString());

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
        public bool MoveFriend(FriendOnBoard friendOnBoard, Vector2Int to, RotationDirection dir)
        {
            if (Map.TryGetValue(to, out Cell cell_to))
            {
                return MoveFriend(friendOnBoard, cell_to, dir);
            }
            else
            {
                Debug.LogError("No such coordinate: " + to);
            }
            return false;
        }
        public bool MoveFriendWithAnimation(FriendOnBoard friendOnBoard, Cell cell_to)
        {
            if (Map.TryGetValue(friendOnBoard.Pos, out Cell cell_from))
            {
                StartCoroutine(friendOnBoard.Friend.MoveNormal(friendOnBoard.Pos, cell_to.SelfPos, friendOnBoard));
                return true;
            }
            else
            {
                Debug.LogError("No such coordinate: " + friendOnBoard.Pos);
            }
            return false;
        }
        public bool MoveFriend(FriendOnBoard friendOnBoard, Cell cell_to, RotationDirection dir)
        {
            if (Map.TryGetValue(friendOnBoard.Pos, out Cell cell_from))
            {
                if (cell_to.Friends != null)
                {
                    cell_to.Friends.GoToLounge(Players[0]);
                }
                cell_from.Friends = null;
                friendOnBoard.MoveToCell(cell_to);
                cell_to.Friends = friendOnBoard;
                friendOnBoard.transform.SetParent(cell_to.transform, false);
                friendOnBoard.transform.localPosition = Vector3.zero;
                friendOnBoard.SetDir(dir);
                Debug.Log("Moved friend " + friendOnBoard.Friend.Name + ": " + cell_from.SelfPos + "->" + cell_to.SelfPos);
                return true;
            }
            else
            {
                Debug.LogError("No such coordinate: " + friendOnBoard.Pos);
            }
            return false;
        }
    }
}