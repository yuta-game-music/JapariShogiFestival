using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JSF.Database
{
    [CreateAssetMenu(fileName = nameof(FriendsDatabase)+".asset", menuName = "JSF/Database/" + nameof(FriendsDatabase))]
    public class FriendsDatabase : ScriptableObject
    {
        public Friend[] Friends;
        private static FriendsDatabase _static_db;

        public Friend GetFriend(string name)
        {
            foreach(var Friend in Friends)
            {
                if(Friend.Name == name)
                {
                    return Friend;
                }
            }
            Debug.LogError("Friend "+name+" not found! Maybe you have to refresh "+nameof(FriendsDatabase)+".");
            return null;
        }

        public static FriendsDatabase Get()
        {
#if UNITY_EDITOR
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                return _static_db = AssetDatabase.LoadAssetAtPath<FriendsDatabase>("Assets/Common/Database/Friends/FriendsDatabase.asset");
            }
#endif
            if (_static_db) { return _static_db; }

            var myLoadedAssetBundle
            = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "Database", "friends"));
            if (myLoadedAssetBundle == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return null;
            }
            _static_db = myLoadedAssetBundle.LoadAsset<FriendsDatabase>("FriendsDatabase");
            return _static_db;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(FriendsDatabase))]
    public class FriendsDatabaseEditor: Editor
    {
        public override void OnInspectorGUI()
        {
            FriendsDatabase db = target as FriendsDatabase;

            if (GUILayout.Button("Refresh"))
            {
                string[] paths = AssetDatabase.GetAllAssetPaths();
                List<Friend> tmp = new List<Friend>();
                foreach(var path in paths)
                {
                    if (!path.StartsWith("Assets/Common/Database/"))
                    {
                        continue;
                    }
                    if (!path.EndsWith(".asset"))
                    {
                        continue;
                    }
                    Friend f = AssetDatabase.LoadAssetAtPath<Friend>(path);
                    if (f)
                    {
                        tmp.Add(f);
                    }
                }
                db.Friends = tmp.ToArray();
            }

            DrawDefaultInspector();
        }
    }
#endif
}