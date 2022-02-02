using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Common.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class RectScaler : MonoBehaviour
    {
        public RectTransform ReferenceTF;
        private RectTransform tf;
        public PivotDirection PivotSize;
        public float Ratio = 1;
        // Start is called before the first frame update
        void Start()
        {
            tf = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            ReferenceTF = (ReferenceTF==null)?tf:ReferenceTF;
            switch (PivotSize)
            {
                case PivotDirection.Horizontal:
                    // ‰¡•‚É‡‚í‚¹‚Äc•‚ğ•Ï‚¦‚é
                    tf.sizeDelta = new Vector2(ReferenceTF.rect.width, ReferenceTF.rect.width * Ratio);
                    break;
                case PivotDirection.Vertical:
                    // c•‚É‡‚í‚¹‚Ä‰¡•‚ğ•Ï‚¦‚é
                    tf.sizeDelta = new Vector2(ReferenceTF.rect.height * Ratio, ReferenceTF.rect.height);
                    break;
            }
        }
    }

    public enum PivotDirection
    {
        Horizontal, Vertical
    }
}