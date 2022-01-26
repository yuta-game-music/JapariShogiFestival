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
        public static readonly Color COLOR_NORMAL = Color.white;
        public static readonly Color COLOR_HIGHLIGHT = new Color(0.8f,0.8f, 0.8f);
        public static readonly Color COLOR_CLICKED = new Color(0.6f, 0.6f, 0.6f);

        private Color PlayerColor = Color.white;
        private Color MouseColor = COLOR_NORMAL;
        // Start is called before the first frame update
        void Start()
        {
            GameUI = GameUI ?? FindObjectOfType<GameUI>();
        }

        // Update is called once per frame
        void Update()
        {
            Image.color = Color.Lerp(PlayerColor, MouseColor, 0.5f);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            MouseColor = COLOR_CLICKED;
            GameUI.OnClickSkip();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            MouseColor = COLOR_HIGHLIGHT;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MouseColor = COLOR_NORMAL;
        }

        public void SetPlayerColor(Color color)
        {
            PlayerColor = Color.Lerp(color,Color.white, 0.8f);
        }
    }

}