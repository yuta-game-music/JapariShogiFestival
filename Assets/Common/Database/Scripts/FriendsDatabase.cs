using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using JSF.Common;
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
                if (Friend.FileName == name)
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

#if UNITY_EDITOR
            // (エディタ専用)DBから読み込み
            if (Application.platform == RuntimePlatform.WindowsEditor && Directory.Exists("Assets/ServerUtil/Database/Friends"))
            {
                _static_db = new FriendsDatabase();
                string[] files = Directory.GetFiles("Assets/ServerUtil/Database/Friends/", "Friend.asset", SearchOption.AllDirectories);
                foreach (var file in files)
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

            // ロードが終わっていない場合の緊急措置
            GameObject loaderObj = new GameObject();
            loaderObj.name = "loader";
            var loader = loaderObj.AddComponent<Loader>();
            loader.AutoMoveToTitle = false;
            return _static_db;
        }

        public static IEnumerator Load()
        {

            _static_db = new FriendsDatabase();
#if UNITY_EDITOR
            if (Application.platform == RuntimePlatform.WindowsEditor && Directory.Exists("Assets/ServerUtil/Database/Friends"))
            {
                string[] files = Directory.GetFiles("Assets/ServerUtil/Database/Friends/", "Friend.asset", SearchOption.AllDirectories);
                foreach (var file in files)
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
                yield break;
            }
#endif
            // StreamingAssetsのデータをPersistentPath以下に書き込み
            {
                if(!Directory.Exists(Path.Combine(Util.GetSavedFileDirectoryPath(), "database", "friends"))){
                    Directory.CreateDirectory(Path.Combine(Util.GetSavedFileDirectoryPath(), "database", "friends"));
                }
                string[] files = BetterStreamingAssets.GetFiles(Path.Combine("database", "friends"), "*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if (file.EndsWith(".meta")) { continue; }

                    string FriendName = Path.GetFileName(file);

                    using (var readStream = BetterStreamingAssets.OpenRead(file))
                    {
                        using (var writeStream = File.OpenWrite(Path.Combine(Util.GetSavedFileDirectoryPath(), "database", "friends", FriendName)))
                        {
                            while (true)
                            {
                                int b = readStream.ReadByte();
                                if (b < 0) { break; }
                                writeStream.WriteByte((byte)b);
                            }
                        }
                    }
                }
            }
            // サーバーにアクセスしフレンズ一覧を読み込み
            {
                UnityWebRequest req = UnityWebRequest.Get("https://yutagamemusic.ddns.net/JSF/db/list.json");
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var text = req.downloadHandler.text;
                    Debug.Log(text);
                    var namelist = JsonUtility.FromJson<FriendsNameList>(text);
                    var names = namelist.list;

                    for(var i = 0; i < names.Length; i++)
                    {
                        var FriendName = names[i];
                        UnityWebRequest req_f = UnityWebRequest.Get("https://yutagamemusic.ddns.net/JSF/db/"+GetPlatformID()+"/"+ FriendName);
                        yield return req_f.SendWebRequest();
                        if (req_f.result == UnityWebRequest.Result.Success)
                        {
                            using (var writeStream = File.OpenWrite(Path.Combine(Util.GetSavedFileDirectoryPath(), "database", "friends", FriendName)))
                            {
                                byte[] data = req_f.downloadHandler.data;
                                writeStream.Write(data,0,data.Length);
                                Debug.Log("Downloaded " + FriendName);
                            }
                        }
                        else
                        {
                            Debug.LogWarning(req_f.result);
                        }
                    }
                }
                else {
                    Debug.LogWarning(req.result);
                }

            }

            // PersistentPath以下のファイルを読み込み
            {
                if(Directory.Exists(Path.Combine(Util.GetSavedFileDirectoryPath(), "database", "friends")))
                {
                    string[] files = Directory.GetFiles(Path.Combine(Util.GetSavedFileDirectoryPath(), "database", "friends"), "*", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        if (file.EndsWith(".meta")) { continue; }
                        var loading = AssetBundle.LoadFromFileAsync(file);
                        yield return new WaitUntil(() => loading.isDone);

                        var loaded = loading.assetBundle;
                        if (loaded == null)
                        {
                            Debug.LogWarning("Failed to load " + file);
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
            }
        }

        private static string GetPlatformID()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return "iOS";
                default:
                    return "Windows";
            }
        }
    }

    public struct FriendsNameList
    {
        public string[] list;
    }
}