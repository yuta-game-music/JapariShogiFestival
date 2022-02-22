using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Game.Tutorial
{
    [CreateAssetMenu(fileName = "TutorialDatabase.asset", menuName = "JSF/Tutorial/Database")]
    public class TutorialDatabase : ScriptableObject
    {
        public static Tutorial[] tutorials = new Tutorial[]
        {
            // �`���[�g���A��1 �ړ��Ǝ��
            new Tutorial()
            {
                GuideIconID = 0,
                InitialBoardStatus = new BoardStatus()
                {
                    Size=new Vector2Int(3,3),
                    Cells=new BoardCell[]
                    {
                        new BoardCell()
                        {
                            Pos = new Vector2Int(1,0),
                            Dir = RotationDirection.FORWARD,
                            FriendName = "Serval",
                            PlayerID = 0
                        },
                        new BoardCell()
                        {
                            Pos = new Vector2Int(1,2),
                            Dir = RotationDirection.BACKWARD,
                            FriendName = "Dhole",
                            PlayerID = 1,
                        },
                        new BoardCell()
                        {
                            Pos = new Vector2Int(0,0),
                            Dir = RotationDirection.FORWARD,
                            FriendName = "Serval",
                            PlayerID = 0
                        },
                        new BoardCell()
                        {
                            Pos = new Vector2Int(2,2),
                            Dir = RotationDirection.BACKWARD,
                            FriendName = "Dhole",
                            PlayerID = 1
                        },
                    },
                },
                InitialSandstarAmount = 0,
                Nodes = new TutorialNode[]
                {
                    new TutorialNode(){
                        Text="�悤�������Ղ�ցI���͈ē����̃T�[�o������A��낵���ˁI\n\n\n<size=70%>�N���b�Nor�^�b�v�Ŏ��ɐi�݂܂��B</size>"
                    },
                    new TutorialNode(){
                        Text="���̂��Ղ�́A�t�����Y������2�`�[���ɕ�����Ď�育�����Ő키�c�̐�Ȃ񂾁I"
                    },
                    new TutorialNode(){
                        Text="2�`�[���������ꏊ�łǂ�����΂�����키��I\n\n�t�����Y���ǂ����̃`�[���ɂ��邩�́A�t�����Y�̎���ɂ���g�̐F�Ŋm�F�ł����I",
                        TextPosAnchor=new Vector2(0.3f,0.5f),
                        FocusedGameObjectName="GameBoard"
                    },
                    new TutorialNode(){
                        Text="���ꂼ��̃`�[���ɂ̓��[�_�[�������I���[�_�[�͂���Ȃӂ��Ɂ��}�[�N���t���Ă����I",
                        FocusedCell=new Vector2Int[]{new Vector2Int(1,0) },
                        TextPosAnchor=new Vector2(0.3f,0.8f)
                    },
                    new TutorialNode(){
                        Text="����`�[���̃��[�_�[�����Ə�������I\n�t�Ɏ����̃��[�_�[�����ꂿ�Ⴄ�ƕ������Ⴄ�񂾁I",
                        FocusedCell=new Vector2Int[]{new Vector2Int(1,2) },
                        TextPosAnchor=new Vector2(0.3f,0.2f)
                    },
                    new TutorialNode()
                    {
                        Text="���̃Q�[���ł͌��݂Ƀ^�[��������Ă���񂾂��ǁc"
                    },
                    new TutorialNode
                    {
                        Text="1�^�[���̊ԂɁA�t�����Y��1�l���������Ƃ��ł����I�����ɂ��̃t�����Y�ɓ����Ă��炨�[�I",
                        FocusedCell=new Vector2Int[]{new Vector2Int(0,0) },
                        TextPosAnchor=new Vector2(0.3f,0.8f)
                    },
                    new TutorialNode
                    {
                        Text="�t�����Y�Ɏw�����o���ɂ́A���̃t�����Y�ɐG��Ă���s����̃}�X�ɐG��ĂˁI\n\n<color=#808080>�����̃t�����Y��1��̃}�X�ɓ������Ă݂܂��傤�B</color>",
                        FocusedCell=new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0,1) },
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                        MovableFriendsId = new int[]{0},
                        MovablePos = new Vector2Int[]{new Vector2Int(0,1)},
                        TutorialCondition = (GameManager, TutorialManager) =>
                        {
                            if(GameManager.Map.TryGetValue(new Vector2Int(0, 1), out Board.Cell cell))
                            {
                                if (cell.Friends)
                                {
                                    if(cell.Friends == TutorialManager.GetFriendById(2))
                                    {
                                        return true;
                                    }
                                }
                            }
                            return false;
                        }
                    },
                    new TutorialNode
                    {
                        Text="������[�I�w�����`�������I\n\n���͑���̃^�[������I����̏o�������Ă݂�[�I",
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                    },
                    new TutorialNode
                    {
                        Text="",
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                        TutorialCondition = (GameManager, TutorialManager) =>
                        {
                            return true;
                        },
                        AutoMoveStrategy = (GameManager, TutorialManager) =>
                        {
                            return GameManager.MoveFriendWithAnimation(
                                GameManager.Map[new Vector2Int(1,2)].Friends,
                                GameManager.Map[new Vector2Int(0,1)],
                                false
                            );
                        },
                    },
                    new TutorialNode
                    {
                        Text="�����I���Ԃ����ꂿ������c�ł�����ő���ɃX�L���ł�����I",
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                    },
                    new TutorialNode
                    {
                        Text="���̃t�����Y������̃��[�_�[����ꂻ�����ˁI�΂߂ɓ����Ď���Ă݂�[�I\n\n<color=#808080>�������̃t�����Y������̃}�X�ɓ������Ă݂܂��傤�B\n�t�����Y�̓h���b�O�ł��������܂��B</color>",

                        FocusedCell=new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0,1) },
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                        MovableFriendsId = new int[]{2},
                        MovablePos = new Vector2Int[]{new Vector2Int(0,1)},
                        TutorialCondition = (GameManager, TutorialManager) =>
                        {
                            if(GameManager.Map.TryGetValue(new Vector2Int(0, 1), out Board.Cell cell))
                            {
                                if (cell.Friends)
                                {
                                    if(cell.Friends == TutorialManager.GetFriendById(0))
                                    {
                                        return true;
                                    }
                                }
                            }
                            return false;
                        }
                    },
                    new TutorialNode
                    {
                        Text="�������[���I����ő���̃��[�_�[����ꂽ����A�������̏������ˁI"
                    },
                    new TutorialNode
                    {
                        Text="����Łu�ړ��v�u���v�̑���̐����͏I��肾��I\n\n<color=#808080>�N���b�Nor�^�b�v�Ń^�C�g����ʂɖ߂�܂��B</color>"
                    }
                }
            },

            
            // �`���[�g���A��2 ��]�Ɠ���X�L��
            new Tutorial()
            {
                GuideIconID=0,
                InitialBoardStatus = new BoardStatus()
                {
                    Size=new Vector2Int(4,3),
                    Cells=new BoardCell[]
                    {
                        new BoardCell()
                        {
                            Pos = new Vector2Int(1,0),
                            Dir = RotationDirection.FORWARD,
                            FriendName = "Serval",
                            PlayerID = 0
                        },
                        new BoardCell()
                        {
                            Pos = new Vector2Int(3,2),
                            Dir = RotationDirection.BACKWARD,
                            FriendName = "Dhole",
                            PlayerID = 1,
                        },
                        new BoardCell()
                        {
                            Pos = new Vector2Int(0,0),
                            Dir = RotationDirection.FORWARD_RIGHT,
                            FriendName = "Serval",
                            PlayerID = 0
                        },
                        new BoardCell()
                        {
                            Pos = new Vector2Int(2,2),
                            Dir = RotationDirection.BACKWARD,
                            FriendName = "Dhole",
                            PlayerID = 1
                        },
                    },
                },
                InitialSandstarAmount = 3,
                Nodes = new TutorialNode[]
                {
                    new TutorialNode(){
                        Text="�������������I�����͂Ƃ��Ă����̋Z���������I\n\n\n<size=70%>�N���b�Nor�^�b�v�Ŏ��ɐi�݂܂��B</size>"
                    },
                    new TutorialNode(){
                        Text="���̃Q�[���A���ʂ́u�����v�ƈ���āA��̌������ς�����񂾁I"
                    },
                    new TutorialNode(){
                        Text="�Ⴆ�΂��̎q�A�����E�ɌX���Ă���ł���H",
                        FocusedCell=new Vector2Int[]{new Vector2Int(0,0) },
                        TextPosAnchor=new Vector2(0.7f,0.8f)
                    },
                    new TutorialNode(){
                        Text="���̌����ɂ���āA��̓�����ꏊ���ς��񂾁I\n�����ɂ��̎q����]�����Ă݂�[�I",
                        FocusedCell=new Vector2Int[]{new Vector2Int(1,0) },
                        TextPosAnchor=new Vector2(0.3f,0.2f)
                    },
                    new TutorialNode
                    {
                        Text="��]������ɂ́A���̃t�����Y��<u><color=#800000>2��</color></u>�G��Ă���\n���������ق��ɂ���}�X�ɐG��ĂˁI\n\n<color=#808080>���[�_�[�̃t�����Y���E������ɂ��Ă݂܂��傤�B</color>",
                        FocusedCell=new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(2,1) },
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                        MovableFriendsId = new int[]{0},
                        MovablePos = new Vector2Int[]{new Vector2Int(1,0)},
                        TutorialCondition = (GameManager, TutorialManager) =>
                        {
                            if(GameManager.Map.TryGetValue(new Vector2Int(1, 0), out Board.Cell cell))
                            {
                                if (cell.Friends)
                                {
                                    if(cell.Friends == TutorialManager.GetFriendById(0))
                                    {
                                        if(cell.Friends.Dir == RotationDirection.FORWARD_RIGHT)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            return false;
                        }
                    },
                    new TutorialNode
                    {
                        Text="�ł����[�I",
                        TextPosAnchor=new Vector2(0.3f,0.2f),
                    },
                    new TutorialNode
                    {
                        Text="���͑���̃^�[������I�����Sandstar�Q�[�W�ɒ��ڂ��ĂĂˁI",
                        TextPosAnchor=new Vector2(0.3f,0.2f),
                        FocusedGameObjectName = "Game/ViewCanvas/ScaleSupporter/Background/GameBoard/Players/Player2/SandstarViewer"
                    },
                    new TutorialNode
                    {
                        Text="",
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                        TutorialCondition = (GameManager, TutorialManager) =>
                        {
                            return true;
                        },
                        AutoMoveStrategy = (GameManager, TutorialManager) =>
                        {
                            return GameManager.MoveFriend(
                                GameManager.Map[new Vector2Int(3,2)].Friends,
                                new Vector2Int(3,2),
                                RotationDirection.BACKWARD_LEFT,
                                false
                            );
                        },
                    },
                    new TutorialNode
                    {
                        Text="���ܑ������]�����񂾂��ǁA�T���h�X�^�[��1��������ˁI�H",
                        TextPosAnchor=new Vector2(0.3f,0.2f),
                    },
                    new TutorialNode
                    {
                        Text="�T���h�X�^�[��1�^�[����1��\n�������I���ƁA�^�[�����ɉ����������Ȃ���\n�T���h�X�^�[��ǉ���1���炤���Ƃ��ł����I\n\n<size=70%>���e�����ʂ͐ݒ�ɂ��ς��܂��B</size>",
                        TextPosAnchor=new Vector2(0.3f,0.2f),
                    },
                    new TutorialNode
                    {
                        Text="���̃T���h�X�^�[���g���Ɠ���X�L�����g�����Ƃ��ł����I\n����X�L���́A���ꂼ��̃t�����Y�̓��ӂȂ��ƂȂ񂾁I",
                        TextPosAnchor=new Vector2(0.3f,0.2f),
                    },
                    new TutorialNode
                    {
                        Text="�Ⴆ�Ύ��A�T�[�o���͑傫���W�����v���邱�Ƃ����ӂ�����A�ڂ̑O�̃t�����Y���щz���čs�����Ƃ��ł����I",
                        FocusedCell = new Vector2Int[]{ new Vector2Int(1,0) },
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                    },
                    new TutorialNode
                    {
                        Text="����X�L�����g���ɂ́A���̃t�����Y��3��G���āA���F�̃}�X���o���炻����G���Ă݂āI",

                        FocusedCell=new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(3,2) },
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                        MovableFriendsId = new int[]{0},
                        MovablePos = new Vector2Int[]{new Vector2Int(3,2)},
                        TutorialCondition = (GameManager, TutorialManager) =>
                        {
                            if(GameManager.Map.TryGetValue(new Vector2Int(3, 2), out Board.Cell cell))
                            {
                                if (cell.Friends)
                                {
                                    if(cell.Friends == TutorialManager.GetFriendById(0))
                                    {
                                        return true;
                                    }
                                }
                            }
                            return false;
                        }
                    },
                    new TutorialNode
                    {
                        Text="����߂����đ�W�����v�I�I\n����X�L���̌��ʂ̓t�����Y�ɂ���đS�R�Ⴄ����A�g���O�ɃX�L�������m�F����Ƃ�����I"
                    },
                    new TutorialNode
                    {
                        Text="�u��]�v�u����X�L���v�ɂ��ĕ����������ȁH\n������Ȃ����Ƃ�����΂܂�����ł������ɗ��ĂˁI\n\n<color=#808080>�N���b�Nor�^�b�v�Ń^�C�g����ʂɖ߂�܂��B</color>"
                    }
                }
            }


        };

        public Sprite[] tutorialImages = new Sprite[0];
    }

    public struct Tutorial
    {
        public int GuideIconID;
        public BoardStatus InitialBoardStatus;
        public TutorialNode[] Nodes;
        public int InitialSandstarAmount;
    }

    public struct BoardStatus
    {
        public Vector2Int Size;
        public BoardCell[] Cells;
    }
    public struct BoardCell
    {
        public Vector2Int Pos;
        public RotationDirection Dir;
        public string FriendName;
        public int PlayerID;
    }

    public struct TutorialNode
    {
        public string Text;
        public Vector2? TextPosAnchor;
        public Vector2Int[] FocusedCell;
        public string FocusedGameObjectName;
        public int[] MovableFriendsId;
        public Vector2Int[] MovablePos;
        public TutorialCondition TutorialCondition;
        public AutoMoveStrategy AutoMoveStrategy;
    }

    public delegate bool TutorialCondition(GameManager Manager, TutorialManager TutorialManager);
    public delegate IEnumerator AutoMoveStrategy(GameManager Manager, TutorialManager TutorialManager);
}