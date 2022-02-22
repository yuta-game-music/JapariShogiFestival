using System.Collections;
using System;
using UnityEngine;

namespace JSF.Game
{
    [Serializable]
    public enum RotationDirection
    {
        FORWARD, FORWARD_LEFT, LEFT, BACKWARD_LEFT, BACKWARD, BACKWARD_RIGHT, RIGHT, FORWARD_RIGHT
    }

    public class RotationDirectionUtil
    {
        public static readonly string[] DirectionNames = new string[]
        {
            "�O","���O","��","�����","���","�E���","�E","�E�O"
        };
        public static RotationDirection RotateLeft(RotationDirection base_rot, int step)
        {
            return (RotationDirection)(((int)base_rot + step) % 8);
        }
        public static RotationDirection RotateRight(RotationDirection base_rot, int step)
        {
            return (RotationDirection)((((int)base_rot - step) % 8 + 8) % 8);
        }

        // vec�����_���S��dir�ŉ�]������X�N���v�g
        // vec�͑��΍��W��p����Ƃ悢
        public static Vector2Int GetRotatedVector(Vector2Int vec, RotationDirection dir)
        {
            PositionUtil.CalcCirclePos(vec, out int r, out int p_base);
            int p_aim = p_base + r * (int)dir;
            var res = PositionUtil.CalcVectorFromCirclePos(r, p_aim);

            return res;
        }

        // dir���񎟌����ʏ�ׂ̗荇���}�X���w����(=�΂߂łȂ���)
        // �A�j���[�V���������Ɏg��
        public static bool IsStraight(RotationDirection dir)
        {
            return (int)dir % 2 == 0;
        }

        public static RotationDirection Invert(RotationDirection dir)
        {
            return (RotationDirection)((16 - (int)dir) % 8);
        }

        public static RotationDirection Merge(RotationDirection one, RotationDirection other)
        {
            return (RotationDirection)(((int)one + (int)other) % 8);
        }

        public static float GetRotationDegree(RotationDirection dir)
        {
            return (int)dir * 45;
        }

        // vec����ł��߂�rotation-degree���v�Z����
        public static RotationDirection CalcRotationDegreeFromVector(Vector2 vec)
        {
            if (vec.x == 0 && vec.y == 0) { return RotationDirection.FORWARD; }
            var d = (Mathf.Atan2(vec.y,vec.x)*Mathf.Rad2Deg+180)%360;
            if (d < 0 + 22.5)
            {
                return RotationDirection.RIGHT;
            }
            else if (d < 45 + 22.5)
            {
                return RotationDirection.FORWARD_RIGHT;
            }
            else if (d < 90 + 22.5)
            {
                return RotationDirection.FORWARD;
            }
            else if (d < 135 + 22.5)
            {
                return RotationDirection.FORWARD_LEFT;
            }
            else if (d < 180 + 22.5)
            {
                return RotationDirection.LEFT;
            }
            else if (d < 225 + 22.5)
            {
                return RotationDirection.BACKWARD_LEFT;
            }
            else if (d < 270 + 22.5)
            {
                return RotationDirection.BACKWARD;
            }
            else if (d < 315 + 22.5)
            {
                return RotationDirection.BACKWARD_RIGHT;
            }
            else
            {
                return RotationDirection.RIGHT;
            }
        }

        // fromPos�ɂ����]�p�xfromDir�̃t�����Y����toPos�������Ƃ��̑��΍��W�����߂�
        public static Vector2Int GetRelativePos(Vector2Int fromPos, RotationDirection fromDir, Vector2Int toPos)
        {
            return GetRotatedVector(toPos - fromPos, Invert(fromDir));
        }
    }
}