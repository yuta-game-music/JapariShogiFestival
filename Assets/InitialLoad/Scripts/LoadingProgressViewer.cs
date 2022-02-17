using UnityEngine;
using TMPro;
using JSF.Database;

namespace JSF.Common
{
    public class LoadingProgressViewer : MonoBehaviour
    {
        public TMP_Text text;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            text.text = FriendsDatabase.LoadingStatus;
        }
    }

}