using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using JSF.Database;

namespace JSF.Common
{
    public class CommonObject : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this);

            // èâä˙ÉçÅ[Éh
            BetterStreamingAssets.Initialize();
            StartCoroutine(Load());

        }

        private IEnumerator Load()
        {
            yield return FriendsDatabase.Load();
            SceneManager.LoadScene("TitlePage");
        }
    }

}