using JSF.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Title
{
    public class CreditFriendsList : MonoBehaviour
    {
        public GameObject FriendsPrefab;
        // Start is called before the first frame update
        void Start()
        {
            FriendsDatabase db = FriendsDatabase.Get();
            for(int ci=transform.childCount-1;ci>=0; ci--)
            {
                Destroy(transform.GetChild(ci).gameObject);
            }

            float h = 0;
            foreach(var f in db.Friends)
            {
                var obj = Instantiate(FriendsPrefab);
                obj.transform.SetParent(transform, false);
                obj.transform.localPosition = new Vector3(0, h, 0);

                CreditFriendsCell cell = obj.GetComponent<CreditFriendsCell>();
                cell?.SetFriend(f);

                h += obj.GetComponent<RectTransform>()?.rect.height ?? 150;
            }

            RectTransform tf = GetComponent<RectTransform>();
            if (tf)
            {
                tf.sizeDelta = new Vector2(0, h);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}