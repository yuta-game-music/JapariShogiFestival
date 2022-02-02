
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using JSF.Common;

namespace JSF.SettingPage
{
    public class SettingPageButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Image Image;
        Color BaseColor;
        Color ColorByPointer;

        public UnityEvent OnClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            ColorByPointer = Color.red;
            Util.PlaySE(SE.SEType.Click);
            OnClick.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ColorByPointer = Color.gray;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ColorByPointer = Color.white;
        }

        // Start is called before the first frame update
        void Start()
        {
            BaseColor = Image.color;
            OnPointerExit(null);
        }

        // Update is called once per frame
        void Update()
        {
            Image.color = Color.Lerp(BaseColor, ColorByPointer, 0.3f);
        }


    }

}