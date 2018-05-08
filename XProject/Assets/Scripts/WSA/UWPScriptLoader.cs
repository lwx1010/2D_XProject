using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MoonSharp.Interpreter.Loaders
{
    /// <summary>
    /// A script loader which can load scripts from assets in Unity3D.
    /// Scripts should be saved as .txt files in a subdirectory of Assets/Resources.
    /// 
    /// When MoonSharp is activated on Unity3D and the default script loader is used,
    /// scripts should be saved as .txt files in Assets/Resources/MoonSharp/Scripts.
    /// </summary>
#if UNITY_METRO
    public class UWPScriptLoader : ScriptLoaderBase
    {
        Dictionary<string, string> m_Resources = new Dictionary<string, string>();

        /// <summary>
        /// The default path where scripts are meant to be stored (if not changed)
        /// </summary>
        public const string DEFAULT_PATH = "LuaScript";

        public bool beZip = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityAssetsScriptLoader"/> class.
        /// </summary>
        /// <param name="assetsPath">The path, relative to Assets/Resources. For example
        /// if your scripts are stored under Assets/Resources/Scripts, you should
        /// pass the value "Scripts". If null, "MoonSharp/Scripts" is used. </param>
        public UWPScriptLoader(bool beZip = false, string assetsPath = null)
        {
            assetsPath = assetsPath ?? DEFAULT_PATH;
            this.beZip = beZip;
            LoadLuaResources(assetsPath);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UnityAssetsScriptLoader"/> class.
        /// </summary>
        /// <param name="scriptToCodeMap">A dictionary mapping filenames to the proper Lua script code.</param>
        public UWPScriptLoader(Dictionary<string, string> scriptToCodeMap)
        {
            m_Resources = scriptToCodeMap;
        }


        void LoadLuaResources(string assetsPath)
        {
            try
            {
                if (beZip)
                {
                    string filePath = string.Format("{0}/{1}", Global_Tools.GetResourcesPath(), Riverlake.MD5.ComputeString("textasset.bytes"));
                    byte[] data = File.ReadAllBytes(filePath);
                    UnityEngine.AssetBundle bundle = UnityEngine.AssetBundle.LoadFromMemory(Riverlake.Crypto.Decode(data));
                    foreach (UnityEngine.TextAsset lua in bundle.LoadAllAssets())
                    {
                        m_Resources.Add(lua.name, System.Text.Encoding.UTF8.GetString(lua.bytes));
                    }
                }
                else
                {
                    string path = UnityEngine.Application.dataPath + "/LuaScript";
                    DirectoryInfo dirInfo = new DirectoryInfo(path);
                    foreach (var lua in dirInfo.GetFiles("*.lua", SearchOption.AllDirectories))
                    {
                        string assetPath = lua.FullName.Substring(lua.FullName.IndexOf("LuaScript") + 10).Replace('\\', '/');
                        string name = assetPath.Replace('/', '_');
                        m_Resources.Add(name, File.ReadAllText(lua.FullName));
                    }
                }
            }
            catch (Exception ex)
            {
#if !NETFX_CORE
                Console.WriteLine("Error initializing UnityScriptLoader : {0}", ex);
#endif
            }
        }

        private string GetFileName(string filename)
        {
            if (filename.IndexOf('/') != -1)
                filename = filename.Replace('/', '_');
            else if (filename.IndexOf('\\') != -1)
                filename = filename.Replace('\\', '_');

            if (!filename.CustomEndsWith(".lua"))
                filename += ".lua";

            return filename;
        }

        /// <summary>
        /// Opens a file for reading the script code.
        /// It can return either a string, a byte[] or a Stream.
        /// If a byte[] is returned, the content is assumed to be a serialized (dumped) bytecode. If it's a string, it's
        /// assumed to be either a script or the output of a string.dump call. If a Stream, autodetection takes place.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="globalContext">The global context.</param>
        /// <returns>
        /// A string, a byte[] or a Stream.
        /// </returns>
        /// <exception cref="System.Exception">UnityAssetsScriptLoader.LoadFile : Cannot load  + file</exception>
        public override object LoadFile(string file, Table globalContext)
        {
            UnityEngine.Debug.Log(string.Format("load lua file: Assets/{0}/{1}", DEFAULT_PATH, file));
            file = GetFileName(file);

            if (m_Resources.ContainsKey(file))
            {
                return m_Resources[file];
            }
            else
            {
                var error = string.Format(
@"Cannot load script '{0}'. By default, scripts should be .txt files placed under a Assets/{1} directory.
If you want scripts to be put in another directory or another way, use a custom instance of UnityAssetsScriptLoader or implement
your own IScriptLoader (possibly extending ScriptLoaderBase).", file, DEFAULT_PATH);

                throw new Exception(error);
            }
        }

        /// <summary>
        /// Checks if a given file exists
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public override bool ScriptFileExists(string file)
        {
            file = GetFileName(file);
            return m_Resources.ContainsKey(file);
        }


        /// <summary>
        /// Gets the list of loaded scripts filenames (useful for debugging purposes).
        /// </summary>
        /// <returns></returns>
        public string[] GetLoadedScripts()
        {
            return m_Resources.Keys.ToArray();
        }
    }
#endif
}

