using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JSF.Game;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

/*
 * �A�N�V�����̎�ނ𑝂₵�����Ƃ��̎菇
 * 1. FriendsActionType�ɐV���ȍ��ڂ�ǉ�����
 * 2. FriendsActionTypeName.Names�ɒǉ��������ڂ̕\������ǉ����� �ǉ�����ʒu��FriendsActionType�̋L�q���ɍ��킹�邱��
 * 3. FriendAction�N���X���ɕK�v�ȕϐ���ǉ����� ������(�^�C�v��)_(�ϐ���)�Ƃ���̂��悢 
 * �@�@�ꍇ�ɂ���Ă͊����̕ϐ������ʕϐ������Ă悢���A���̍ۂ�4.�̍�ƂŐݒ肵���������ύX����̂�Y��Ȃ��悤�ɂ��邱��
 * 4. FriendActionDataDrawer.OnGUI���\�b�h����3.�̕ϐ��𑀍삷�邽�߂�serializedXXX�ϐ���p�ӂ��� ������3.�̕ϐ���(������) nameof�͎g���Ȃ�
 * 5. FriendActionDataDrawer.OnGUI���\�b�h���ŕϐ��̐ݒ�p���C�A�E�g���L�q����
 * 6. FriendActionDataDrawer.GetPropertyHeight���\�b�h���Ń��C�A�E�g�̕����L�q����
 * 7. Friend.OnUseSkill���ŏ������L�q����
 * 
 * */

namespace JSF.Database
{
    [CreateAssetMenu(fileName = "ActionDescriptor", menuName = "JSF/Friends/ActionDescriptor")]
    public class FriendActionDescriptor : ScriptableObject
    {
        public string Name;
        public string Memo;
        [HideInInspector]
        public List<FriendAction> FriendAction = new List<FriendAction>();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FriendActionDescriptor))]
    public class FriendActionDescriptorEditor : Editor
    {
        SerializedObject SerializedObject;
        ReorderableList ActionList;
        public void OnEnable()
        {
            FriendActionDescriptor desc = target as FriendActionDescriptor;
            SerializedObject = new SerializedObject(desc);
            var SerializedActions = SerializedObject.FindProperty("FriendAction");

            ActionList = new ReorderableList(SerializedObject, SerializedActions);
            ActionList.drawElementCallback = (rect, index, active, focused) =>
            {
                var actionData = SerializedActions.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, actionData);
                /*SerializedProperty prop = SerializedActions.GetArrayElementAtIndex(index);
                FriendAction action = desc.FriendAction[index];
                GUILayout.BeginArea(
                    rect,
                    SerializedActions.GetArrayElementAtIndex(index).type
                );
                action.DrawGUI(prop);
                GUILayout.EndArea();*/
            };
            ActionList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "�R�}���h�ꗗ");
            ActionList.elementHeightCallback = index => EditorGUI.GetPropertyHeight(SerializedActions.GetArrayElementAtIndex(index));

        }
        public override void OnInspectorGUI()
        {
            FriendActionDescriptor desc = target as FriendActionDescriptor;
            SerializedObject.Update();

            base.DrawDefaultInspector();
            ActionList.DoLayoutList();

            SerializedObject.ApplyModifiedProperties();
        }
    }
#endif
    [Serializable]
    public class FriendAction
    {
        public FriendActionType ActionType = FriendActionType.PlayAnimation;
        public string Comment;
        #region Common
        // ��������̍s����
        public LoungeDestinationType LoungeDestinationMode = LoungeDestinationType.Mine;

        public enum LoungeDestinationType
        {
            Mine, // �s����
            Opponent, // ����(�Z�����A���̍s���p�^�[���ɂ͎g�p�s��)
            Possessor, // ���L��
            OpponentOfPossessor // ���L�҂̑���(�΃Z�����A���Ȃ�Z�����A����)
        }
        public static readonly string[] LoungeDestinationTypeSelectorLabel = new string[]
        {
            "�s����",
            "����(�Z�����A���̍s���p�^�[���ɂ͎g�p�s��)",
            "���L��",
            "���L�҂̑���(�΃Z�����A���Ȃ�Z�����A����)"
        };

        public float WaitSec = 0;
        #endregion
        #region PlayAnimation
        // Skill�A�j���[�V�����ɑ������A�j���[�V����
        // ��(None)�Ńf�t�H���g���p������
        public AnimatorOverrideController Animation_Animation;

        // �A�j���[�V�������I���܂ő҂�(�󗓂ő҂��Ȃ�)
        public bool Animation_WaitForEnd = true;

        // �ړ������ɉ�����Weight��ς��邩�ǂ���
        // 1�}�X=0.01 ���[�N���b�h�������l�����邽�ߎ΂߂Ȃ�0.014142�Ȃ�
        public bool Animation_UseWeight = true;

        // ��]���[�h
        public Animation_RotationType Animation_RotationMode = Animation_RotationType.Toward;

        public enum Animation_RotationType
        {
            Toward, // ���̕����Ɍ���������
            Now, // ���̌�����ێ�
            New, // �V���������ɕύX
            Fixed // ��ʏ�ł̌������Œ�(��ʏ�������)
        }
        public static readonly string[] Animation_RotationTypeSelectorLabel = new string[]
        {
            "���̕����Ɍ���������",
            "���̌�����ێ�",
            "�V���������ɕύX",
            "��ʏ�ł̌������Œ�(��ʏ�������)"
        };
        #endregion

        #region ResetAnimatinon
        #endregion

        #region MoveToCell
        // �s����(���g���猩�����΍��W)
        public Vector2Int MoveToCell_MoveDestinationRelative = new Vector2Int();

        // ���ɃN���b�N�����ꏊ�𑫂����ǂ���
        public bool MoveToCell_AddClickedPos = true;
        #endregion

        #region Rotate
        // ��]�p
        public RotationDirection Rotate_RotationDirection;
        // ��]�p�̊
        public Rotation_RelativeTo Rotate_RelativeTo;
        public enum Rotation_RelativeTo
        {
            Toward, // �n�_���I�_�Ɍ���������
            Base, // �J�n���_�̌���
            Now, // ���݂̌���
            Fixed // �i�s���������
        }
        public static readonly string[] Rotate_RelativeToSelectorLabel = new string[]
        {
            "�n�_���I�_�Ɍ���������",
            "�J�n���_�̌���",
            "���݂̌���",
            "�i�s���������"
        };
        #endregion

        #region MoveToLounge
        // ������ɑ���Ώ�(���g���猩�����΍��W)
        public Vector2Int MoveToLounge_MoveDestinationRelative = new Vector2Int();

        // ���ɃN���b�N�����ꏊ�𑫂����ǂ���
        public bool MoveToLounge_AddClickedPos = true;
        #endregion

        #region EndTurn
        #endregion
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FriendAction))]
    public class FriendActionDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var serializedActionType = property.FindPropertyRelative("ActionType");
            var serializedComment = property.FindPropertyRelative("Comment");
            #region Common
            var serializedLoungeDestinationMode = property.FindPropertyRelative("LoungeDestinationMode");
            var serializedWaitSec = property.FindPropertyRelative("WaitSec");
            #endregion
            #region Animation
            var serializedAnimationAnimation = property.FindPropertyRelative("Animation_Animation");
            var serializedAnimationWaitForEnd = property.FindPropertyRelative("Animation_WaitForEnd");
            var serializedAnimationUseWeight = property.FindPropertyRelative("Animation_UseWeight");
            var serializedAnimationRotationMode = property.FindPropertyRelative("Animation_RotationMode");
            #endregion
            #region MoveToCell
            var serializedMoveToCellMoveDestinationRelative = property.FindPropertyRelative("MoveToCell_MoveDestinationRelative");
            var serializedMoveToCellAddClickedPos = property.FindPropertyRelative("MoveToCell_AddClickedPos");
            #endregion
            #region Rotate
            var serializedRotateRotationDirection = property.FindPropertyRelative("Rotate_RotationDirection");
            var serializedRotateRelativeTo = property.FindPropertyRelative("Rotate_RelativeTo");
            #endregion
            #region MoveToLounge
            var serializedMoveToLoungeMoveDestinationRelative = property.FindPropertyRelative("MoveToLounge_MoveDestinationRelative");
            var serializedMoveToLoungeAddClickedPos = property.FindPropertyRelative("MoveToLounge_AddClickedPos");
            #endregion

            GUIStyle LabelStyle = new GUIStyle(GUI.skin.label);
            LabelStyle.wordWrap = true;
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                position.height = EditorGUIUtility.singleLineHeight;

                // �w�i
                var BackgroundRect = new Rect(position)
                {
                    height = GetPropertyHeight(property, label)
                };
                GUI.Box(BackgroundRect,"", GUI.skin.box);

                // �R�}���h���
                var ViewPortRect = new Rect(position);
                {
                    ViewPortRect.y = position.y;
                    serializedActionType.enumValueIndex = EditorGUI.Popup(
                        ViewPortRect,
                        "�R�}���h���",
                        serializedActionType.enumValueIndex,
                        FriendActionTypeName.Names
                    );
                    ViewPortRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                switch ((FriendActionType)(serializedActionType.intValue))
                {
                    case FriendActionType.PlayAnimation:
                        {
                            // �A�j���[�V�����Đ�
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedAnimationAnimation, new GUIContent("�A�j���[�V����"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "�g�p����A�j���[�V������ݒ肵�܂��B" +
                                        "�󗓂ɂ����Animator Override Controller�Őݒ肵��" +
                                        "�A�j���[�V�������g�p����܂��B",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing*2;
                                }
                            }
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedAnimationWaitForEnd, new GUIContent("�I���܂ő҂�"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "�L���ɂ���ƃA�j���[�V�������Đ����I���܂Ŏ��̃R�}���h�̎��s��҂��܂��B",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedAnimationUseWeight, new GUIContent("Weight���g��"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "�I�u�W�F�N�g�̈ړ������ɉ�����Weight��ύX���܂��B" +
                                        "1�}�X=0.01�Ƃ��A�ړ������ɔ�Ⴕ�܂��B" +
                                        "����𖳌��ɂ����Weight��1�ŌŒ�ł��B",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    serializedAnimationRotationMode.enumValueIndex = EditorGUI.Popup(
                                        ViewPortRect,
                                        "�����ݒ�",
                                        serializedAnimationRotationMode.enumValueIndex,
                                        FriendAction.Animation_RotationTypeSelectorLabel
                                    );
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "�A�j���[�V�����Đ����̌����̕ύX�ɂ��Ă̐ݒ�ł��B",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                        }
                        break;
                    case FriendActionType.ResetAnimation:
                        {
                            // �A�j���[�V�������Z�b�g
                        }
                        break;
                    case FriendActionType.MoveToCell:
                        {
                            // �Z���Ɉړ�
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedMoveToCellMoveDestinationRelative, new GUIContent("�ړ���(���΍��W)"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "�ړ�����w�肵�܂��B���g����̑��΍��W�ŋL�q���܂��B" +
                                        "���̍��ڂ͒ʏ�A0�ɂ��Ă����ĉ������B",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedMoveToCellAddClickedPos, new GUIContent("�N���b�N�����ʒu���N�_�Ƃ���"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "�X�L���g�p���ɃN���b�N�����ʒu���ړ���̎w��̊�ɂ��܂��B�ʏ�̓I���ɂ��܂��B",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                        }
                        break;
                    case FriendActionType.Rotate:
                        {
                            // ��]
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    serializedRotateRotationDirection.enumValueIndex = EditorGUI.Popup(
                                        ViewPortRect,
                                        "��]��",
                                        serializedRotateRotationDirection.enumValueIndex,
                                        RotationDirectionUtil.DirectionNames
                                    );
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 1;
                                    GUI.Label(
                                        ViewPortRect,
                                        "��]�ʂ��w�肵�܂��B",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 1 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    serializedRotateRelativeTo.enumValueIndex = EditorGUI.Popup(
                                        ViewPortRect,
                                        "��]�",
                                        serializedRotateRelativeTo.enumValueIndex,
                                        FriendAction.Rotate_RelativeToSelectorLabel
                                    );
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 1;
                                    GUI.Label(
                                        ViewPortRect,
                                        "��]�̊�ƂȂ�����ł��B",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 1 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                        }
                        break;
                    case FriendActionType.MoveToLounge:
                        {
                            // ������Ɉړ�
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedMoveToLoungeMoveDestinationRelative, new GUIContent("�ړ���(���΍��W)"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "�ڕW�n�_���w�肵�܂��B���g����̑��΍��W�ŋL�q���܂��B",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedMoveToLoungeAddClickedPos, new GUIContent("�N���b�N�����ʒu���N�_�Ƃ���"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "�X�L���g�p���ɃN���b�N�����ʒu��ڕW�n�_�̎w��̊�ɂ��܂��B�ʏ�̓I���ɂ��܂��B",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                        }
                        break;
                    case FriendActionType.EndTurn:
                        {
                            // �^�[���I��
                        }
                        break;
                }
                // ���ʍ�
                switch ((FriendActionType)(serializedActionType.intValue))
                {
                    case FriendActionType.MoveToCell:
                    case FriendActionType.MoveToLounge:
                        {
                            ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                            serializedLoungeDestinationMode.enumValueIndex = EditorGUI.Popup(
                                ViewPortRect,
                                "�N�̎�����ɂ���H",
                                serializedLoungeDestinationMode.enumValueIndex,
                                FriendAction.LoungeDestinationTypeSelectorLabel
                            );
                            ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                        }
                        {
                            ViewPortRect.height = EditorGUIUtility.singleLineHeight * 1;
                            GUI.Label(
                                ViewPortRect,
                                "������ɑ������ۂ̑������w�肵�܂��B",
                                LabelStyle);
                            ViewPortRect.y += EditorGUIUtility.singleLineHeight * 1 + EditorGUIUtility.standardVerticalSpacing * 2;
                        }
                        break;
                }
                switch ((FriendActionType)(serializedActionType.intValue))
                {
                    case FriendActionType.EndTurn:
                        break;
                    default:
                        {
                            ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                            EditorGUI.PropertyField(ViewPortRect, serializedWaitSec, new GUIContent("�҂�����"));
                            ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                        }
                        {
                            ViewPortRect.height = EditorGUIUtility.singleLineHeight * 1;
                            GUI.Label(
                                ViewPortRect,
                                "���̃R�}���h�����s������ǂꂾ���҂����w�肵�܂��B",
                                LabelStyle);
                            ViewPortRect.y += EditorGUIUtility.singleLineHeight * 1 + EditorGUIUtility.standardVerticalSpacing * 2;
                        }
                        break;
                }
                ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(ViewPortRect, serializedComment, new GUIContent("�R�����g"));
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;

            var serializedActionType = property.FindPropertyRelative("ActionType");
            switch ((FriendActionType)serializedActionType.intValue)
            {
                case FriendActionType.PlayAnimation:
                    height = EditorGUIUtility.singleLineHeight * 16 + EditorGUIUtility.standardVerticalSpacing * 12 + 10f;
                    break;
                case FriendActionType.ResetAnimation:
                    height = EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 4 + 10f;
                    break;
                case FriendActionType.MoveToCell:
                    height = EditorGUIUtility.singleLineHeight * 12 + EditorGUIUtility.standardVerticalSpacing * 9 + 10f;
                    break;
                case FriendActionType.Rotate:
                    height = EditorGUIUtility.singleLineHeight * 8 + EditorGUIUtility.standardVerticalSpacing * 9 + 10f;
                    break;
                case FriendActionType.MoveToLounge:
                    height = EditorGUIUtility.singleLineHeight * 12 + EditorGUIUtility.standardVerticalSpacing * 9 + 10f;
                    break;
                case FriendActionType.EndTurn:
                    height = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2 + 10f;
                    break;
            }

            return height;
        }
    }
#endif

    public enum FriendActionType
    {
        PlayAnimation=10,
        ResetAnimation=11,
        MoveToCell=20,
        Rotate=21,
        MoveToLounge=30,
        EndTurn=10000
    }
    public static class FriendActionTypeName
    {
        public static string[] Names = new string[] {
            "�A�j���[�V�����Đ�",
            "�A�j���[�V�������Z�b�g",
            "�t�����Y�����Z���Ɉړ�",
            "�t�����Y����]",
            "�t�����Y��������Ɉړ�",
            "�^�[���I��",
        };
    }
}