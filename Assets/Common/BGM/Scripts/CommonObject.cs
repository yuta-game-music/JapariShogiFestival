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

            // ‰Šúƒ[ƒh
            FriendsDatabase.Get();

            SceneManager.LoadScene("TitlePage");
        }
    }

}