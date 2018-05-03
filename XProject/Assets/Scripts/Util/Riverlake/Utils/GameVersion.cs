using UnityEngine;
using System.Collections;
using System;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// 游戏版本号类
/// </summary>
public class GameVersion : IComparable, ICloneable, IComparable<GameVersion>, IEquatable<GameVersion>
{
    /// <summary>
    /// 通过一个version字符串创建GameVersion实例
    /// </summary>
    /// <param name="version">版本号字符串</param>
    /// <returns></returns>
    public static GameVersion CreateVersion(string version)
    {
        if (Regex.IsMatch(version, "^\\d+\\.\\d+\\.\\d+$"))
            return new GameVersion(version);
        else if (Regex.IsMatch(version, "^\\d+\\.\\d+\\$"))
            return new GameVersion(string.Format("{0}.0", version));
        else
            return new GameVersion("1.0.0");
    }

    /// <summary>
    /// 通过版本文件创建GameVersion实例
    /// </summary>
    /// <param name="fileName">版本文件名</param>
    /// <param name="defaultVersion">默认版本号</param>
    /// <returns></returns>
    public static GameVersion CreateVersion(string fileName, string defaultVersion)
    {
        if (File.Exists(fileName))
        {
            var content = File.ReadAllText(fileName);
            Debug.LogFormat("Read version file: {0}, {1}", fileName, content);
            return CreateVersion(content);
        }
        return CreateVersion(defaultVersion);
    }

    #region Attributes
    /// <summary>
    /// 主版本号
    /// </summary>
    public int majorVersion { get; private set; }
    /// <summary>
    /// 副版本号
    /// </summary>
    public int assistantVersion { get; private set; }
    /// <summary>
    /// 资源版本号
    /// </summary>
    public int bundleVersion { get; private set; }
    /// <summary>
    /// 最大资源版本号
    /// </summary>
    private int BUNDLE_VERSION_MAX = 500;
    #endregion

    GameVersion(string version)
    {
        var vers = version.Split('.');
        if (vers.Length >= 3)
        {
            majorVersion = Convert.ToInt32(vers[0]);
            assistantVersion = Convert.ToInt32(vers[1]);
            bundleVersion = Convert.ToInt32(vers[2]);
        }
        else if (vers.Length == 2)
        {
            majorVersion = Convert.ToInt32(vers[0]);
            assistantVersion = Convert.ToInt32(vers[1]);
            bundleVersion = 0;
        }
        else if (vers.Length == 1)
        {
            majorVersion = Convert.ToInt32(vers[0]);
            assistantVersion = 0;
            bundleVersion = 0;
        }
        else
        {
            majorVersion = 1;
            assistantVersion = 0;
            bundleVersion = 0;
        }
    }

    /// <summary>
    /// 版本号+1
    /// </summary>
    public void VersionIncrease()
    {
        bundleVersion += 1;
        if (bundleVersion > BUNDLE_VERSION_MAX)
        {
            bundleVersion = 0;
            assistantVersion += 1;
            if (assistantVersion > 10)
            {
                majorVersion += 1;
            }
        }
    }

    /// <summary>
    /// 版本号-1
    /// </summary>
    public void VersionDecrease()
    {
        bundleVersion -= 1;
        if (bundleVersion < 0)
        {
            bundleVersion = BUNDLE_VERSION_MAX;
            assistantVersion -= 1;
            if (assistantVersion < 0)
            {
                assistantVersion = 10;
                majorVersion -= 1;
            }
        }
    }

    #region interface implements
    public object Clone()
    {
        return this;
    }

    public int CompareTo(object obj)
    {
        return CompareTo(obj as GameVersion);
    }

    public int CompareTo(GameVersion other)
    {
        if (other == null)
            return -1;
        if (other.ToInt() == this.ToInt())
            return 0;
        return -1;
    }

    public bool Equals(GameVersion other)
    {
        if (other == null) return false;
        if (other.ToInt() == this.ToInt())
            return true;
        return false;
    }
    #endregion

    #region opreator overrides
    public static bool operator <(GameVersion lgv, GameVersion rgv)
    {
        return lgv.ToInt() < rgv.ToInt();
    }

    public static bool operator >(GameVersion lgv, GameVersion rgv)
    {
        return lgv.ToInt() > rgv.ToInt();
    }

    public static bool operator >=(GameVersion lgv, GameVersion rgv)
    {
        return lgv.ToInt() >= rgv.ToInt();
    }

    public static bool operator <=(GameVersion lgv, GameVersion rgv)
    {
        return lgv.ToInt() <= rgv.ToInt();
    }
    #endregion

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    public override string ToString()
    {
        return string.Format("{0}.{1}.{2}", majorVersion, assistantVersion, bundleVersion);
    }

    private int ToInt()
    {
        return majorVersion * 10 * BUNDLE_VERSION_MAX + assistantVersion * BUNDLE_VERSION_MAX + bundleVersion;
    }
}
