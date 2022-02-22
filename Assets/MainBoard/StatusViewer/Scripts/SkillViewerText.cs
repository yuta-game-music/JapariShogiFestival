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

                // �I�𒆂̃t�����Y�̏ڍ�
                if (Manager.GameUI.SelectedFriendOnBoard)
                {
                    Friend friend = Manager.GameUI.SelectedFriendOnBoard.Friend;
                    sb.Append(friend.Name + " - �X�L�����\n");
                    if (SkillSheetViewer)
                    {
                        Vector2Int? _Selected = SkillSheetViewer.SelectedCell;
                        if (_Selected.HasValue)
                        {
                            Vector2Int Selected = _Selected.Value;
                            sb.Append("�ʏ�X�L���F");
                            bool hasNormalSkill = false;
                            if (friend.CanNormalMove(Selected))
                            {
                                sb.Append("�ړ�OK ");
                                hasNormalSkill = true;
                            }
                            if (friend.CanRotateTo(Selected))
                            {
                                sb.Append("���̏��]OK");
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
                                sb.Append("����X�L���F" + SkillMap.Name+"\n");
                                sb.Append("�K�v�T���h�X�^�[�ʁF"+SkillMap.NeededSandstar+"\n");
                                sb.Append(SkillMap.Description+"\n");
                            }
                            else
                            {
                                sb.Append("����X�L���F---\n");
                            }
                        }
                        else
                        {
                            sb.Append("���̃Z����I������ƃX�L���ڍׂ��\������܂�\n");
                        }
                    }
                }
                else
                {
                    sb.Append("�t�����Y��I������Əڍׂ��\������܂�\n");
                }
                Text.text = sb.ToString();
            }
        }
    }

}