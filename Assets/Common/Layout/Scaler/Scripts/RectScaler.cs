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
                    // 横幅に合わせて縦幅を変える
                    tf.sizeDelta = new Vector2(ReferenceTF.rect.width, ReferenceTF.rect.width * Ratio);
                    break;
                case PivotDirection.Vertical:
                    // 縦幅に合わせて横幅を変える
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