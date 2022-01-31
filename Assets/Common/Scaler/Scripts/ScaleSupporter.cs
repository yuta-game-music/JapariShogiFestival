using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JSF.Common.UI
{
    public class ScaleSupporter : MonoBehaviour
    {
        public Vector2 AimSize = new Vector2(960, 540);
        public void Update()
        {
            RectTransform tf = GetComponent<RectTransform>();
            RectTransform root_tf = tf.parent as RectTransform;
            float zoom = 1;

            Vector2 aim_tf = root_tf.rect.size;
            if (aim_tf.x > AimSize.x)
            {
                zoom /= (AimSize.x / aim_tf.x);
                aim_tf *= (AimSize.x / aim_tf.x);
            }
            if (aim_tf.y > AimSize.y)
            {
                zoom /= (AimSize.y / aim_tf.y);
                aim_tf *= (AimSize.y / aim_tf.y);
            }
            tf.sizeDelta = aim_tf;
            tf.localScale = Vector3.one * zoom;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ScaleSupporter))]
    public class ScaleSupporterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var sup = target as ScaleSupporter;
            sup.Update();
            /*var tf = sup.GetComponent<RectTransform>();
            if (tf)
            {
                tf.anchorMin = tf.anchorMax = Vector2.one * 0.5f;
                tf.sizeDelta = sup.AimSize;
            }*/
        }
    }
#endif
}