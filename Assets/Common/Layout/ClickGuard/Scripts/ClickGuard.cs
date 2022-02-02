using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JSF.Common.UI
{
    // クリックしても何も起きないようにしたい領域に設置
    public class ClickGuard : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            
        }
    }

}