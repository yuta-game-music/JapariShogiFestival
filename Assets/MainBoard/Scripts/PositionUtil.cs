using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PositionUtil
{
    // 極座標変換みたいなことをする
    // Circle(r)において、(0,r)から左回りにs回移動した地点をP(r,s)とし、P(r,s)=vecとなるsを求める
    public static void CalcCirclePos(Vector2Int vec, out int r, out int circle_pos)
    {
        // 点(x,y)について以下の式でr(x,y)を計算する
        r = Mathf.Max(Mathf.Abs(vec.x), Mathf.Abs(vec.y));
        // Circle(R)を「r(x,y)=Rとなるx,yの集合からなる正方形の淵」とする
        // 定義より、Circle(R)に属する格子点は8*r個ある

        if (vec.y == r)
        {
            circle_pos = (vec.x + 8 * r) % (8 * r);
        }
        else if (vec.x == -r)
        {
            circle_pos = 2 * r + (-vec.y);
        }
        else if (vec.y == -r)
        {
            circle_pos = 4 * r + vec.x;
        }
        else
        {
            circle_pos = 6 * r + vec.y;
        }
        return;
    }
    public static Vector2Int CalcVectorFromCirclePos(int r, int circle_pos)
    {
        // R(x,y)=rとなる格子点の数
        int R = 8 * r;

        // circle_posを正規化
        circle_pos = ((circle_pos%R)+R) % R;

        if (circle_pos < r)
        {
            return new Vector2Int(-circle_pos, r);
        }
        circle_pos -= 2 * r;
        if (circle_pos < r)
        {
            return new Vector2Int(-r, -circle_pos);
        }
        circle_pos -= 2 * r;
        if (circle_pos < r)
        {
            return new Vector2Int(circle_pos, -r);
        }
        circle_pos -= 2 * r;
        if (circle_pos < r)
        {
            return new Vector2Int(r, circle_pos);
        }
        circle_pos -= 2 * r;
        if (circle_pos < r)// ここは絶対に満たされる
        {
            return new Vector2Int(-circle_pos, r);
        }
        // ここに来たら何かがおかしい
        throw new System.Exception("Invalid calculation in "+nameof(CalcVectorFromCirclePos));
    }
}
