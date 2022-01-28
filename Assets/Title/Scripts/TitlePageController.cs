using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Title
{
    public class TitlePageController : MonoBehaviour
    {
        public Animator WhiteOutAnimator;
        public IEnumerator PlayWhiteOutEffect()
        {
            var layer_id = WhiteOutAnimator.GetLayerIndex("WhiteOut");
            WhiteOutAnimator.SetBool("WhiteOut", true);

            yield return new WaitUntil(() => WhiteOutAnimator.GetCurrentAnimatorStateInfo(layer_id).IsName("End"));
        }
    }

}