using JSF.Common.UI;
using JSF.Database;
using JSF.Game.Board;
using JSF.Game.Effect;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JSF.Game.UI
{
    public class GameUI : MonoBehaviour
    {
        public GameManager GameManager;

        public ViewCanvasController ViewCanvasController;

        public WaitButton WaitButton;

        public GameObject CutInPrefab;
        public GameObject GameEndPrefab;
        public WhiteOutEffectController whiteOutEffectController;

        [SerializeField]
        private FriendOnBoard SelectedFriendOnBoard;

        [SerializeField]
        private UIMode UIMode;

        [SerializeField]
        private bool CanInteract = false;

        public void OnClickEmptyCell(Cell cell)
        {
            if (OnClickCell(cell))
            {

            }
            else
            {
                SelectedFriendOnBoard = null;
            }
        }

        public void OnClickFriendsOnBoard(FriendOnBoard friend)
        {
            if (OnClickCell(friend.Cell))
            {

            }
            else
            {
                if (friend.Possessor == GameManager.PlayerInTurn)
                {
                    if (SelectedFriendOnBoard == friend)
                    {
                        switch (UIMode)
                        {
                            case UIMode.View:
                            case UIMode.Move:
                                UIMode = UIMode.Rotate;
                                break;
                            case UIMode.Rotate:
                                UIMode = UIMode.Skill;
                                break;
                            case UIMode.Skill:
                                SelectedFriendOnBoard = null;
                                break;
                            default:
                                Debug.LogError("Unknown UIMode " + UIMode.ToString());
                                break;
                        }
                    }
                    else
                    {
                        SelectedFriendOnBoard = friend;
                        UIMode = UIMode.Move;
                    }
                }
                else
                {
                    SelectedFriendOnBoard = friend;
                    UIMode = UIMode.View;
                }
            }
        }

        // 何かしらのアクションを起こす(ターン経過を含むがこれに限らない)場合はtrueを返す
        private bool OnClickCell(Cell cell)
        {
            if (SelectedFriendOnBoard == null)
            {
                return false;
            }
            if (SelectedFriendOnBoard.Cell == cell)
            {
                return false;
            }
            if(SelectedFriendOnBoard.Possessor != GameManager.PlayerInTurn)
            {
                return false;
            }
            if (SelectedFriendOnBoard.Pos.HasValue)
            {
                // 盤上のフレンズを選択している場合
                Vector2Int diff = cell.SelfPos - SelectedFriendOnBoard.Pos.Value;
                diff = RotationDirectionUtil.GetRotatedVector(diff, RotationDirectionUtil.Invert(SelectedFriendOnBoard.Dir));
                if (UIMode == UIMode.Move && SelectedFriendOnBoard.Friend.CanNormalMove(diff) && !cell.RotationOnly)
                {
                    // フレンズをcellに動かす
                    StartCoroutine(MoveFriendCoroutine(SelectedFriendOnBoard, cell, null, true));
                    return true;
                }
                else if (UIMode == UIMode.Rotate && SelectedFriendOnBoard.Friend.CanRotateTo(diff))
                {
                    // 回転
                    StartCoroutine(MoveFriendCoroutine(SelectedFriendOnBoard, SelectedFriendOnBoard.Cell, RotationDirectionUtil.CalcRotationDegreeFromVector(SelectedFriendOnBoard.Pos.Value - cell.SelfPos), false));
                    return true;
                }
                else if (UIMode == UIMode.Skill && !cell.RotationOnly)
                {
                    var Skill = SelectedFriendOnBoard.Friend.GetSkillMapByPos(diff);
                    if (Skill.HasValue)
                    {
                        if (Skill.Value.NeededSandstar > SelectedFriendOnBoard.Possessor.SandstarAmount)
                        {
                            return false;
                        }
                        if(SelectedFriendOnBoard.Friend.CanUseSkill(cell.SelfPos, SelectedFriendOnBoard, GameManager))
                        {
                            // スキル発動
                            StartCoroutine(UseSkillCoroutine(SelectedFriendOnBoard, cell));
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                // TODO: 駒台のフレンズを選択している場合
            }
            return false;
        }

        public bool OnClickSkip()
        {
            if (CanInteract)
            {
                StartCoroutine(GameManager.SkipTurn());
                return true;
            }
            else
            {
                return false;
            }
        }

        public void OnDragAndDropFriendsOnBoard(FriendOnBoard friend, Cell from, Cell to)
        {
            SelectedFriendOnBoard = friend;
            Vector2Int diff = to.SelfPos - from.SelfPos;
            diff = RotationDirectionUtil.GetRotatedVector(diff, RotationDirectionUtil.Invert(friend.Dir));
            if(friend.Friend.CanNormalMove(diff))
            {
                StartCoroutine(MoveFriendCoroutine(SelectedFriendOnBoard, to, null, true));
            }
        }

        public IEnumerator OnPlayerTurnStart(Player.Player PlayerInTurn)
        {
            WaitButton.SetPlayerColor(PlayerInTurn.PlayerColor);
            yield return null;
        }
        private IEnumerator MoveFriendCoroutine(FriendOnBoard SelectedFriendOnBoard, Cell to, RotationDirection? dir, bool Animated)
        {
            CanInteract = false;
            if (Animated)
            {
                yield return GameManager.MoveFriendWithAnimation(SelectedFriendOnBoard, to, true);
            }
            else
            {
                if (!dir.HasValue)
                {
                    dir = RotationDirectionUtil.CalcRotationDegreeFromVector((SelectedFriendOnBoard.Pos ?? Vector2Int.zero) - to.SelfPos);
                }
                yield return GameManager.MoveFriend(SelectedFriendOnBoard, to.SelfPos, dir.Value, true);
            }
            CanInteract = true;
        }
        private IEnumerator UseSkillCoroutine(FriendOnBoard SelectedFriendOnBoard, Cell cell)
        {
            CanInteract = false;
            yield return GameManager.UseSkill(SelectedFriendOnBoard, cell);
            CanInteract = true;
        }

        public CellDrawStatus GetCellDrawStatus(Cell cell, out bool disabled)
        {
            if (!CanInteract) { disabled = false; return CellDrawStatus.Normal; }
            disabled = false;
            if (SelectedFriendOnBoard)
            {
                if (!SelectedFriendOnBoard.Pos.HasValue)
                {
                    // TODO:駒台のフレンズを選択しているとき
                    return CellDrawStatus.Normal;
                }
                if (SelectedFriendOnBoard.Pos == cell.SelfPos)
                {
                    if (cell.Friends != null && cell.Friends.Possessor != GameManager.PlayerInTurn)
                    {
                        disabled = true;
                    }
                    return CellDrawStatus.Selected;
                }
                Vector2Int diff = cell.SelfPos - SelectedFriendOnBoard.Pos.Value;
                diff = RotationDirectionUtil.GetRotatedVector(diff, RotationDirectionUtil.Invert(SelectedFriendOnBoard.Dir));

                if (UIMode==UIMode.Move && SelectedFriendOnBoard.Friend.CanNormalMove(diff) && !cell.RotationOnly)
                {
                    return CellDrawStatus.CanMove;
                }
                else if(UIMode == UIMode.Rotate && SelectedFriendOnBoard.Friend.CanRotateTo(diff))
                {
                    return CellDrawStatus.CanRotate;
                }
                else if (UIMode == UIMode.Skill && !cell.RotationOnly)
                {
                    var _skillMap = SelectedFriendOnBoard.Friend.GetSkillMapByPos(diff);
                    if (_skillMap.HasValue)
                    {
                        var skillMap = _skillMap.Value;
                        if (skillMap.NeededSandstar > SelectedFriendOnBoard.Possessor.SandstarAmount)
                        {
                            disabled = true;
                        }
                        if(!SelectedFriendOnBoard.Friend.CanUseSkill(cell.SelfPos, SelectedFriendOnBoard, GameManager))
                        {
                            disabled = true;
                        }
                        return CellDrawStatus.CanEffectBySkill;
                    }
                    else
                    {
                        return CellDrawStatus.CannotUse;
                    }
                }
                else if (UIMode == UIMode.View)
                {
                    disabled = true;
                    if (SelectedFriendOnBoard.Friend.CanNormalMove(diff))
                    {
                        return CellDrawStatus.CanMove;
                    }
                    else if (SelectedFriendOnBoard.Friend.CanRotateTo(diff))
                    {
                        return CellDrawStatus.CanRotate;
                    }

                    var _skillMap = SelectedFriendOnBoard.Friend.GetSkillMapByPos(diff);
                    if (_skillMap.HasValue)
                    {
                        return CellDrawStatus.CanEffectBySkill;
                    }
                    else
                    {
                        disabled = false;
                        return CellDrawStatus.Normal;
                    }
                }
                else
                {
                    return CellDrawStatus.CannotUse;
                }
            }
            else
            {
                if (cell.Friends != null && cell.Friends.Possessor != GameManager.PlayerInTurn)
                {
                    disabled = true;
                }
                if (cell.RotationOnly)
                {
                    return CellDrawStatus.CannotUse;
                }
                else
                {
                    return CellDrawStatus.Normal;
                }
            }
        }

        public IEnumerator PlayCutIn(Friend friend)
        {
            var CutInObject = Instantiate(CutInPrefab);
            Transform p = ViewCanvasController?.EffectObject?.transform;
            CutInObject.transform.SetParent(p, false);
            RectTransform tf = CutInObject.GetComponent<RectTransform>();
            //tf.anchorMin = new Vector2(0,0.3f);
            //tf.anchorMax = new Vector2(1,0.7f);

            CutInEffectController cont = CutInObject.GetComponent<CutInEffectController>();
            cont.SetFriend(friend);
            yield return new WaitUntil(()=>cont.AnimationEnd);
            Destroy(CutInObject);
        }

        public IEnumerator PlayFinish(Player.Player Winner)
        {
            var GameEndObject = Instantiate(GameEndPrefab);
            Transform p = ViewCanvasController?.EffectObject?.transform;
            GameEndObject.transform.SetParent(p, false);
            RectTransform tf = GameEndObject.GetComponent<RectTransform>();
            //tf.anchorMin = new Vector2(0,0.3f);
            //tf.anchorMax = new Vector2(1,0.7f);
            
            GameEndEffectController cont = GameEndObject.GetComponent<GameEndEffectController>();
            cont.SetWinner(Winner);
            //yield return new WaitUntil(() => cont.AnimationEnd);
            yield return new WaitForSeconds(1f);
            yield return whiteOutEffectController.PlayWhiteIn();
        }

        public void ResetView()
        {
            SelectedFriendOnBoard = null;
            UIMode = UIMode.View;
            CanInteract = true;
        }
    }

    public enum CellDrawStatus
    {
        Normal, CanMove, CanRotate, CanEffectBySkill, Selected, CannotUse
    }

    public enum UIMode
    {
        Move, Rotate, Skill, View
    }
}