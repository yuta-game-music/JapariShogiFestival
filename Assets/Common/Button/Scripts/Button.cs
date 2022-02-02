using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSF.Common.UI
{
    public abstract class Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Color NormalColor = new Color(0.9f,0.9f,0.9f);
        public Color HoveredColor = new Color(0.6f,0.6f,0.6f);
        public Color ClickedColor = new Color(1,0.7f,0.7f);

        public Image Image;
        public AudioClip ClickSound;
        public void SetAllColor(Color NormalColor)
        {
            this.NormalColor = NormalColor;
            this.HoveredColor = Color.Lerp(NormalColor, Color.black, 0.2f);
            this.ClickedColor = Color.Lerp(NormalColor, Color.red, 0.2f);
        }
        public abstract void OnClick();
        public void OnPointerClick(PointerEventData eventData)
        {
            Image.color = ClickedColor;
            Util.PlaySE(ClickSound);
            OnClick();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Image.color = HoveredColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Image.color = NormalColor;
        }
        public void Start()
        {
            Image.color = NormalColor;
        }
    }

}