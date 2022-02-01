using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace JSF.Title
{
    public class CreditCloser : MonoBehaviour, IPointerClickHandler
    {
        public TitlePageController Controller;
        public void OnPointerClick(PointerEventData eventData)
        {
            StartCoroutine(Controller.SetCreditPageVisible(false));
        }


    }

}