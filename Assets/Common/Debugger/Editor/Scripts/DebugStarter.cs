using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace JSF.Debug
{
    public class DebugStarter
    {
        [MenuItem("JSF/Start Debug")]
        public static void StartGame()
        {
            EditorSceneManager.OpenScene("Assets/Common/Scenes/InitialLoad.unity",OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
    }

}