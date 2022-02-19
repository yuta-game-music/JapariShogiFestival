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
        public AppVersion AppVersion;

        // 各種データベース
        public Game.Tutorial.TutorialDatabase TutorialDatabase;

        private void Start()
        {
            DontDestroyOnLoad(this);
            GlobalVariable.Loader = this;

            // 初期ロード
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