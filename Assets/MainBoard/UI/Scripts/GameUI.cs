using JSF.Common.UI;
using JSF.Database;
using JSF.Game.Board;
using JSF.Game.Effect;
using JSF.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using System;

namespace JSF.Game.UI
{
    public class GameUI : MonoBehaviour
    {
        public GameManager GameManager;

        public ViewCanvasController ViewCanvasController;

        public WaitButton WaitButton;

        public GameObject CutInPrefab;
        public GameObject GameEndPrefab;
        public GameObject LoungeCellPrefab;
        public WhiteOutEffectController whiteOutEffectController;

        public AudioClip ClickSound;

        [SerializeField]
        private FriendOnBoard _selectedFriendOnBoard;

        public FriendOnBoard SelectedFriendOnBoard { get => _selectedFriendOnBoard; }

        [SerializeField]
        private UIMode UIMode;

        [SerializeField]
        private bool CanInteract = false;

        public void OnClickEmptyCell(Cell cell)
        {
            Util.PlaySE(ClickSound);
            if (OnClickCell(cell))
            {

            }
            else
            {
                _selectedFriendOnBoard = null;
            }
        }

        public void OnClickFriendsOnBoard(FriendOnBoard friend)
        {
            Util.PlaySE(ClickSound);
            if (OnClickCell(friend.Cell))
            {

            }
            else
            {
                if (friend.Possessor == GameManager.PlayerInTurn)
                {
                    if (_selectedFriendOnBoard == friend)
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
                                _selectedFriendOnBoard = null;
                                break;
                            default:
                                Debug.LogError("Unknown UIMode " + UIMode.ToString());
                                break;
                        }
                    }
                    else
                    {
                        _selectedFriendOnBoard = friend;
                        UIMode = UIMode.Move;
                    }
                }
                else
                {
                    _selectedFriendOnBoard = friend;
                    UIMode = UIMode.View;
                }
            }
        }

        // 何かしらのアクションを起こす(ターン経過を含むがこれに限らない)場合はtrueを返す
        private bool OnClickCell(Cell cell)
        {
            if (_selectedFriendOnBoard == null)
            {
                return false;
            }
            if (_selectedFriendOnBoard.Cell == cell)
            {
                return false;
            }
            if(_selectedFriendOnBoard.Possessor != GameManager.PlayerInTurn)
            {
                return false;
            }
            if (_selectedFriendOnBoard.Pos.HasValue)
            {
                // 盤上のフレンズを選択している場合
                Vector2Int diff = cell.SelfPos - _selectedFriendOnBoard.Pos.Value;
                diff = RotationDirectionUtil.GetRotatedVector(diff, RotationDirectionUtil.Invert(_selectedFriendOnBoard.Dir));
                if (UIMode == UIMode.Move && _selectedFriendOnBoard.Friend.CanNormalMove(diff) && !cell.RotationOnly)
                {
                    // フレンズをcellに動かす
                    StartCoroutine(MoveFriendCoroutine(_selectedFriendOnBoard, cell, null, true));
                    return true;
                }
                else if (UIMode == UIMode.Rotate && _selectedFriendOnBoard.Friend.CanRotateTo(diff))
                {
                    // 回転
                    StartCoroutine(MoveFriendCoroutine(_selectedFriendOnBoard, _selectedFriendOnBoard.Cell, RotationDirectionUtil.CalcRotationDegreeFromVector(_selectedFriendOnBoard.Pos.Value - cell.SelfPos), false));
                    return true;
                }
                else if (UIMode == UIMode.Skill && !cell.RotationOnly)
                {
                    var Skill = _selectedFriendOnBoard.Friend.GetSkillMapByPos(diff);
                    if (Skill.HasValue)
                    {
                        if (Skill.Value.NeededSandstar > _selectedFriendOnBoard.Possessor.SandstarAmount)
                        {
                            GameManager.PlayerInTurn.PlaySandstarGaugeAnimation(Player.SandstarGaugeStatus.LitRed, Skill.Value.NeededSandstar, 1f);
                            return true;
                        }
                        if(_selectedFriendOnBoard.Friend.SimulateSkill(cell.SelfPos, _selectedFriendOnBoard, GameManager).CanUseSkill)
                        {
                            // スキル発動
                            StartCoroutine(UseSkillCoroutine(_selectedFriendOnBoard, cell));
                            return true;
                        }
                        else
                        {
                            // TODO:スキル使用不可のメッセージ
                            return true;
                        }
                    }
                }
            }
            else
            {
                // TODO: 駒台のフレンズを選択している場合
                if (GameManager.PlayerInTurn.SandstarAmount >= GlobalVariable.NeededSandstarForPlacingNewFriend)
                {
                    GameManager.PlaceFriendFromLounge(_selectedFriendOnBoard, cell);
                    return true;
                }
                else
                {
                    GameManager.PlayerInTurn.PlaySandstarGaugeAnimation(Player.SandstarGaugeStatus.LitRed, GlobalVariable.NeededSandstarForPlacingNewFriend, 1f);
                }
            }
            return false;
        }

        public bool OnClickSkip()
        {
            if (CanInteract)
            {
                Util.PlaySE(ClickSound);
                StartCoroutine(GameManager.SkipTurn());
                return true;
            }
            else
            {
                return false;
            }
        }
        public void OnStartDragFriendOnBoard(FriendOnBoard friend, Cell from)
        {
            friend.transform.SetParent(transform, true);
            if (_selectedFriendOnBoard != friend)
            {
                // ドラッグし始めたフレンズが未選択だった場合はUIModeをリセット
                UIMode = UIMode.View;
            }
            _selectedFriendOnBoard = friend;
        }
        public void OnDraggingFriendOnBoard(FriendOnBoard friend, Vector2 cursorPos)
        {
            friend.transform.position = new Vector3(cursorPos.x, cursorPos.y, 0);
        }
        public void OnDragAndDropFriendOnBoard(FriendOnBoard friend, Cell from, Cell to)
        {
            _selectedFriendOnBoard = null;
            friend.transform.SetParent(from.transform, true);
            friend.transform.localPosition = Vector3.zero;
            if(friend.Cell is LoungeCell)
            {
                if (to != null)
                {
                    if (friend.Possessor.SandstarAmount >= GlobalVariable.NeededSandstarForPlacingNewFriend)
                    {
                        GameManager.PlaceFriendFromLounge(friend, to);
                    }
                    else
                    {
                        GameManager.PlayerInTurn.PlaySandstarGaugeAnimation(Player.SandstarGaugeStatus.LitRed, GlobalVariable.NeededSandstarForPlacingNewFriend, 1f);
                    }
                }
            }
            else
            {
                Vector2Int diff = to.SelfPos - from.SelfPos;
                diff = RotationDirectionUtil.GetRotatedVector(diff, RotationDirectionUtil.Invert(friend.Dir));
                if ((UIMode == UIMode.Move || UIMode == UIMode.View) && friend.Friend.CanNormalMove(diff) && !to.RotationOnly)
                {
                    StartCoroutine(MoveFriendCoroutine(friend, to, null, false));
                }
                else if ((UIMode == UIMode.Rotate || UIMode == UIMode.View) && friend.Friend.CanRotateTo(diff))
                {
                    StartCoroutine(MoveFriendCoroutine(friend, from,
                        RotationDirectionUtil.Merge(friend.Dir, RotationDirectionUtil.CalcRotationDegreeFromVector(-diff)), false));
                }
                else if ((UIMode == UIMode.Skill || UIMode == UIMode.View) && !to.RotationOnly && friend.Friend.SimulateSkill(to.SelfPos, friend, GameManager).CanUseSkill)
                {
                    var _Skill = friend.Friend.GetSkillMapByPos(diff);
                    if (_Skill.HasValue)
                    {
                        var Skill = _Skill.Value;
                        if (Skill.NeededSandstar > GameManager.PlayerInTurn.SandstarAmount)
                        {
                            // TODO:サンドスター不足のメッセージ
                            GameManager.PlayerInTurn.PlaySandstarGaugeAnimation(Player.SandstarGaugeStatus.LitRed, Skill.NeededSandstar, 1f);
                            return;
                        }
                        StartCoroutine(UseSkillCoroutine(friend, to));
                    }
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
            if (_selectedFriendOnBoard)
            {
                if (!_selectedFriendOnBoard.Pos.HasValue)
                {
                    // TODO:駒台のフレンズを選択しているとき
                    switch (GameManager.PlayerInTurn.Direction)
                    {
                        case RotationDirection.FORWARD:
                            if (cell.SelfPos.y > GlobalVariable.BoardRealmHeight)
                            {
                                // 領域外
                                return CellDrawStatus.CannotUse;
                            }
                            break;
                        case RotationDirection.BACKWARD:
                            if (GlobalVariable.BoardH - 1 - cell.SelfPos.y > GlobalVariable.BoardRealmHeight)
                            {
                                // 領域外
                                return CellDrawStatus.CannotUse;
                            }
                            break;
                        default:
                            return CellDrawStatus.CannotUse;
                    }
                    if(GameManager.PlayerInTurn.SandstarAmount >= GlobalVariable.NeededSandstarForPlacingNewFriend)
                    {
                    }
                    else
                    {
                        disabled = true;
                    }
                    return CellDrawStatus.Normal;
                }
                if (_selectedFriendOnBoard.Pos == cell.SelfPos)
                {
                    if (cell.Friends != null && cell.Friends.Possessor != GameManager.PlayerInTurn)
                    {
                        disabled = true;
                    }
                    return CellDrawStatus.Selected;
                }
                Vector2Int diff = cell.SelfPos - _selectedFriendOnBoard.Pos.Value;
                diff = RotationDirectionUtil.GetRotatedVector(diff, RotationDirectionUtil.Invert(_selectedFriendOnBoard.Dir));

                if (UIMode==UIMode.Move && _selectedFriendOnBoard.Friend.CanNormalMove(diff) && !cell.RotationOnly)
                {
                    return CellDrawStatus.CanMove;
                }
                else if(UIMode == UIMode.Rotate && _selectedFriendOnBoard.Friend.CanRotateTo(diff))
                {
                    return CellDrawStatus.CanRotate;
                }
                else if (UIMode == UIMode.Skill && !cell.RotationOnly)
                {
                    var _skillMap = _selectedFriendOnBoard.Friend.GetSkillMapByPos(diff);
                    if (_skillMap.HasValue)
                    {
                        var skillMap = _skillMap.Value;
                        if (skillMap.NeededSandstar > _selectedFriendOnBoard.Possessor.SandstarAmount)
                        {
                            disabled = true;
                        }
                        if(!_selectedFriendOnBoard.Friend.SimulateSkill(cell.SelfPos, _selectedFriendOnBoard, GameManager).CanUseSkill)
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
                    if (_selectedFriendOnBoard.Friend.CanNormalMove(diff) && !cell.RotationOnly)
                    {
                        return CellDrawStatus.CanMove;
                    }
                    else if (_selectedFriendOnBoard.Friend.CanRotateTo(diff))
                    {
                        return CellDrawStatus.CanRotate;
                    }

                    var _skillMap = _selectedFriendOnBoard.Friend.GetSkillMapByPos(diff);
                    if (_skillMap.HasValue && !cell.RotationOnly)
                    {
                        return CellDrawStatus.CanEffectBySkill;
                    }
                    else
                    {
                        disabled = false;
                        return CellDrawStatus.CannotUse;
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

        public Cell GetCellFromScreenPos(Vector2 pos)
        {
            PointerEventData ev = new PointerEventData(EventSystem.current);
            ev.position = pos;

            List<RaycastResult> result = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ev, result);
            try
            {
                return result.Select(r => r.gameObject?.GetComponent<Cell>()).First(t => t != null);
            }catch(InvalidOperationException)
            {
                return null;
            }
        }

        public IEnumerator PlayCutIn(Friend friend)
        {
            // SE
            Util.PlaySE(SE.SEType.SkillCutIn);

            // カットイン表示
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
            _selectedFriendOnBoard = null;
            UIMode = UIMode.View;
            CanInteract = true;
        }
    }

    public enum CellDrawStatus
    {
        Normal, CanMove, CanRotate, CanEffectBySkill, Selected, CannotUse
    }

    public enum MouseStatus
    {
        None, Hovered, Clicked
    }

    public enum UIMode
    {
        Move, Rotate, Skill, View
    }
}