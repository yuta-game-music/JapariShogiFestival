using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Database;

namespace JSF.Common.FriendsSelector
{
    public class FriendsSelectorController : MonoBehaviour
    {
        Animator anim;
        public Transform FriendsGrid;
        public GameObject FriendCellPrefab;

        private Friend[] Friends;

        private Friend selectedFriend = null;
        private bool selected = false;
        public IEnumerator ShowSelector(int PlayerID, int FriendsPos)
        {
            Init();

            anim = GetComponent<Animator>();
            int ShowHideAnimatorLayer = anim.GetLayerIndex("ShowHide");
            anim.SetBool("Show", true);
            yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(ShowHideAnimatorLayer).IsName("Shown"));

            Friends = FriendsDatabase.Get().Friends;
            for(var i = 0; i < Friends.Length; i++)
            {
                var cellObj = Instantiate(FriendCellPrefab);
                cellObj.transform.SetParent(FriendsGrid, false);
                
                var cellController = cellObj.GetComponent<FriendsCellController>();
                cellController.Friend = Friends[i];
                cellController.Controller = this;

                //yield return new WaitForSeconds(0.25f);
            }

            yield return new WaitUntil(()=>selected);
            GlobalVariable.Players[PlayerID].Friends[FriendsPos] = selectedFriend;
            anim.SetBool("Show", false);
            yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(ShowHideAnimatorLayer).IsName("Hidden"));
            yield return null;
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void Init()
        {
            // 既に並べてあるフレンズ一覧を消す
            for(var i = FriendsGrid.childCount - 1; i >= 0; i--)
            {
                Destroy(FriendsGrid.GetChild(i).gameObject);
            }

            // 各種選択状態を解除
            selected = false;
            selectedFriend = null;
        }

        public void OnSelectFriend(Friend friend)
        {
            selectedFriend = friend;
            selected = true;
        }
    }

}