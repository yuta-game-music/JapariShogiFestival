using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PositionUtil
{
    // �ɍ��W�ϊ��݂����Ȃ��Ƃ�����
    // Circle(r)�ɂ����āA(0,r)���獶����s��ړ������n�_��P(r,s)�Ƃ��AP(r,s)=vec�ƂȂ�s�����߂�
    public static void CalcCirclePos(Vector2Int vec, out int r, out int circle_pos)
    {
        // �_(x,y)�ɂ��Ĉȉ��̎���r(x,y)���v�Z����
        r = Mathf.Max(Mathf.Abs(vec.x), Mathf.Abs(vec.y));
        // Circle(R)���ur(x,y)=R�ƂȂ�x,y�̏W������Ȃ鐳���`�̕��v�Ƃ���
        // ��`���ACircle(R)�ɑ�����i�q�_��8*r����

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
        // R(x,y)=r�ƂȂ�i�q�_�̐�
        int R = 8 * r;

        // circle_pos�𐳋K��
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
        if (circle_pos < r)// �����͐�΂ɖ��������
        {
            return new Vector2Int(-circle_pos, r);
        }
        // �����ɗ����牽������������
        throw new System.Exception("Invalid calculation in "+nameof(CalcVectorFromCirclePos));
    }
}
