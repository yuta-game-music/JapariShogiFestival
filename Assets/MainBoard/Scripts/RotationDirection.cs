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
            "前","左前","左","左後ろ","後ろ","右後ろ","右","右前"
        };
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
            var res = PositionUtil.CalcVectorFromCirclePos(r, p_aim);

            return res;
        }

        // dirが二次元平面上の隣り合うマスを指すか(=斜めでないか)
        // アニメーション距離に使う
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

        // vecから最も近いrotation-degreeを計算する
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

        // fromPosにいる回転角度fromDirのフレンズからtoPosを見たときの相対座標を求める
        public static Vector2Int GetRelativePos(Vector2Int fromPos, RotationDirection fromDir, Vector2Int toPos)
        {
            return GetRotatedVector(toPos - fromPos, Invert(fromDir));
        }
    }
}