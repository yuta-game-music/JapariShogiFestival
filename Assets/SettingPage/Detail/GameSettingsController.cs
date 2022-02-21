using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JSF.SettingPage
{
    public class GameSettingsController : MonoBehaviour, IPointerClickHandler
    {
        public SettingPageController SettingPageController;
        public DetailRow[] Rows;

        private void Start()
        {
            UpdateValues();
        }

        public void UpdateValues()
        {
            bool updated;
            do
            {
                updated = false;
                foreach (var row in Rows)
                {
                    if (row != null)
                    {
                        updated = row.UpdateValue() || updated;
                    }
                }
            } while (updated);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            StartCoroutine(SettingPageController.SetGameSettingsPageVisible(false));
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameSettingsController))]
    public class GameSettingsControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            if(GUILayout.Button("Reload Rows"))
            {
                serializedObject.Update();

                SerializedProperty RowsProp = serializedObject.FindProperty(nameof(GameSettingsController.Rows));

                var rows = (target as GameSettingsController).transform.GetComponentsInChildren<DetailRow>();
                rows = rows.Reverse().ToArray();
                RowsProp.arraySize = rows.Length;
                for(var i = 0; i < rows.Length; i++)
                {
                    var RowElemProp = RowsProp.GetArrayElementAtIndex(i);
                    RowElemProp.objectReferenceValue = rows[i];
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}