using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JSF.Database
{
    public class FriendsDatabase
    {
        private List<Friend> friends = new List<Friend>();
        public Friend[] Friends { get => friends.ToArray(); }
        private static FriendsDatabase _static_db;

        public Friend GetFriend(string name)
        {
            foreach(var Friend in friends)
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
            if (_static_db!=null) { return _static_db; }
            _static_db = new FriendsDatabase();
#if UNITY_EDITOR
            if (Application.platform == RuntimePlatform.WindowsEditor && Directory.Exists("Assets/ServerUtil/Database/Friends"))
            {
                string[] files = Directory.GetFiles("Assets/ServerUtil/Database/Friends/", "Friend.asset", SearchOption.AllDirectories);
                foreach(var file in files)
                {
                    Debug.Log(file);
                    Friend f = AssetDatabase.LoadAssetAtPath<Friend>(file);
                    if (f != null)
                    {
                        _static_db.friends.Add(f);
                        Debug.Log("Loaded " + f.Name);
                    }
                    else
                    {
                        Debug.LogWarning("not a friend!");
                    }
                }
                return _static_db;
            }
#endif
            // AssetBundlesÇ©ÇÁê∂ê¨
            {
                string[] files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "Database", "Friends"), "*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if (file.EndsWith(".meta")) { continue; }
                    var loaded = AssetBundle.LoadFromFile(file);
                    if (loaded == null)
                    {
                        Debug.LogWarning("Failed to load "+file);
                        continue;
                    }
                    Friend f = loaded.LoadAsset<Friend>("friend");
                    if (f == null)
                    {
                        Debug.LogWarning("Failed to load " + file + " (not a friend)");
                        continue;
                    }
                    _static_db.friends.Add(f);
                }
            }
            return _static_db;
        }
    }
}