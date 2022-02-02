using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Title
{
    public class TitlePageController : MonoBehaviour
    {
        public Animator WhiteOutAnimator;
        public Animator CreditPageAnimation;
        public IEnumerator PlayWhiteOutEffect()
        {
            var layer_id = WhiteOutAnimator.GetLayerIndex("WhiteOut");
            WhiteOutAnimator.SetBool("WhiteOut", true);

            yield return new WaitUntil(() => WhiteOutAnimator.GetCurrentAnimatorStateInfo(layer_id).IsName("End"));
        }
        public IEnumerator SetCreditPageVisible(bool visible)
        {
            var layer_id = CreditPageAnimation.GetLayerIndex("ShowHide");
            CreditPageAnimation.SetBool("Show", visible);

            yield return new WaitUntil(() => CreditPageAnimation.GetCurrentAnimatorStateInfo(layer_id).IsName(visible ? "Shown" : "Hidden"));
        }
    }

}