using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using JSF.Common;
using System;
using System.Linq;
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

        public static string LoadingStatus { get; private set; } = "�t�����Y�f�[�^�x�[�X���X�V���Ă��܂��c";

        public static readonly string LOCAL_DATA_PATH = Path.Combine(Util.GetSavedFileDirectoryPath(), "database");
        public static readonly string LOCAL_FRIENDS_DATA_PATH = Path.Combine(Util.GetSavedFileDirectoryPath(), "database", "friends");
        public static readonly string LOCAL_JSON_PATH = Path.Combine(Util.GetSavedFileDirectoryPath(), "database", "local.json");
        public static readonly string DATABASE_JSON_URL = "https://yutagamemusic.ddns.net/JSF/db/list.json";
        public static readonly string DATABASE_URL = "https://yutagamemusic.ddns.net/JSF/db/";

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
            // (�G�f�B�^��p)DB����ǂݍ���
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
                _static_db.friends.Sort((a, b) => a.FriendID - b.FriendID);
                return _static_db;
            }
#endif

            // ���[�h���I����Ă��Ȃ��ꍇ�ً̋}�[�u
            GameObject loaderObj = new GameObject();
            loaderObj.name = "loader";
            var loader = loaderObj.AddComponent<Loader>();
            loader.AutoMoveToTitle = false;
            return _static_db;
        }

        public static IEnumerator Load()
        {
            LoadingStatus = "�t�����Y�f�[�^�x�[�X���X�V���Ă��܂��c";
            _static_db = new FriendsDatabase();
#if UNITY_EDITOR
            if (Application.platform == RuntimePlatform.WindowsEditor && Directory.Exists("Assets/ServerUtil/Database/Friends"))
            {
                string[] files = Directory.GetFiles("Assets/ServerUtil/Database/Friends/", "Friend.asset", SearchOption.AllDirectories);
                LoadingStatus = $"�f�o�b�O�p�̃f�[�^�x�[�X����ǂݍ���ł��܂��c(0/{files.Length})";
                for (var i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    Debug.Log(file);
                    Friend f = AssetDatabase.LoadAssetAtPath<Friend>(file);
                    if (f != null)
                    {
                        _static_db.friends.Add(f);
                        Debug.Log("Loaded " + f.Name);
                        LoadingStatus = $"�f�o�b�O�p�̃f�[�^�x�[�X����ǂݍ���ł��܂��c({i+1}/{files.Length})";
                    }
                    else
                    {
                        Debug.LogWarning("not a friend!");
                    }
                }
                _static_db.friends.Sort((a, b) => a.FriendID - b.FriendID);
                LoadingStatus = "�ǂݍ��݂��������܂���";
                yield break;
            }
#endif
            // ���[�J���f�[�^�p�̃t�H���_�쐬
            if (!Directory.Exists(LOCAL_DATA_PATH))
            {
                Directory.CreateDirectory(LOCAL_DATA_PATH);
            }
            // StreamingAssets�̃f�[�^��PersistentPath�ȉ��ɏ�������
            {
                if(!Directory.Exists(LOCAL_FRIENDS_DATA_PATH)){
                    Directory.CreateDirectory(LOCAL_FRIENDS_DATA_PATH);
                }
                LoadingStatus = "�f�t�H���g�f�[�^�������o���Ă��܂��c";
                try
                {
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
                }catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }
            // �T�[�o�[�ɃA�N�Z�X���t�����Y�ꗗ��ǂݍ���
            {
                LoadingStatus = "�f�[�^�x�[�X�����T�[�o�[����擾���Ă��܂��c";
                UnityWebRequest req = UnityWebRequest.Get(DATABASE_JSON_URL);
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var text = req.downloadHandler.text;
                    Debug.Log(text);
                    var namelist = JsonUtility.FromJson<NameList.FriendsNameList>(text);
                    var downloaded_names = namelist.list;
                    var names = namelist.list.ToList();

                    // ���[�J���ɕۑ����Ă���json�f�[�^�Ɣ�r
                    if(File.Exists(LOCAL_JSON_PATH))
                    {
                        using(var local_file = File.OpenRead(LOCAL_JSON_PATH))
                        {
                            var stream = new StreamReader(local_file);
                            try
                            {
                                var local_db = JsonUtility.FromJson<NameList.FriendsNameList>(stream.ReadToEnd());
                                var local_list = local_db.list;
                                foreach(var local_friend in local_list)
                                {
                                    var _comp = names.Where((f) => f.name == local_friend.name);
                                    if (_comp.Count() == 0)
                                    {
                                        // �Y���t�����Y���T�[�o�[�������������Ă��遨���[�J���ł��폜
                                        Debug.Log("[DB change]<Deleted>"+local_friend.name);
                                    }
                                    else
                                    {
                                        var comp = _comp.First();
                                        if (local_friend.last_update != comp.last_update)
                                        {
                                            // �f�[�^���X�V����Ă��遨�X�V���X�g�Ɏc�����܂�
                                            Debug.Log("[DB change]<Updated>" + local_friend.name);
                                        }
                                        else
                                        {
                                            // �f�[�^���ω����Ă��Ȃ����X�V���X�g�������
                                            names.Remove(comp);
                                        }
                                    }
                                }

                            }catch(System.Exception e)
                            {
                                Debug.LogException(e);
                            }
                            
                        }
                    }

                    bool successful = true;
                    for (var i = 0; i < names.Count(); i++)
                    {
                        LoadingStatus = $"�V�K�f�[�^���_�E�����[�h���Ă��܂��c({i+1}/{names.Count()})";
                        var FriendName = names[i];
                        UnityWebRequest req_f = UnityWebRequest.Get(DATABASE_URL + GetPlatformID()+"/"+ FriendName.name);
                        yield return req_f.SendWebRequest();
                        if (req_f.result == UnityWebRequest.Result.Success)
                        {
                            using (var writeStream = File.OpenWrite(Path.Combine(Util.GetSavedFileDirectoryPath(), "database", "friends", FriendName.name)))
                            {
                                byte[] data = req_f.downloadHandler.data;
                                writeStream.Write(data,0,data.Length);
                                Debug.Log("Downloaded " + FriendName);
                            }
                        }
                        else
                        {
                            Debug.LogWarning(req_f.result);
                            successful = false;
                        }
                    }

                    if (successful)
                    {
                        // ���[�J����json�t�@�C�����㏑��
                        using (var fwp = File.Open(LOCAL_JSON_PATH,FileMode.Create))
                        {
                            using (var writer = new StreamWriter(fwp))
                            {
                                var writing_data = new NameList.FriendsNameList()
                                {
                                    list = downloaded_names,
                                };
                                writer.WriteLine(JsonUtility.ToJson(writing_data));
                            }
                        }
                    }
                }
                else
                {
                    LoadingStatus = $"�f�[�^�x�[�X�ǂݍ��ݎ��ɃG���[���������܂����F{req.result}";
                    Debug.LogWarning(req.result);
                    yield return new WaitForSeconds(1f);
                }

            }

            // PersistentPath�ȉ��̃t�@�C����ǂݍ���
            {
                if(Directory.Exists(Path.Combine(Util.GetSavedFileDirectoryPath(), "database", "friends")))
                {
                    string[] files = Directory.GetFiles(Path.Combine(Util.GetSavedFileDirectoryPath(), "database", "friends"), "*", SearchOption.TopDirectoryOnly);
                    for (var i = 0; i < files.Length; i++)
                    {
                        var file = files[i];
                        LoadingStatus = $"�t�@�C����ǂݍ���ł��܂��c({i+1}/{files.Length})";
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
            _static_db.friends.Sort((a, b) => a.FriendID - b.FriendID);
            LoadingStatus = "�ǂݍ��݂��������܂���";
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

    namespace NameList
    {
        public struct FriendsNameList
        {
            public Friend[] list;
        }

        [Serializable]
        public struct Friend
        {
            public string name;
            public long last_update;
        }
    }
}