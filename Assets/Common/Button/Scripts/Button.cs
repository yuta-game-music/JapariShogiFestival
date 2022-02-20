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

        public bool CanInteract = true;
        public void SetAllColor(Color NormalColor)
        {
            this.NormalColor = NormalColor;
            this.HoveredColor = Color.Lerp(NormalColor, Color.black, 0.2f);
            this.ClickedColor = Color.Lerp(NormalColor, Color.red, 0.2f);
        }
        public abstract void OnClick();
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (CanInteract)
            {
                Image.color = ClickedColor;
                Util.PlaySE(ClickSound);
                OnClick();
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (CanInteract)
            {
                Image.color = HoveredColor;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            Image.color = NormalColor;
        }
        public virtual void Start()
        {
            Image.color = NormalColor;
        }
    }

}