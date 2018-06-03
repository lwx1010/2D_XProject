using UnityEngine;
using System.Collections;
using System.IO;
using System;
using AL;
using System.Collections.Generic;

public class FileManager
{
    /// <summary>
    /// 文件是否存在
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    static public bool FileExist(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// 目录是否存在
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    static public bool DirectoryExist(string dir)
    {
        return Directory.Exists(dir);
    }

    static public void BinaryCopyFile(string sourceFile, string destFile)
    {
        try
        {
            byte[] content = File.ReadAllBytes(sourceFile);
            File.WriteAllBytes(destFile, content);
#if DEVELOPER || UNITY_EDITOR
            Debug.Log("Copy file: " + sourceFile + "to" + destFile);
#endif
        }
        catch (Exception e)
        {
            Debug.Log("Copy file error: " + e.ToString());
        }
    }

    static public void StringCopyFile(string sourceFile, string destFile)
    {
        try
        {
            string content = File.ReadAllText(sourceFile);
            File.WriteAllText(destFile, content);
#if DEVELOPER || UNITY_EDITOR
            Debug.Log("Copy file: " + sourceFile + "to" + destFile);
#endif
        }
        catch (Exception e)
        {
            Debug.Log("Copy file error: " + e.ToString());
        }
    }

    /// <summary>
    /// 写入文件(字符串)
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    static public void WriteFile(string filePath, string content)
    {
        try
        {
            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.Write(content);
            }
#if DEVELOPER || UNITY_EDITOR
            Debug.Log("Write file to " + filePath);
#endif
        }
        catch (System.Exception ex)
        {
            Debug.Log("Write file error: " + ex.ToString());
        }
    }

    /// <summary>
    /// 写入文件(字节流)
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="bytes"></param>
    static public void WriteFile(string filePath, byte[] bytes)
    {
        try
        {
            File.WriteAllBytes(filePath, bytes);
            Debug.Log("Write file to " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.Log("Write file error: " + ex.ToString());	
        }
    }

    /// <summary>
    /// 读入文件(字节流)
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>读入文件的字节流</returns>
    static public byte[] ReadFileByte(string filePath)
    {
        FileStream fs = new FileStream(filePath, FileMode.Open);
        byte[] content = new byte[fs.Length];
        fs.Read(content, 0, content.Length);
        fs.Dispose();
        return content;
    }

    static public string ReadFileString(string filePath)
    {
        return File.ReadAllText(filePath);
    }

    /// <summary>
    /// 清空指定目录
    /// </summary>
    /// <param name="path">目录</param>
    /// <param name="recursive">是否清空子文件夹,但不会删除文件夹</param>
    static public void ClearPath(string path, List<string> savePattern, bool recursive = true)
    {
        DirectoryInfo pathInfo = new DirectoryInfo(path);
        if (recursive)
        {
            foreach (DirectoryInfo dirInfo in pathInfo.GetDirectories())
            {
                if (DirectoryExist(dirInfo.FullName))
                {
                    ClearPath(dirInfo.FullName, savePattern, recursive);
                    Directory.Delete(dirInfo.FullName);
                }
            }
        }
        foreach (FileInfo fileInfo in pathInfo.GetFiles())
        {
            if (fileInfo.Name.Contains("."))
            {
                string extend = fileInfo.Name.Split('.')[1];
                if (savePattern != null && savePattern.Contains(extend))
                    continue;
            }
            File.Delete(fileInfo.FullName);
        }
        Debug.Log("Clear path: " + path);
    }

    /// <summary>
    /// 删除指定文件夹(递归)
    /// </summary>
    /// <param name="dir"></param>
    public static void DeleteFolder(string dir)
    {
        foreach (string d in Directory.GetFileSystemEntries(dir))
        {
            if (File.Exists(d))
            {
                FileInfo fi = new FileInfo(d);
                if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                    fi.Attributes = FileAttributes.Normal;
                File.Delete(d);
            }
            else
            {
                DirectoryInfo d1 = new DirectoryInfo(d);
                if (d1.GetFiles().Length != 0)
                {
                    DeleteFolder(d1.FullName);////递归删除子文件夹
                }
                Directory.Delete(d);
            }
        }
    }

    /// <summary>
    /// 拷贝指定文件夹
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="destinationPath"></param>
    public static void CopyDirectory(string sourcePath, string destinationPath)
    {
        DirectoryInfo info = new DirectoryInfo(sourcePath);
        Directory.CreateDirectory(destinationPath);
        foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
        {
            string destName = Path.Combine(destinationPath, fsi.Name);
            if (fsi is System.IO.FileInfo)
                File.Copy(fsi.FullName, destName);
            else
            {
                Directory.CreateDirectory(destName);
                CopyDirectory(fsi.FullName, destName);
            }
        }
    }
}
