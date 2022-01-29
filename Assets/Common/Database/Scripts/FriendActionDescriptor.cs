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
 * アクションの種類を増やしたいときの手順
 * 1. FriendsActionTypeに新たな項目を追加する
 * 2. FriendsActionTypeName.Namesに追加した項目の表示名を追加する 追加する位置はFriendsActionTypeの記述順に合わせること
 * 3. FriendActionクラス内に必要な変数を追加する 命名は(タイプ名)_(変数名)とするのがよい 
 * 　　場合によっては既存の変数を共通変数化してよいが、その際は4.の作業で設定した文字列を変更するのを忘れないようにすること
 * 4. FriendActionDataDrawer.OnGUIメソッド内で3.の変数を操作するためのserializedXXX変数を用意する 引数は3.の変数名(文字列) nameofは使えない
 * 5. FriendActionDataDrawer.OnGUIメソッド内で変数の設定用レイアウトを記述する
 * 6. FriendActionDataDrawer.GetPropertyHeightメソッド内でレイアウトの幅を記述する
 * 7. Friend.OnUseSkill内で処理を記述する
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
            ActionList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "コマンド一覧");
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
        // 送った駒の行き先
        public LoungeDestinationType LoungeDestinationMode = LoungeDestinationType.Mine;

        public enum LoungeDestinationType
        {
            Mine, // 行動者
            Opponent, // 相手(セルリアンの行動パターンには使用不可)
            Possessor, // 所有者
            OpponentOfPossessor // 所有者の相手(対セルリアンならセルリアンに)
        }
        public static readonly string[] LoungeDestinationTypeSelectorLabel = new string[]
        {
            "行動者",
            "相手(セルリアンの行動パターンには使用不可)",
            "所有者",
            "所有者の相手(対セルリアンならセルリアンに)"
        };

        public float WaitSec = 0;
        #endregion
        #region PlayAnimation
        // Skillアニメーションに代入するアニメーション
        // 空欄(None)でデフォルトが用いられる
        public AnimatorOverrideController Animation_Animation;

        // アニメーションが終わるまで待つ(空欄で待たない)
        public bool Animation_WaitForEnd = true;

        // 移動距離に応じてWeightを変えるかどうか
        // 1マス=0.01 ユークリッド距離を考慮するため斜めなら0.014142など
        public bool Animation_UseWeight = true;

        // 回転モード
        public Animation_RotationType Animation_RotationMode = Animation_RotationType.Toward;

        public enum Animation_RotationType
        {
            Toward, // その方向に向かう向き
            Now, // 元の向きを保持
            New, // 新しい向きに変更
            Fixed // 画面上での向きを固定(画面上を上向き)
        }
        public static readonly string[] Animation_RotationTypeSelectorLabel = new string[]
        {
            "その方向に向かう向き",
            "元の向きを保持",
            "新しい向きに変更",
            "画面上での向きを固定(画面上を上向き)"
        };
        #endregion

        #region ResetAnimatinon
        #endregion

        #region MoveToCell
        // 行き先(自身から見た相対座標)
        public Vector2Int MoveToCell_MoveDestinationRelative = new Vector2Int();

        // ↑にクリックした場所を足すかどうか
        public bool MoveToCell_AddClickedPos = true;
        #endregion

        #region Rotate
        // 回転角
        public RotationDirection Rotate_RotationDirection;
        // 回転角の基準
        public Rotation_RelativeTo Rotate_RelativeTo;
        public enum Rotation_RelativeTo
        {
            Toward, // 始点→終点に向かう向き
            Base, // 開始時点の向き
            Now, // 現在の向き
            Fixed // 進行方向上向き
        }
        public static readonly string[] Rotate_RelativeToSelectorLabel = new string[]
        {
            "始点→終点に向かう向き",
            "開始時点の向き",
            "現在の向き",
            "進行方向上向き"
        };
        #endregion

        #region MoveToLounge
        // 持ち駒に送る対象(自身から見た相対座標)
        public Vector2Int MoveToLounge_MoveDestinationRelative = new Vector2Int();

        // ↑にクリックした場所を足すかどうか
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

                // 背景
                var BackgroundRect = new Rect(position)
                {
                    height = GetPropertyHeight(property, label)
                };
                GUI.Box(BackgroundRect,"", GUI.skin.box);

                // コマンド種類
                var ViewPortRect = new Rect(position);
                {
                    ViewPortRect.y = position.y;
                    serializedActionType.enumValueIndex = EditorGUI.Popup(
                        ViewPortRect,
                        "コマンド種類",
                        serializedActionType.enumValueIndex,
                        FriendActionTypeName.Names
                    );
                    ViewPortRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                switch ((FriendActionType)(serializedActionType.intValue))
                {
                    case FriendActionType.PlayAnimation:
                        {
                            // アニメーション再生
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedAnimationAnimation, new GUIContent("アニメーション"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "使用するアニメーションを設定します。" +
                                        "空欄にするとAnimator Override Controllerで設定した" +
                                        "アニメーションが使用されます。",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing*2;
                                }
                            }
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedAnimationWaitForEnd, new GUIContent("終了まで待つ"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "有効にするとアニメーションが再生し終わるまで次のコマンドの実行を待ちます。",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedAnimationUseWeight, new GUIContent("Weightを使う"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "オブジェクトの移動距離に応じてWeightを変更します。" +
                                        "1マス=0.01とし、移動距離に比例します。" +
                                        "これを無効にするとWeightは1で固定です。",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    serializedAnimationRotationMode.enumValueIndex = EditorGUI.Popup(
                                        ViewPortRect,
                                        "向き設定",
                                        serializedAnimationRotationMode.enumValueIndex,
                                        FriendAction.Animation_RotationTypeSelectorLabel
                                    );
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "アニメーション再生時の向きの変更についての設定です。",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                        }
                        break;
                    case FriendActionType.ResetAnimation:
                        {
                            // アニメーションリセット
                        }
                        break;
                    case FriendActionType.MoveToCell:
                        {
                            // セルに移動
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedMoveToCellMoveDestinationRelative, new GUIContent("移動先(相対座標)"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "移動先を指定します。自身からの相対座標で記述します。" +
                                        "この項目は通常、0にしておいて下さい。",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedMoveToCellAddClickedPos, new GUIContent("クリックした位置を起点とする"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "スキル使用時にクリックした位置を移動先の指定の基準にします。通常はオンにします。",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                        }
                        break;
                    case FriendActionType.Rotate:
                        {
                            // 回転
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    serializedRotateRotationDirection.enumValueIndex = EditorGUI.Popup(
                                        ViewPortRect,
                                        "回転量",
                                        serializedRotateRotationDirection.enumValueIndex,
                                        RotationDirectionUtil.DirectionNames
                                    );
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 1;
                                    GUI.Label(
                                        ViewPortRect,
                                        "回転量を指定します。",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 1 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    serializedRotateRelativeTo.enumValueIndex = EditorGUI.Popup(
                                        ViewPortRect,
                                        "回転基準",
                                        serializedRotateRelativeTo.enumValueIndex,
                                        FriendAction.Rotate_RelativeToSelectorLabel
                                    );
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 1;
                                    GUI.Label(
                                        ViewPortRect,
                                        "回転の基準となる向きです。",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 1 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                        }
                        break;
                    case FriendActionType.MoveToLounge:
                        {
                            // 持ち駒に移動
                            {
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedMoveToLoungeMoveDestinationRelative, new GUIContent("移動先(相対座標)"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "目標地点を指定します。自身からの相対座標で記述します。",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.PropertyField(ViewPortRect, serializedMoveToLoungeAddClickedPos, new GUIContent("クリックした位置を起点とする"));
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                                }
                                {
                                    ViewPortRect.height = EditorGUIUtility.singleLineHeight * 2;
                                    GUI.Label(
                                        ViewPortRect,
                                        "スキル使用時にクリックした位置を目標地点の指定の基準にします。通常はオンにします。",
                                        LabelStyle);
                                    ViewPortRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                                }
                            }
                        }
                        break;
                    case FriendActionType.EndTurn:
                        {
                            // ターン終了
                        }
                        break;
                }
                // 共通項
                switch ((FriendActionType)(serializedActionType.intValue))
                {
                    case FriendActionType.MoveToCell:
                    case FriendActionType.MoveToLounge:
                        {
                            ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                            serializedLoungeDestinationMode.enumValueIndex = EditorGUI.Popup(
                                ViewPortRect,
                                "誰の持ち駒にする？",
                                serializedLoungeDestinationMode.enumValueIndex,
                                FriendAction.LoungeDestinationTypeSelectorLabel
                            );
                            ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                        }
                        {
                            ViewPortRect.height = EditorGUIUtility.singleLineHeight * 1;
                            GUI.Label(
                                ViewPortRect,
                                "持ち駒に送った際の送り先を指定します。",
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
                            EditorGUI.PropertyField(ViewPortRect, serializedWaitSec, new GUIContent("待ち時間"));
                            ViewPortRect.y += EditorGUIUtility.singleLineHeight;
                        }
                        {
                            ViewPortRect.height = EditorGUIUtility.singleLineHeight * 1;
                            GUI.Label(
                                ViewPortRect,
                                "このコマンドを実行した後どれだけ待つかを指定します。",
                                LabelStyle);
                            ViewPortRect.y += EditorGUIUtility.singleLineHeight * 1 + EditorGUIUtility.standardVerticalSpacing * 2;
                        }
                        break;
                }
                ViewPortRect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(ViewPortRect, serializedComment, new GUIContent("コメント"));
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
            "アニメーション再生",
            "アニメーションリセット",
            "フレンズを特定セルに移動",
            "フレンズを回転",
            "フレンズを持ち駒に移動",
            "ターン終了",
        };
    }
}