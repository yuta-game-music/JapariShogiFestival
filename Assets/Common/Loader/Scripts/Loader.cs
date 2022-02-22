using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using JSF.Database;

namespace JSF.Common
{
    public class Loader : MonoBehaviour
    {
        public bool AutoMoveToTitle = true;
        private void Start()
        {
            DontDestroyOnLoad(this);

            // �������[�h
            BetterStreamingAssets.Initialize();
            StartCoroutine(Load());

        }

        private IEnumerator Load()
        {
            yield return FriendsDatabase.Load();
            if (AutoMoveToTitle)
            {
                SceneManager.LoadScene("TitlePage");
            }
        }
    }

}