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

        // vecを原点中心にdirで回転させるスクリプト
        // vecは相対座標を用いるとよい
        public static Vector2Int GetRotatedVector(Vector2Int vec, RotationDirection dir)
        {
            PositionUtil.CalcCirclePos(vec, out int r, out int p_base);
            int p_aim = p_base + r * (int)dir;
            return PositionUtil.CalcVectorFromCirclePos(r, p_aim);
        }
    }
}