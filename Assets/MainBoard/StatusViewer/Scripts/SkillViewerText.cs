using JSF.Database;
using System.Text;
using TMPro;
using UnityEngine;

namespace JSF.Game.UI
{
    public class SkillViewerText : MonoBehaviour
    {
        public GameManager Manager;
        public SkillSheetViewer SkillSheetViewer;

        public TMP_Text Text;

        // Update is called once per frame
        void Update()
        {
            if(Manager && Text)
            {
                StringBuilder sb = new StringBuilder();

                // 選択中のフレンズの詳細
                if (Manager.GameUI.SelectedFriendOnBoard)
                {
                    Friend friend = Manager.GameUI.SelectedFriendOnBoard.Friend;
                    sb.Append(friend.Name + " - スキル情報\n");
                    if (SkillSheetViewer)
                    {
                        Vector2Int? _Selected = SkillSheetViewer.SelectedCell;
                        if (_Selected.HasValue)
                        {
                            Vector2Int Selected = _Selected.Value;
                            sb.Append("通常スキル：");
                            bool hasNormalSkill = false;
                            if (friend.CanNormalMove(Selected))
                            {
                                sb.Append("移動OK ");
                                hasNormalSkill = true;
                            }
                            if (friend.CanRotateTo(Selected))
                            {
                                sb.Append("その場回転OK");
                                hasNormalSkill = true;
                            }
                            if (!hasNormalSkill)
                            {
                                sb.Append("---");
                            }
                            sb.Append("\n\n");

                            SkillMap? _SkillMap = friend.GetSkillMapByPos(Selected);
                            if (_SkillMap.HasValue)
                            {
                                SkillMap SkillMap = _SkillMap.Value;
                                sb.Append("特殊スキル：" + SkillMap.Name+"\n");
                                sb.Append("必要サンドスター量："+SkillMap.NeededSandstar+"\n");
                                sb.Append(SkillMap.Description+"\n");
                            }
                            else
                            {
                                sb.Append("特殊スキル：---\n");
                            }
                        }
                        else
                        {
                            sb.Append("↑のセルを選択するとスキル詳細が表示されます\n");
                        }
                    }
                }
                else
                {
                    sb.Append("フレンズを選択すると詳細が表示されます\n");
                }
                Text.text = sb.ToString();
            }
        }
    }

}