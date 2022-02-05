using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Common
{
    [ExecuteAlways]
    public class HorizontalListSizeAdjuster : MonoBehaviour
    {
        RectTransform tf;
        public float AnchorYMin = 0;
        public float AnchorYMax = 1;
        public float HeightDelta = 0;
        // Start is called before the first frame update
        void Start()
        {
            tf = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            if (tf)
            {
                float w = 0;
                for(int i=0; i<tf.childCount; i++)
                {
                    if(tf.GetChild(i) is RectTransform tfc)
                    {
                        float scaleRate = 1;
                        if (tfc.rect.height > 0)
                        {
                            scaleRate = tfc.rect.width / tfc.rect.height;
                        }
                        var _w = tfc.rect.width;
                        tfc.anchorMin = new Vector2(1, 0);
                        tfc.anchorMax = new Vector2(1, 1);
                        tfc.pivot = new Vector2(0.5f, 0.5f);
                        tfc.localPosition = new Vector3(w, 0, 0);
                        tfc.sizeDelta = new Vector2(tfc.rect.height * scaleRate, 0);
                        w += _w;
                    }
                }
                tf.anchorMin = new Vector2(0, AnchorYMin);
                tf.anchorMax = new Vector2(0, AnchorYMax);
                tf.sizeDelta = new Vector2(w, HeightDelta);
            }
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            Update();
        }
#endif
    }

}