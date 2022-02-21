using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace JSF.SettingPage
{
    public abstract class DetailRow : MonoBehaviour
    {
        public TMP_Text currentValueViewer;
        public abstract string CurrentValueToString();
        private GameSettingsController controller;

        // Start is called before the first frame update
        void Start()
        {
            controller = GetComponentInParent<GameSettingsController>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void UpdateAllValue()
        {
            controller.UpdateValues();
        }
        public abstract bool ValidateValue();
        public bool UpdateValue()
        {
            bool updated = ValidateValue();
            if (currentValueViewer)
            {
                currentValueViewer.text = CurrentValueToString();
            }
            return updated;
        }
    }

}