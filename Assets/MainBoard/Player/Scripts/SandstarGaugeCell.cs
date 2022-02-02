using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace JSF.Game.Player
{
    public class SandstarGaugeCell : MonoBehaviour
    {
        public Image Image;

        private SandstarGaugeStatus status;
        private SandstarGaugeStatus temp_status;
        private float AnimationTimer = 0;
        private float AnimationLeftLength = 0;

        public readonly static Color ActiveColor = new Color(1, 1, 1, 1);
        public readonly static Color InactiveColor = new Color(0.1f, 0.1f, 0.1f, 1);
        public readonly static Color LitRedColor1 = new Color(1f, 0.1f, 0.1f, 1);
        public readonly static Color LitRedColor2 = new Color(0.1f, 0.1f, 0.1f, 1);

        private void Update()
        {
            AnimationTimer += Time.deltaTime;
            AnimationLeftLength -= Time.deltaTime;
            switch ((AnimationLeftLength>0)?temp_status : status)
            {
                case SandstarGaugeStatus.Active:
                    Image.color = ActiveColor;
                    break;
                case SandstarGaugeStatus.Inactive:
                    Image.color = InactiveColor;
                    break;
                case SandstarGaugeStatus.LitRed:
                    ApplyAnimatedColor(LitRedColor1, LitRedColor2, AnimationTimer, 0.3f);
                    break;
            }
        }

        private void ApplyAnimatedColor(Color Color1, Color Color2, float AnimationTimer, float AnimationFreq)
        {
            AnimationTimer = AnimationTimer % AnimationFreq;
            float v = 1 - Mathf.Abs(AnimationFreq / 2 - AnimationTimer) / (AnimationFreq / 2);
            Image.color = Color.Lerp(Color1, Color2, v);
        }

        public void SetActive(bool active)
        {
            Image.color = active ? ActiveColor : InactiveColor;
        }

        public void SetStatus(SandstarGaugeStatus status)
        {
            if (this.status != status)
            {
                this.status = status;
                this.AnimationTimer = 0;
            }
        }
        public void SetStatus(SandstarGaugeStatus status, float length)
        {
            if(((this.AnimationLeftLength>0)?this.temp_status : this.status) != status)
            {
                this.temp_status = status;
                this.AnimationTimer = 0;
            }
            this.AnimationLeftLength = length;
        }
    }

    public enum SandstarGaugeStatus
    {
        Active, Inactive, LitRed
    }

}