using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Common.UI
{
    /*
     横方向のスクロールビュー
     */
    public class HorizontalScrollView : MonoBehaviour
    {
        RectTransform TF;
        // Start is called before the first frame update
        void Start()
        {
            TF = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            int child_count = TF.childCount;
            if (child_count == 0) { return; }
            Vector2 pos;// = (TF.GetChild(0) as RectTransform)?.anchoredPosition ?? Vector2.zero;
            pos.x = -TF.rect.width/2 + (TF.GetChild(0) as RectTransform)?.rect.width/2 ?? 0;
            pos.y = (TF.GetChild(0) as RectTransform)?.anchoredPosition.y ?? 0;
            float height = TF.rect.height;
            float width = 0;
            
            for(var i = 0; i < child_count; i++)
            {
                if(i>0 && TF.GetChild(i-1) is RectTransform prev_tf)
                {
                    pos.x += prev_tf.rect.width;
                }
                if(TF.GetChild(i) is RectTransform now_tf)
                {
                    if (now_tf.sizeDelta.y > 0)
                    {
                        now_tf.sizeDelta *= (height / now_tf.sizeDelta.y);
                        now_tf.anchoredPosition = pos;
                        width += now_tf.sizeDelta.x;
                    }
                }
            }
            TF.sizeDelta = new Vector2(width, TF.sizeDelta.y);
        }
    }

}