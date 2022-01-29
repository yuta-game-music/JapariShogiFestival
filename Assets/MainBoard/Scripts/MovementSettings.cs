using System;

namespace JSF.Game
{
    [Obsolete]
    public struct MovementSettings
    {
        public bool CanMoveNormal;
        public bool CanRotateToward;
        public bool CanEffectBySkill;
        public int NeededSandstarForSkill;

        public MovementSettings(bool CanMoveNormal = false, bool CanRotateToward = false, bool CanEffectBySkill = false, int NeededSandstarForSkill = 1)
        {
            this.CanMoveNormal = CanMoveNormal;
            this.CanRotateToward = CanRotateToward;
            this.CanEffectBySkill = CanEffectBySkill;
            this.NeededSandstarForSkill = NeededSandstarForSkill;
        }
    }

}