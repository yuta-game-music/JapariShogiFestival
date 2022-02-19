using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using JSF.Common;

namespace JSF.Debug
{
    [InitializeOnLoad]
    public class DebugStarter
    {
        public static Loader Loader;

        static DebugStarter()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        [MenuItem("JSF/Start Debug")]
        public static void StartGame()
        {
            EditorSceneManager.OpenScene("Assets/InitialLoad/Scenes/InitialLoad.unity",OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();

        }

        public static void OnEditorUpdate()
        {

            // CommonObj‚ð”z’u
            if (EditorApplication.isPlaying)
            {
                if (!Loader)
                {
                    var LoaderObject = GameObject.Find("CommonObj");
                    Loader = LoaderObject?.GetComponent<Loader>();
                }
                if (!Loader)
                {
                    GameObject common_obj_prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Common/Prefabs/CommonObj.prefab");
                    if (common_obj_prefab)
                    {
                        var common_obj = Object.Instantiate(common_obj_prefab);

                        var loader = common_obj.GetComponent<Loader>();
                        if (loader)
                        {
                            loader.AutoMoveToTitle = false;
                            Loader = loader;
                        }
                    }
                }
            }
            else
            {
                if (Loader)
                {
                    Object.Destroy(Loader.gameObject);
                }
            }
        }
    }

}