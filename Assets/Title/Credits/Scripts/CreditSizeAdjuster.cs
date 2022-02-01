using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Title
{
    [ExecuteAlways]
    public class CreditSizeAdjuster : MonoBehaviour
    {
        RectTransform tf;
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
                float h = 0;
                for(int i=0; i<tf.childCount; i++)
                {
                    if(tf.GetChild(i) is RectTransform tfc)
                    {
                        var _h = tfc.rect.height;
                        tfc.localPosition = new Vector3(0, -h, 0);
                        tfc.anchorMin = new Vector2(0, 1);
                        tfc.anchorMax = new Vector2(1, 1);
                        tfc.pivot = new Vector2(0.5f, 1);
                        h += _h;
                    }
                }
                tf.anchorMin = new Vector2(0, 1);
                tf.anchorMax = new Vector2(1, 1);
                tf.sizeDelta = new Vector2(0, h);
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