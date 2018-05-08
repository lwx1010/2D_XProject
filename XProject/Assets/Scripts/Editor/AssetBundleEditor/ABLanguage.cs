using UnityEngine;
using System.Collections;

public class ABLanguage
{
#if UNITY_IOS
    public const string SET_RESOURCE_FOLDER = "Set Res Folder";

    public const string LOAD_CONFIG_INFO = "Load";

    public const string REFRESH_ALL = "Refresh all";

    public const string RES_VERSION = "资源版本号";

    public const string APK_VERSION = "APK版本号";

    public const string GAME_NAME = "GameName: ";

    public const string CHN_NAME = "CNName: ";

    public const string CHANNEL_NAME = "CHNName: ";

    public const string SDK_NAME = "SDK: ";

    public const string SAVE_CONFIG_INFO = "Save";

    public const string CLEAR_ALL_ABNAME = "Clear abName";

    public const string PACKRES_SUBPACK = "Pack with subpack";

    public const string SUBPACK_RES_ONLY = "Subpack only";

    public const string ALLPACK_RES_ONLY = "Fullpack only";

    public const string SUBPACK_NOW = "Subpack APK";

    public const string RELEASE_INSIDE = "Release APK(inside)";

    public const string RELEASE_OUTSIDE = "Release APK(outside)";

    public const string RELEASE_UPDATE = "Release APK(update)";

    public const string RELEASE_RES_UPDATE = "Release Res(update)";

    public const string PACK_IOS_RESOURCES = "Pack IOS Resources";

    public const string RELEASE_IOS_XCODE_PORJ = "Release IOS Project";

    public const string PACK_SAVE_FOLDER = "Save pack";

    public const string ALLPACK = "Full pack";

    public const string SUBPACK = "Sub pack";

    public const string PRELOAD = "Preload";

    public const string NONE = "None";

    public const string DISPLAY_FILES_ONLY = "Files only";

    public const string DISPLAY_ALL = "DisplayAll";

    public const string FULLPACK_RES = "FullRes";

    public const string SUBPACK_RES = "SubRes";

    public const string UNDOWNLOAD = "NotDownload";

    public const string PATCH_RES = "PatchRes";

    public const string PREFAB_FILE = "Prefab";

    public const string PICTURE_FILE = "Picture";

    public const string SCENE_FILE = "Scene";

    public const string MATIRIAL_FILE = "Material";

    public const string SOUND_FILE = "Sound";

    public const string Bytes_File = "Bytes";

    public const string AB_NAME = "abName: ";

    public const string PACK_ORDER = "PackOrder: ";

    public const string FILE_FILTER = "Filter: ";

    public const string DOWNLOAD_ORDER = "DownloadOrder: ";

    public const string PACK_CONFIG = "Pack Config: ";

    public const string EDIT_CONFIG = "Config Edit";
#else
    public const string SET_RESOURCE_FOLDER = "设置资源目录";

    public const string LOAD_CONFIG_INFO = "加载配置信息";

    public const string REFRESH_ALL = "一键刷新";

    public const string RES_VERSION = "资源版本号";

    public const string APK_VERSION = "APK版本号";

    public const string GAME_NAME = "游戏名: ";

    public const string CHN_NAME = "中文名: ";

    public const string CHANNEL_NAME = "渠道名: ";

    public const string SDK_NAME = "SDK: ";

    public const string SAVE_CONFIG_INFO = "保存配置信息";

    public const string CLEAR_ALL_ABNAME = "清除所有AB名";

    public const string PACKRES_SUBPACK = "打包资源+分包处理";

    public const string SUBPACK_RES_ONLY = "分包资源处理";

    public const string ALLPACK_RES_ONLY = "整包资源处理";

    public const string SUBPACK_NOW = "打包分包APK";

    public const string RELEASE_PACKAGE = "一键打包";

    public const string RELEASE_PACKAGE_INSIDE = "一键打包内网";

    public const string RELEASE_RES_UPDATE = "一键发布资源更新";

    public const string RELEASE_APK_UPDATE = "一键发布强更包";

    public const string RELEASE_IOS_XCODE_PORJ = "一键发布IOS工程";

    public const string PACK_SAVE_FOLDER = "打包保存目录";

    public const string ALLPACK = "整体打包";

    public const string SUBPACK = "分开打包";

    public const string PRELOAD = "预加载";

    public const string NONE = "无";

    public const string DISPLAY_FILES_ONLY = "只显示文件";

    public const string DISPLAY_ALL = "显示所有";

    public const string FULLPACK_RES = "整包资源(包体)";

    public const string SUBPACK_RES = "整包资源(非包体)";

    public const string UNDOWNLOAD = "不需要下载";

    public const string PATCH_RES = "补丁资源";

    public const string PREFAB_FILE = "预设";

    public const string PICTURE_FILE = "图片";

    public const string SCENE_FILE = "场景";

    public const string MATIRIAL_FILE = "材质";

    public const string SOUND_FILE = "音效";

    public const string Bytes_File = "二进制";

    public const string AB_NAME = "ab名: ";

    public const string PACK_ORDER = "打包顺序: ";

    public const string FILE_FILTER = "文件过滤类型: ";

    public const string DOWNLOAD_ORDER = "下载顺序: ";

    public const string PACK_CONFIG = "启用打包配置: ";

    public const string EDIT_CONFIG = "编辑配置";
#endif
}
