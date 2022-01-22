using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Game
{
    public enum RotationDirection
    {
        FORWARD, FORWARD_LEFT, LEFT, BACKWARD_LEFT, BACKWARD, BACKWARD_RIGHT, RIGHT, FORWARD_RIGHT
    }

    public class RotationDirectionUtil
    {
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
    }
}