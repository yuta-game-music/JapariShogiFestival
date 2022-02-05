using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Common
{
    public static class Const
    {
        // マウス状態による色変化
        public static readonly Color CellColorNormal = Color.white;
        public static readonly Color CellColorHovered = new Color(0.7f, 0.7f, 0.7f);
        public static readonly Color CellColorSelected = new Color(1f, 0.7f, 0.7f);

        // UIによる動的な色変化
        public static readonly Color CellColorCannotUse = new Color(0.2f, 0.2f, 0.2f);
        public static readonly Color CellColorCanMove = new Color(0.4f, 1f, 0.55f);
        public static readonly Color CellColorCanRotate = new Color(0.4f, 0.4f, 1f);
        public static readonly Color CellColorCanEffectBySkill = new Color(1f, 0.7f, 0.6f);
    }

}