using UnityEngine;
using System.Collections;
using LuaInterface;
using System;

namespace LuaFramework {
    public class LuaManager : Manager {
        private LuaState lua;
        private LuaLoader loader;
        private LuaLooper loop = null;

        public delegate void OnBundleProgressChanged(float percent);
        public OnBundleProgressChanged bundleProgress;

        public IBinder luaBinder { get; set; }
        public IDelegateCreator delCreator { get; set; }

        public LuaState mainLua
        {
            get { return lua; }
        }

        // Use this for initialization
        void Init() {
            loader = new LuaLoader();
            lua = new LuaState();
            this.OpenLibs();
            lua.LuaSetTop(0);

            LuaBinder.Bind(lua, luaBinder);
            DelegateFactory.Init(delCreator);
            LuaCoroutine.Register(lua, this);
        }

        public void InitStart(Action onInitComplete) {
            Init();
            InitLuaPath();
            InitLuaBundle(() => {
                this.lua.Start();    //启动LUAVM
                this.StartMain();
                this.StartLooper();
                if (onInitComplete != null)
                    onInitComplete();
                onInitComplete = null;
            });
        }

        void StartLooper() {
            loop = gameObject.AddComponent<LuaLooper>();
            loop.luaState = lua;
        }

        //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
        protected void OpenCJson() {
            lua.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            lua.OpenLibs(LuaDLL.luaopen_cjson);
            lua.LuaSetField(-2, "cjson");

            lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
            lua.LuaSetField(-2, "cjson.safe");
        }

        void StartMain() {
            lua.DoFile("Main.lua");

            LuaFunction main = lua.GetFunction("Main");
            main.Call();
            main.Dispose();
            main = null;    
        }
        
        /// <summary>
        /// 初始化加载第三方库
        /// </summary>
        void OpenLibs() {
            lua.OpenLibs(LuaDLL.luaopen_pb);      
            //lua.OpenLibs(LuaDLL.luaopen_sproto_core);
            lua.OpenLibs(LuaDLL.luaopen_protobuf_c);
            lua.OpenLibs(LuaDLL.luaopen_lpeg);
            lua.OpenLibs(LuaDLL.luaopen_bit);
            lua.OpenLibs(LuaDLL.luaopen_socket_core);

            this.OpenCJson();
        }

        /// <summary>
        /// 初始化Lua代码加载路径
        /// </summary>
        void InitLuaPath() {
            if (!AppConst.LuaBundleMode) {
                string rootPath = AppConst.FrameworkRoot;
                lua.AddSearchPath(rootPath + "/Lua");
                lua.AddSearchPath(rootPath + "/ToLua/Lua");
            } else {
                lua.AddSearchPath(Util.DataPath + "lua");
            }
        }

        /// <summary>
        /// 初始化LuaBundle
        /// </summary>
        void InitLuaBundle(Action onBundleLoaded) {
            if (AppConst.LuaBundleMode)
                StartCoroutine(LoadBundle(onBundleLoaded));
            else if (onBundleLoaded != null)
                onBundleLoaded();
        }

        IEnumerator LoadBundle(Action onBundleLoaded)
        {
            string bundlePath = Util.DataPath + "lua/";
            if (!System.IO.Directory.Exists(bundlePath))
                System.IO.Directory.CreateDirectory(bundlePath);
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(bundlePath);
            System.IO.FileInfo[] files = dirInfo.GetFiles("*.unity3d", System.IO.SearchOption.AllDirectories);
            if (bundleProgress != null) bundleProgress(0);
            for (int i = 0; i < files.Length; ++i)
            {
                var fileName = files[i].FullName.Replace('\\', '/');
                var dataPath = Util.DataPath.Replace('\\', '/');
                string bundle = fileName.Replace(dataPath, "");
                loader.AddBundle(bundle);
                if (bundleProgress != null) bundleProgress((float)i / (float)files.Length);
                yield return 0;
            }
            if (onBundleLoaded != null) onBundleLoaded();
        }

        public void DoFile(string filename) {
            lua.DoFile(filename);
        }

        public T DoFile<T>(string filename) {
            return lua.DoFile<T>(filename);
        }

        // Update is called once per frame
        public object[] CallFunction(string funcName, params object[] args) {
            if (lua == null) return null;
            LuaFunction func = lua.GetFunction(funcName);
            if (func != null) {
                return func.LazyCall(args);
            }
            return null;
        }

        public void LuaGC() {
            if(lua != null)
                lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
        }

        public void Close() {
            if (loop != null) loop.Destroy();
            loop = null;

            if (lua != null) lua.Dispose();
            lua = null;
            loader = null;
        }

        LuaTable profiler = null;
        public void AttachProfiler()
        {
            if (profiler == null)
            {
                profiler = lua.Require<LuaTable>("UnityEngine.Profiler");
                profiler.Call("start", profiler);
            }
        }
        public void DetachProfiler()
        {
            if (profiler != null)
            {
                profiler.Call("stop", profiler);
                profiler.Dispose();
            }
        }
    }
}