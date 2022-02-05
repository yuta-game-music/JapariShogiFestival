using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JSF.Game.Tutorial
{
    public class TutorialBackground : MonoBehaviour, IPointerClickHandler
    {
        public TutorialManager TutorialManager;

        public void OnPointerClick(PointerEventData eventData)
        {
            TutorialManager?.OnClickNext();
        }
    }

}