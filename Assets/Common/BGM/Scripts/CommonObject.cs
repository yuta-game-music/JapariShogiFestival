using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using JSF.Database;

namespace JSF.BGM
{
    public class CommonObject : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this);

            // �������[�h
            FriendsDatabase.Get();

            SceneManager.LoadScene("TitlePage");
        }
    }

}