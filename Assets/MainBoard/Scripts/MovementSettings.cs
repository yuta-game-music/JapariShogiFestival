using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Game
{
    public struct MovementSettings
    {
        public bool CanMoveNormal;
        public bool CanRotateToward;
        public bool CanEffectBySkill;

        public MovementSettings(bool CanMoveNormal = false, bool CanRotateToward = false, bool CanEffectBySkill = false)
        {
            this.CanMoveNormal = CanMoveNormal;
            this.CanRotateToward = CanRotateToward;
            this.CanEffectBySkill = CanEffectBySkill;
        }
    }

}