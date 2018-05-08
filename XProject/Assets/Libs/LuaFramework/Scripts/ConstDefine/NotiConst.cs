using UnityEngine;
using System.Collections;

public class NotiConst
{
    /// <summary>
    /// Controller层消息通知
    /// </summary>
    public const string START_UP = "StartUp";                       //启动框架
    public const string DISPATCH_MESSAGE = "DispatchMessage";       //派发信息

    /// <summary>
    /// View层消息通知
    /// </summary>
    public const string UPDATE_MESSAGE = "UpdateMessage";           //更新消息
    public const string UPDATE_EXTRACT = "UpdateExtract";           //更新解包
    public const string UPDATE_EXTRACT_FAILED = "UpdateExtractFailed";           //更新解包失败
    public const string UPDATE_DOWNLOAD = "UpdateDownload";         //更新下载
    public const string UPDATE_PROGRESS = "UpdateProgress";         //更新进度
    public const string UPDATE_FAILED = "UpdateFailed";             //下载失败
    public const string UPDATE_CANCELLED = "UpdateCancelled";       //下载已取消
    public const string EXTRACT_ONE = "ExtractOne";                 //解压一个文件
    public const string DOWNLOAD_EXTRACT = "DownloadExtract";       //下载解压
    public const string DOWNLOAD_EXTRACT_FAILED = "DownloadExtractFailed";       //下载解压失败
}
