using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSF.Game.UI
{
    public class WaitButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public GameUI GameUI;
        public Image Image;
        public static readonly Color COLOR_NORMAL = new Color(0.6427109f, 0.9021074f, 0.9528302f);
        public static readonly Color COLOR_HIGHLIGHT = new Color(0.57135545f, 0.7010537f, 0.7264151f);
        public static readonly Color COLOR_CLICKED = new Color(0.8f, 0.6f, 0.6f);
        // Start is called before the first frame update
        void Start()
        {
            GameUI = GameUI ?? FindObjectOfType<GameUI>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void OnPointerClick(PointerEventData eventData)
        {
            Image.color = COLOR_CLICKED;
            GameUI.OnClickSkip();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Image.color = COLOR_HIGHLIGHT;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Image.color = COLOR_NORMAL;
        }
    }

}