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

        // ��������̃A�N�V�������N����(�^�[���o�߂��܂ނ�����Ɍ���Ȃ�)�ꍇ��true��Ԃ�
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
                // �Տ�̃t�����Y��I�����Ă���ꍇ
                Vector2Int diff = cell.SelfPos - SelectedFriendOnBoard.Pos.Value;
                diff = RotationDirectionUtil.GetRotatedVector(diff, RotationDirectionUtil.Invert(SelectedFriendOnBoard.Dir));
                if (SelectedFriendOnBoard.Friend.GetMovementSheet().TryGetValue(diff, out MovementSettings movement_settings))
                {
                    if (UIMode == UIMode.Move && movement_settings.CanMoveNormal && !cell.RotationOnly)
                    {
                        // �t�����Y��cell�ɓ�����
                        StartCoroutine(MoveFriendCoroutine(SelectedFriendOnBoard, cell, null, true));
                        return true;
                    }
                    else if (UIMode == UIMode.Rotate && movement_settings.CanRotateToward)
                    {
                        // ��]
                        StartCoroutine(MoveFriendCoroutine(SelectedFriendOnBoard, SelectedFriendOnBoard.Cell, RotationDirectionUtil.CalcRotationDegreeFromVector(SelectedFriendOnBoard.Pos.Value - cell.SelfPos), false));
                        return true;
                    }
                    else if (UIMode == UIMode.Skill && movement_settings.CanEffectBySkill && !cell.RotationOnly)
                    {
                        if (movement_settings.NeededSandstarForSkill > SelectedFriendOnBoard.Possessor.SandstarAmount)
                        {
                            return false;
                        }
                        // �X�L������
                        StartCoroutine(UseSkillCoroutine(SelectedFriendOnBoard, cell));
                        return true;
                    }
                }
            }
            else
            {
                // TODO: ���̃t�����Y��I�����Ă���ꍇ
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
            if(friend.Friend.GetMovementSheet().TryGetValue(diff, out MovementSettings movement_settings))
            {
                if (movement_settings.CanMoveNormal)
                {
                    StartCoroutine(MoveFriendCoroutine(SelectedFriendOnBoard, to, null, true));
                }
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
                    // TODO:���̃t�����Y��I�����Ă���Ƃ�
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
                if (SelectedFriendOnBoard.Friend.GetMovementSheet().TryGetValue(diff, out MovementSettings movement_settings))
                {
                    if (UIMode==UIMode.Move && !cell.RotationOnly && movement_settings.CanMoveNormal)
                    {
                        return CellDrawStatus.CanMove;
                    }
                    else if(UIMode == UIMode.Rotate && movement_settings.CanRotateToward)
                    {
                        return CellDrawStatus.CanRotate;
                    }
                    else if (UIMode == UIMode.Skill && !cell.RotationOnly && movement_settings.CanEffectBySkill)
                    {
                        if (movement_settings.NeededSandstarForSkill > SelectedFriendOnBoard.Possessor.SandstarAmount)
                        {
                            disabled = true;
                        }
                        return CellDrawStatus.CanEffectBySkill;
                    }
                    else if (UIMode == UIMode.View)
                    {
                        disabled = true;
                        if (movement_settings.CanMoveNormal)
                        {
                            return CellDrawStatus.CanMove;
                        }
                        else if (movement_settings.CanRotateToward)
                        {
                            return CellDrawStatus.CanRotate;
                        }
                        else if (movement_settings.CanEffectBySkill)
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
                return CellDrawStatus.CannotUse;
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