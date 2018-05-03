using System;
using System.Collections.Generic;
using System.IO;
using Frankfort.Threading;
using Riverlake.Crypto;

/// <summary>
/// 启动时的整包资源，本地释放
/// </summary>
public class LocalExtractExecutor : IThreadWorkerObject
{
    public string LocalPath;

    public string FileName;

    public byte[] FileBytes;

    public Action<NotiData> OnComplete;
     
    public string FileMD5 { get; private set; }

    public string SrcMD5;

    /// <summary>
    /// 压缩包内文件的MD5集合
    /// </summary>
    public Dictionary<string,string> SubFileMD5Dic { get; private set; }
    public void ExecuteThreadedWork()
    {
        SubFileMD5Dic = new Dictionary<string, string>();
        try
        {
            FileMD5 = MD5.ComputeHashString(FileBytes);

            using (var decompressor = new ZstdNet.Decompressor())
            {
#if UNITY_IOS
                FileBytes = Crypto.Decode(FileBytes);
#endif
                using (var ms = new MemoryStream(decompressor.Unwrap(FileBytes)))
                {
                    using (var reader = new BinaryReader(ms))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            string abName = reader.ReadString();
                            int length = reader.ReadInt32();
                            var path = LocalPath + abName;
                            string dir = Path.GetDirectoryName(path);
                            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                            var bytes = reader.ReadBytes(length);
                            if (File.Exists(path)) File.Delete(path);
                            File.WriteAllBytes(path, bytes);

                            SubFileMD5Dic[abName] =  MD5.ComputeHashString(bytes);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            var data = new NotiData(NotiConst.UPDATE_EXTRACT_FAILED, FileName, e.Message);
            if (OnComplete != null) OnComplete(data);  //回调逻辑层
        }

        FileBytes = null;
    }



    public void AbortThreadedWork()
    {

    }
}
