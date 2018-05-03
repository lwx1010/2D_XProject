using UnityEngine;
using System.Collections;
using Riverlake;
using System;
using System.Collections.Generic;
using System.IO;
using LuaFramework;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Config
{
    public class User_Config
    {
        public static string ip { get; set; }

        // 更新服务器地址
        public static string web_url = string.Empty;
        // cofing.txt
        private static IniFile config_file = null;
        //服务器信息地址
        public static string server_url = string.Empty;
        //默认服务器
        public static int default_server = 0;

        public static int internal_sdk = 0;

        public static string logoName = string.Empty;
        public static string sdk_server_url = string.Empty;
        public static string cdn_server_url = string.Empty;
        public static string resource_server = string.Empty;
        public static string apk_server = string.Empty;

        public static string user_account = string.Empty;
        public static string user_password = string.Empty;
        // GM 官网地址
        public static string gm_url = string.Empty;
        /// <summary>
        /// 挂机状态（不用保存）
        /// </summary>
        public static int fightStatus { get; set; }
        /// <summary>
        /// 公告显示日期
        /// </summary>
        public static string noticesTime { get; set; }

        public static List<ServerInfo> serverList;
        /// <summary>
        /// 最近服务器列表
        /// </summary>
        public static List<ServerInfo> lastServerList;
        /// <summary>
        /// 玩家信息列表
        /// </summary>
        public static List<RoleInfo> roleInfoList;

        public static int recieveSystemChannel = 1;
        public static int recieveWorldChannel = 1;
        public static int recieveNearChannel = 1;
        public static int recieveGangChannel = 1;
        public static int recieveTeamChannel = 1;
        public static int recieveStrangerChannel = 1;
        public static int autoVoiceToText = 0;
        public static int autoPlayWorldVoice = 0;
        public static int autoPlayNearVoice = 0;
        public static int autoPlayGangVoice = 0;
        public static int autoPlayTeamVoice = 0;
        public static int autoPlayPrivateChatVoice = 0; //私聊

        // 玩家显示隐藏选项
        public static int blockAllianPlayer = 0;  //友方
        public static int blockOtherPartner = 0;
        public static int blockOtherPlayer = 0;
        public static int blockOtherLingqi = 0;
        public static int blockOtherPet = 0;
        public static int blockMonster = 0;
        public static int blockOtherXiulianEffect = 0;      //隐藏其它修炼特效
        public static int blockShakeCamera = 0;  // 震屏

        public static int showSimpleSkillEff = 1;           //是否根据地图配置显示简化技能特效

        public static int blockAllTitleSpr = 0;             //屏蔽其他玩家称号

        //游戏设置
        public static float playerScreenCount = 1; //同屏显示
        public const int MaxPlayerScreen = 20;
        public static float volumn = 1; //音量
        public static int quality = 1;  // 画面品质  0-极速 1-流畅 2 - 完美
        public static int isMusic = 1;  // 音乐
        public static int isAudio = 1;  // 音效
        public static int useLuQi = 0;  // 自动释放怒气
        public static int powSaving = 0;  //省电模式
        public static int use_dadian = 1;

        public static int attactTarget = 0;  //攻击怪物-0 , 玩家- 1, Boss - 2

        private static string curUid = string.Empty;

        //机器人开关
        public static int blockTaskRobot = 0;        //是否开启新手机器人0是开启, 1是屏蔽

        #region 帧数相关
        public static int fpsInterval = 10;
        public static int fpsLow = 15;
        public static int fpsHigh = 20;
        #endregion

        #region 语音相关
        public const string VOICE_MD5_STRING = "TDZ7z5PCE50nzNFp1CTFEwwDO3dg0M0m";

        public static string soundIp = string.Empty;
        public static int soundPort = 0;
        public static string soundToken = string.Empty;

        #endregion

        public static void Initilize(string configContent)
        {
            if (config_file == null)
            {
#if UNITY_EDITOR
                config_file = new IniFile(configContent, true);
#else
                config_file = new IniFile(configContent, false);
#endif
            }
        }

        public static void LoadConfig(string configContent)
        {
            Initilize(configContent);

            // 获取SDK相关信息
            if (config_file != null && config_file.goTo("appsetting"))
            {
                internal_sdk = config_file.GetValue_Int("internal_sdk");
                logoName = config_file.GetValue_String("logo_name");
                use_dadian = config_file.GetValue_Int("use_dadian", 1);
            }
            if (config_file != null && config_file.goTo("download"))
            {
                sdk_server_url = config_file.GetValue_String("sdk_server_link");
                cdn_server_url = config_file.GetValue_String("cdn_server_link");
                resource_server = config_file.GetValue_String("resource_server");
                apk_server = config_file.GetValue_String("apk_server");
            }

            if (config_file != null && config_file.goTo("othersettings"))
            {
                blockTaskRobot = config_file.GetValue_Int("blockTaskRobot");
                fpsInterval = config_file.GetValue_Int("fpsInterval");
                fpsInterval = fpsInterval == 0 ? 10 : fpsInterval;
                fpsLow = config_file.GetValue_Int("fpsLow");
                fpsLow = fpsLow == 0 ? 15 : fpsLow;
                fpsHigh = config_file.GetValue_Int("fpsHigh");
                fpsHigh = fpsHigh == 0 ? 20 : fpsHigh;
            }

            // 如果使用内部sdk, 则直接返回, 服务器相关信息从sdk获取
            if (internal_sdk == 1) return;

            // 账号信息
            if (config_file != null && config_file.goTo("user"))
            {
                if (!string.IsNullOrEmpty(config_file.GetValue_String("user_account")))
                {
                    try
                    {
                        byte[] datas = Convert.FromBase64String(config_file.GetValue_String("user_account"));
                        user_account = Encoding.GetString(datas);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("get user account error: " + ex.Message.ToString());
                    }
                }
                if (!string.IsNullOrEmpty(config_file.GetValue_String("user_password")))
                {
                    try
                    {
                        byte[] datas = Convert.FromBase64String(config_file.GetValue_String("user_password"));
                        user_password = Encoding.GetString(datas);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("get user password error: " + ex.Message.ToString());
                    }
                }
            }

            // 网络信息
            if (config_file != null && config_file.goTo("net"))
            {
                string address = config_file.GetValue_String("server_ip");
                string port = config_file.GetValue_String("server_port");
                default_server = config_file.GetValue_Int("default_serverno");
                ip = string.Format("{0}:{1}", address, port);
                gm_url = config_file.GetValue_String("gm_url");
            }
            //服务器地址
            if (config_file != null && config_file.goTo("download"))
            {
                string url = config_file.GetValue_String("server_link");
                if (url.IndexOf("http://") == -1 && url.IndexOf("https://") == -1)
                    url = "http://" + url;

                server_url = url;
            }
        }

        public static void ResetConfig(string content)
        {
            if (config_file != null)
            {
                config_file.Clear();
                config_file.Read_From_String(content);
            }
            LoadConfig(content);
        }

        public static void SetUserInfo(string name, string password)
        {
            user_account = name;
            user_password = password;

            config_file.SetSelctor("user");

            if (config_file != null && config_file.goTo("appsetting"))
                config_file.SetInt("internal_sdk", internal_sdk);

            if (config_file != null && config_file.goTo("user"))
            {
                if (internal_sdk == 1) return;
                config_file.SetString("user_account", Convert.ToBase64String(Encoding.GetBytes(name)));
                config_file.SetString("user_password", Convert.ToBase64String(Encoding.GetBytes(password)));
            }
        }
        /// <summary>
        /// 公共设置
        /// </summary>
        public static void LoadGlobalSetting()
        {
            curUid = "gameSetting";
            if (config_file.goTo(curUid))
            {
                autoPlayGangVoice = config_file.GetValue_Int("autoPlayGangVoice");
                autoPlayTeamVoice = config_file.GetValue_Int("autoPlayTeamVoice");
                autoPlayPrivateChatVoice = config_file.GetValue_Int("autoPlayPrivateChatVoice");
                // 屏蔽设置
                blockAllianPlayer = config_file.GetValue_Int("blockAllianPlayer");
                blockOtherPartner = config_file.GetValue_Int("blockOtherPartner");
                blockOtherPlayer = config_file.GetValue_Int("blockOtherPlayer");
                blockOtherLingqi = config_file.GetValue_Int("blockOtherLingqi");
                blockOtherPet = config_file.GetValue_Int("blockOtherPet");
                blockMonster = config_file.GetValue_Int("blockMonster");
                blockShakeCamera = config_file.GetValue_Int("blockShakeCamera");
                blockAllTitleSpr = config_file.GetValue_Int("blockAllTitleSpr");

                useLuQi = config_file.GetValue_Int("useLuQi");
                powSaving = config_file.GetValue_Int("powSaving");
                //系统设置
                quality = config_file.GetValue_Int("quality");
                isMusic = config_file.GetValue_Int("isMusic");
                isAudio = config_file.GetValue_Int("isAudio");
                playerScreenCount = config_file.GetValue_Float("playerScreenCount");
                volumn = config_file.GetValue_Float("volumn");
                showSimpleSkillEff = config_file.GetValue_Int("showSimpleSkillEff");
                attactTarget = config_file.GetValue_Int("attactTarget");

                noticesTime = config_file.GetValue_String("noticesTime");
            }
            else
            {
                config_file.SetSelctor(curUid);

                config_file.SetInt("autoPlayGangVoice", 0);
                config_file.SetInt("autoPlayTeamVoice", 0);
                config_file.SetInt("autoPlayPrivateChatVoice", 0);

                config_file.SetInt("showJingjiWings", 1);
                config_file.SetInt("blockAllianPlayer", 0);
                config_file.SetInt("blockOtherPartner", 1);
                config_file.SetInt("blockOtherPlayer", 0);
                config_file.SetInt("blockOtherLingqi", 1);
                config_file.SetInt("blockOtherPet", 1);
                config_file.SetInt("blockMonster", 0);
                config_file.SetInt("blockShakeCamera" , 1);
                config_file.SetInt("blockAllTitleSpr", 0);

                config_file.SetInt("quality", 1);
                config_file.SetInt("isMusic", 1);
                config_file.SetInt("isAudio", 1);
                config_file.SetFloat("playerScreenCount", 0.5f);
                config_file.SetFloat("volumn", 1);
                config_file.SetInt("useLuQi", 0);
                config_file.SetInt("powSaving", 0);
                config_file.SetInt("showSimpleSkillEff" , 0);
                config_file.SetInt("attactTarget", 0);

                config_file.SetString("noticesTime", "0");
                Save();
            }
        }


        public static void Save()
        {
            try
            {
                if (config_file != null)
                {
                    if (AppConst.DebugMode)
                        File.WriteAllText(string.Format("{0}Resources/config.txt", Util.DataPath), config_file.ToString());
                    else
                        File.WriteAllText(string.Format("{0}config.txt", Util.DataPath), config_file.ToString());
#if UNITY_EDITOR
                    AssetDatabase.Refresh();
#endif
                }
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Save config exception: {0}\nStacktrace:{1}", e.Message, e.StackTrace));
                if (e.Message.ToLower().Contains("disk full"))
                {
                    MessageBox.Show(LanguageTips.DISK_FULL);
                }
            }
        }

        public static void SaveInEditorModel()
        {
            if (config_file != null)
            {
                File.WriteAllText("Assets/Resources/config.txt", config_file.ToString());
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
        }

        public static void SetWebServerUrl(string url)
        {
            web_url = Path.Combine(url, LuaConst.osDir).Replace("\\", "/");
            if (config_file == null) return;

            if (config_file.goTo("download"))
            {
                config_file.SetString("web_server", url);
            }
            if (config_file.goTo("net"))
            {
                default_server = config_file.GetValue_Int("default_serverno");
            }
        }


        public static void SetLoginServerInfo(ServerInfo serverInfo)
        {
            if (config_file != null && config_file.goTo("net"))
            {
                if (internal_sdk != 1)
                {
                    config_file.SetString("server_ip", serverInfo.serverIp);
                    config_file.SetString("server_port", serverInfo.serverPort);
                    ip = string.Format("{0}:{1}", serverInfo.serverIp, serverInfo.serverPort);
                }

                default_server = serverInfo.serverNo;
                config_file.SetString("default_serverno", serverInfo.serverNo.ToString());
            }
        }

        /// <summary>
        /// 设置默认服务器
        /// </summary>
        /// <param name="serverNo"></param>
        public static void SetDefaultServer(int serverNo)
        {
            if (default_server > 0)
                return;

            default_server = serverNo;
        }

        public static void LoadCharactorConfig(string uid)
        {
            curUid = uid;
            if (config_file.goTo(uid))
            {
                //聊天设置
                recieveSystemChannel = config_file.GetValue_Int("recieveSystemChannel");
                recieveWorldChannel = config_file.GetValue_Int("recieveWorldChannel");
                recieveNearChannel = config_file.GetValue_Int("recieveNearChannel");
                recieveGangChannel = config_file.GetValue_Int("recieveGangChannel");
                recieveTeamChannel = config_file.GetValue_Int("recieveTeamChannel");
                recieveStrangerChannel = config_file.GetValue_Int("recieveStrangerChannel");
                autoVoiceToText = config_file.GetValue_Int("autoVoiceToText");
                autoPlayWorldVoice = config_file.GetValue_Int("autoPlayWorldVoice");
                autoPlayNearVoice = config_file.GetValue_Int("autoPlayNearVoice");
                autoPlayGangVoice = config_file.GetValue_Int("autoPlayGangVoice");
                autoPlayTeamVoice = config_file.GetValue_Int("autoPlayTeamVoice");
            }
            else
            {
                config_file.SetSelctor(uid);
                config_file.SetInt("recieveSystemChannel", 1);
                config_file.SetInt("recieveWorldChannel", 1);
                config_file.SetInt("recieveNearChannel", 1);
                config_file.SetInt("recieveGangChannel", 1);
                config_file.SetInt("recieveTeamChannel", 1);
                config_file.SetInt("recieveStrangerChannel", 1);
                config_file.SetInt("autoVoiceToText", 0);
                config_file.SetInt("autoPlayWorldVoice", 0);
                config_file.SetInt("autoPlayNearVoice", 0);
                config_file.SetInt("autoPlayGangVoice", 0);
                config_file.SetInt("autoPlayTeamVoice", 0);
                Save();
            }
        }

        #region ---------------基础GET/SET--------------------
        public static bool GetBoolean(string key)
        {
            return GetBoolean(curUid , key);
        }

        public static bool GetBoolean(string group , string key)
        {
            if (config_file.goTo(group))
            {
                return config_file.GetValue_Int(key) == 1;
            }
            return false;
        }

        public static int GetInt(string key)
        {
            return GetInt(curUid , key);
        }

        public static int GetInt(string group , string key)
        {
            if (config_file.goTo(group))
            {
                return config_file.GetValue_Int(key);
            }
            return 0;
        }

        public static string GetString(string key)
        {
            return GetString(curUid , key);
        }

        public static string GetString(string group , string key)
        {
            if (config_file.goTo(group))
            {
                return config_file.GetValue_String(key);
            }
            return "";
        }

        public static float GetFloat(string key)
        {
            return GetFloat(curUid , key);
        }

        public static float GetFloat(string group , string key)
        {
            if (config_file.goTo(group))
            {
                return config_file.GetValue_Float(key);
            }
            return 0;
        }

        /// <summary>
        /// 设置聊天设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetBoolean(string key, bool value)
        {
            SetBoolean(curUid , key , value);
        }

        public static void SetBoolean(string group , string key, bool value)
        {
            config_file.SetSelctor(group);
            config_file.SetInt(key, value ? 1 : 0);
        }

        public static void SetInt(string key, int value)
        {
            SetInt(curUid , key , value);
        }

        public static void SetInt(string group , string key, int value)
        {
            config_file.SetSelctor(group);
            config_file.SetInt(key, value);
        }

        public static void SetFloat(string key, float value)
        {
            SetFloat(curUid , key , value);
        }

        public static void SetFloat(string group , string key, float value)
        {
            config_file.SetSelctor(group);
            config_file.SetFloat(key, value);
        }

        public static void SetString(string key, string value)
        {
            SetString(curUid , key , value);
        }
        public static void SetString(string group , string key, string value)
        {
            config_file.SetSelctor(group);
            config_file.SetString(key, value);
        }

        #endregion


        /// <summary>
        /// 设置服务器列表
        /// </summary>
        /// <param name="list"></param>
        public static void SetServerList(ArrayList list)
        {
            if (serverList == null)
                serverList = new List<ServerInfo>();
            serverList.AddRange(GetServerList(list));
            serverList.Sort((x , y)=>x.serverNo.CompareTo(y.serverNo) * -1);
        }

        /// <summary>
        /// 设置最近登录的服务器列表
        /// </summary>
        /// <param name="list"></param>
        public static void SetLastServerList(ArrayList list)
        {
            if (lastServerList == null)
                lastServerList = new List<ServerInfo>();
            lastServerList.AddRange(GetServerList(list));
            //lastServerList.Sort((x, y) => x.serverNo.CompareTo(y.serverNo) * -1);
        }

        /// <summary>
        /// 设置玩家信息列表
        /// </summary>
        /// <param name="list"></param>
        public static void SetRoleInfoList(ArrayList list)
        {
            if (roleInfoList == null)
                roleInfoList = new List<RoleInfo>();
            roleInfoList.AddRange(GetRoleInfoList(list));
        }
        /// <summary>
        /// 设置服务器列表等级
        /// </summary>
        /// <param name="list"></param>
        public static void SetServerListLevel(List<ServerInfo> list)
        {
            int count = roleInfoList.Count;
            int count2 = list.Count;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count2; j++)
                {
                    if (roleInfoList[i].sid == list[j].serverNo)
                    {
                        list[j].playerLevel = roleInfoList[i].roleLevel;
                        break;
                    }
                }
            }
        }
        private static List<ServerInfo> GetServerList(ArrayList list)
        {
            List<ServerInfo> sl = new List<ServerInfo>();
            for (int i = 0; i < list.Count; i++)
            {
                Hashtable map = list[i] as Hashtable;
                ServerInfo info = new ServerInfo();
                if (map.ContainsKey("sid"))
                {
                    info.serverNo = int.Parse(map["sid"].ToString());
                }
                if (map.ContainsKey("name"))
                {
                    info.serverName = map["name"].ToString();
                }
                if (map.ContainsKey("addr"))
                {
                    info.serverIp = map["addr"].ToString();
                }
                if (map.ContainsKey("port"))
                {
                    info.serverPort = map["port"].ToString();
                }
                if (map.ContainsKey("status"))
                {
                    info.status = int.Parse(map["status"].ToString());
                }
                if (map.ContainsKey("isNew"))
                {
                    info.isNew = int.Parse(map["isNew"].ToString());
                }
                if (map.ContainsKey("openTime"))
                {
                    info.openTime = int.Parse(map["openTime"].ToString());
                }
                if (map.ContainsKey("playerLevel"))
                {
                    info.playerLevel = int.Parse(map["roleLv"].ToString());
                }
                if (map.ContainsKey("tips"))
                {
                    info.tips = map["tips"].ToString();
                }
                sl.Add(info);
            }
            return sl;
        }

        private static List<RoleInfo> GetRoleInfoList(ArrayList list)
        {
            List<RoleInfo> ri = new List<RoleInfo>();
            for (int i = 0; i < list.Count; i++)
            {
                Hashtable map = list[i] as Hashtable;
                RoleInfo info = new RoleInfo();
                if (map.ContainsKey("id"))
                {
                    info.id = int.Parse(map["id"].ToString());
                }
                if (map.ContainsKey("sid"))
                {
                    info.sid = int.Parse(map["sid"].ToString());
                }
                if (map.ContainsKey("pid"))
                {
                    info.pid = int.Parse(map["pid"].ToString());
                }
                if (map.ContainsKey("accountName"))
                {
                    info.accountName = map["accountName"].ToString();
                }
                if (map.ContainsKey("roleName"))
                {
                    info.roleName = map["roleName"].ToString();
                }
                if (map.ContainsKey("roleLv"))
                {
                    info.roleLevel = int.Parse(map["roleLv"].ToString());
                }
                if (map.ContainsKey("vipLv"))
                {
                    info.vipLevel = int.Parse(map["vipLv"].ToString());
                }
                ri.Add(info);
            }
            return ri;
        }

    }
}


/// <summary>
/// 服务器信息
/// </summary>
public class ServerInfo
{
    public int serverNo;
    public string serverName;
    public string serverIp;
    public string serverPort;
    public int status;
    public int isNew;
    public int openTime;
    /// <summary>
    /// 玩家等级（用于最近服务器列表）
    /// </summary>
    public int playerLevel;

    //保留字段（兼容性）
    public int isOpen;
    public string other;

    //对应serverid10
    public string hostId;
    public string corpId;
    public string tips;
}

/// <summary>
/// 玩家信息
/// </summary>
public class RoleInfo
{
    public int id;
    public int sid;
    public int pid;
    public string accountName;
    public string roleName;
    public int roleLevel;
    public int vipLevel;
}