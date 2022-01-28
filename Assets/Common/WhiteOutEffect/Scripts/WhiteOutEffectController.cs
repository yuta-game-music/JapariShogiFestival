using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Common.UI
{
    public class WhiteOutEffectController : MonoBehaviour
    {
        public Animator Animator;
        public float WhiteOutTime = 1f;
        public float WhiteInTime = 1f;
        private int layer_id = 0;
        // Start is called before the first frame update
        void Start()
        {
            Animator.SetFloat("WhiteOutSpeed",1/Mathf.Max(WhiteOutTime,0.01f));
            Animator.SetFloat("WhiteInSpeed",1/Mathf.Max(WhiteInTime,0.01f));
            layer_id = Animator.GetLayerIndex("WhiteOut");
        }

        // ”’‰æ–Ê‚ªŠ®‘S‚ÉÁ‚¦‚é‚Ì‚ð‘Ò‚Â
        public IEnumerator WaitForWhiteOut()
        {
            yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(layer_id).IsName("Clear"));
        }

        // ”’‚­‚·‚é
        public IEnumerator PlayWhiteIn()
        {
            Animator.SetBool("WhiteOut", true);
            yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(layer_id).IsName("WhiteOut"));
        }
    }

}