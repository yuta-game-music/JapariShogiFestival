using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JSF.Database
{
    [CreateAssetMenu(fileName = nameof(FriendsDatabase)+".asset", menuName = "JSF/Database/" + nameof(FriendsDatabase))]
    public class FriendsDatabase : ScriptableObject
    {
        public Friend[] Friends;

        public Friend GetFriend<T>() where T : Friend
        {
            foreach(var Friend in Friends)
            {
                if(Friend is T ft)
                {
                    return Friend;
                }
            }
            Debug.LogError("Friend "+nameof(T)+" not found! Maybe you have to refresh "+nameof(FriendsDatabase)+".");
            return null;
        }

        public static FriendsDatabase Get()
        {
            return AssetDatabase.LoadAssetAtPath<FriendsDatabase>("Assets/Common/Database/Friends/FriendsDatabase.asset");
        }
    }

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
}