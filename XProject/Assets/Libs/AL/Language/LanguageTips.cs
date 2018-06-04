using UnityEngine;
using System.Collections;

public sealed class LanguageTips
{
    public const string LOAD_SDK_NAME_ERROR = "加载SDK名称出错！";

    public const string NETWORK_NOT_REACHABLE = "网络无法连接， 请检查网络是否可用。";

    public const string GET_SERVERLIST_ERROR = "获取服务器列表失败！";

    public const string NETWORK_VIA_CARRIERDATA = "检测到当前正在使用流量数据， 可能会产生额外费用， 点击是继续游戏，否退出游戏。";

    public const string CLICK_SCREEN_UPDATE = "点击屏幕开始更新";

    public const string NO_WIFI_DOWNLOADING = "您当前处于非WiFi环境下， 继续下载将产生手机流量， 确定继续？";

    public const string NO_CONFIG_VIA_EDITORMODE = "Resources目录下不存在config.txt文件， 请复制Asset/config.txt.example文件到Resources目录下并去掉.example后缀名";

    public const string NO_CONFIG_VIA_MOBLIEPLATFORM = "游戏配置文件丢失，无法进入游戏， 请重新下载";

    public const string CONNECTINT_TO_SERVER = "正在连接服务器";

    public const string CHECK_UPDATE = "正在为您检查更新";

#if UNITY_IOS
    public const string CHECK_UPDATE_TIMEOUT = "网络请求失败";

    public const string CHECK_UPDATE_FAILED = "网络请求失败";
#else
    public const string CHECK_UPDATE_TIMEOUT = "连接更新服务器超时";

    public const string CHECK_UPDATE_FAILED = "检测更新失败";
#endif

    public const string UPDATE_DATA_SIZE = "本次更新资源大小";

    public const string UPDATE_FAILED = "更新失败";

    public const string UPDATE_CANCELLED = "更新异常";

    public const string WRITE_APK_ERROR = "文件写入错误，请确保手机有权限且有足够多的空间来下载安装包（预留1G以上）";

    public const string UPDATE_FINISHED = "更新完成";

    public const string UPDATE_MD5_ERROR = "资源更新错误，请重新启动游戏";

    public const string EXTRACT_FILE_PROGRESS = "正在为您努力解压资源...";

    public const string DOWNLOAD_FILE_ING = "正在更新中， 请不要关闭游戏。";

    public const string AUTO_ADJUST_FILES = "正在校验文件: ";

    public const string LOADING_GAME_ING = "正在加载中";

    public const string LOADING_LUA_ING = "正在读取游戏数据";

    public const string LOADING_GAME_FINISHED = "加载完毕";

    public const string EXTRACT_PACKAGE = "正在为您努力释放资源";

    public const string EXTRACT_ERROR = "解压错误";

    public const string EXTRACT_FINISHED = "安装完成";

    public const string READ_FILES = "正在读取数据";

    public const string READ_FILES_MD5_ERROR = "读取数据MD5不匹配，正在重新读取";

    public const string READ_FILES_ERROR_QUIT = "读取数据文件错误，请重新下载安装包或联系客服人员，点击确定退出游戏。";

    public const string CONNECT_GAMESERVER = "正在连接游戏服务器";

    public const string CONNECT_GAMESERVER_SUCCESS = "连接游戏服务器成功";

    public const string LOGIN_CENTERSERVER_SUCCESS = "连接登陆服务器成功";

    public const string LOGIN_CENTERSERVER_FAILED = "连接登陆服务器失败";

    public const string CONNECT_SERVER_ERROR_QUITGAME = "连接服务器超时，请退出游戏后重新登录！";

    public const string SERVER_RESPONSE_FAILED = "请求服务器失败";

    public const string HTTP_ERROR = "无法连接网络服务器，请检查网络连接或重启游戏后再试";

    public const string CHAT_PERMISSION_FAILED = "您还未开启录音权限，请在手机设置中进行开通。";

    public const string POWER_SAVING_MODE = "您现在处于省电模式\n可点击任意地方返回";

    public const string SECOND = "秒";

    public const string BACKGROUND_DOWNLOADING_ERROR = "资源扩展包下载错误";

    public const string DISK_FULL = "手机存储空间已满";

    public const string DOWNLOADING_APK = "正在更新游戏版本";

    public const string DOWNLOAD_APK_INSTALL = "准备安装Apk包";

#region 中央服返回错误码
    public const string ERROR_CODE_0 = "接口成功响应";

    public const string ERROR_CODE_1 = "接口请求参数出错";

    public const string ERROR_CODE_2 = "sign参数出错";

    public const string ERROR_CODE_3 = "渠道不存在";

    public const string ERROR_CODE_4 = "请求SDK出错";

    public const string ERROR_CODE_5 = "不存在此api方法";

    public const string ERROR_CODE_6 = "请求SDK出错参数出错";

    public const string ERROR_CODE_7 = "请求的激活码不存在";

    public const string ERROR_CODE_8 = "请求的激活码类型信息不存在";

    public const string ERROR_CODE_9 = "请求的激活码不在使用期内";

    public const string ERROR_CODE_10 = "激活码不存在";

    public const string ERROR_CODE_11 = "激活码已被使用";

    public const string ERROR_CODE_12 = "帐号未激活";

    public const string ERROR_CODE_13 = "http方法不正确";

    public const string ERROR_CODE_14 = "请求的礼包码已经被使用";

    public const string ERROR_CODE_15 = "活动版本号没更新";

    public const string ERROR_CODE_16 = "此平台不参与此次测试";

    public const string ERROR_CODE_17 = "此账号被封禁";

    public const string ERROR_CODE_18 = "您已领取过同类型的礼包码";

    public const string ERROR_CODE_19 = "玩家手机设备被封禁";

    public const string ERROR_CODE_20 = "手机绑定验证码不对";
#endregion
}
