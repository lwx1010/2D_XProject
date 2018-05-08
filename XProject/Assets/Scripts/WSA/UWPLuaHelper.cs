#if UNITY_METRO
using UnityEngine;
using System;
using MoonSharp.Interpreter;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
#endif

namespace MoonSharp
{
    namespace Interpreter
    {
        public class UWPLuaHelper
        {
#if UNITY_METRO
            enum RegisterType
            {
                ArrayList = 0,
                Battle_fuben_npc = 1,
                Battle_fuben_info = 2,
                Battle_sample_npc = 3,
                Battle_sample_info = 4,
                Battle_warinfo = 5,
                Battle_player_info = 6,
                Battle_partner_info = 7,
                Battle_statuepvp_info = 8,
                Battle_initBuff = 9,
                S2c_aoi_skill_calinfo = 10,
                Aoi_one_pmove = 11,
                Aoi_playermove = 12,
                Battle_convoy_npc = 13,
                Battle_convoy_info = 14,
                Battle_pata_info = 15,
                Battle_fuben_plot = 16,
                Battle_pata_npc = 17,
                S2c_aoi_syncplayer = 18,
                Aoi_skill_char = 19,
                S2c_aoi_syncnpc = 20,
                S2c_aoi_syncpartner = 21,
                Aoi_add_normalmsg = 22,
                Aoi_ui_skill = 23,
                Aoi_stips_circle = 24,
                Aoi_stips_line = 25,
                S2c_battle_pata_npcrefresh = 26,
                Battle_siege_info = 27,
                Battle_eliminate_info = 28,
                Battle_siege_npc = 29,
                Battle_eliminate_npc = 30,
            }
            public Script script { get; private set; }

            private Dictionary<string, string> m_resources = new Dictionary<string, string>();
            private Dictionary<string, object> instances = new Dictionary<string, object>();

            public UWPLuaHelper()
            {
                script = new Script();
            }

            public void Start()
            {
                script.Options.ScriptLoader = new MoonSharp.Interpreter.Loaders.UWPScriptLoader(!ResourcesManager.Instance.useLocalResources);

                RegisterCSharpToLua();

                List<Type> registerTypes = new List<Type>();
                registerTypes.Add(typeof(Cmd.Battle_fuben_npc));
                registerTypes.Add(typeof(Cmd.Battle_fuben_info));
                registerTypes.Add(typeof(Cmd.Battle_sample_npc));
                registerTypes.Add(typeof(Cmd.Battle_sample_info));
                registerTypes.Add(typeof(Cmd.Battle_warinfo));
                registerTypes.Add(typeof(Cmd.Battle_player_info));
                registerTypes.Add(typeof(Cmd.Battle_partner_info));
                registerTypes.Add(typeof(Cmd.Battle_statuepvp_info));
                registerTypes.Add(typeof(Cmd.Battle_initBuff));
                registerTypes.Add(typeof(Cmd.S2c_aoi_skill_calinfo));
                registerTypes.Add(typeof(Cmd.Aoi_one_pmove));
                registerTypes.Add(typeof(Cmd.Aoi_playermove));
                registerTypes.Add(typeof(Cmd.Battle_convoy_npc));
                registerTypes.Add(typeof(Cmd.Battle_convoy_info));
                registerTypes.Add(typeof(Cmd.Battle_pata_info));
                registerTypes.Add(typeof(Cmd.Battle_fuben_plot));
                registerTypes.Add(typeof(Cmd.Battle_pata_npc));
                registerTypes.Add(typeof(Cmd.S2c_aoi_syncplayer));
                registerTypes.Add(typeof(Cmd.Aoi_skill_char));
                registerTypes.Add(typeof(Cmd.S2c_aoi_syncnpc));
                registerTypes.Add(typeof(Cmd.S2c_aoi_syncpartner));
                registerTypes.Add(typeof(Cmd.Aoi_add_normalmsg));
                registerTypes.Add(typeof(Cmd.Aoi_ui_skill));
                registerTypes.Add(typeof(Cmd.Aoi_stips_circle));
                registerTypes.Add(typeof(Cmd.Aoi_stips_line));
                registerTypes.Add(typeof(Cmd.S2c_battle_pata_npcrefresh));
                registerTypes.Add(typeof(Cmd.Battle_siege_info));
                registerTypes.Add(typeof(Cmd.Battle_eliminate_info));
                registerTypes.Add(typeof(Cmd.Battle_siege_npc));
                registerTypes.Add(typeof(Cmd.Battle_eliminate_npc));

                for (int i = 0; i < registerTypes.Count; ++i)
                {
                    SetClrToScriptCustomConverter(registerTypes[i], LuaProtoRegister.ClassToLuaTable);
                }
            }

            void SetClrToScriptCustomConverter(Type clrDataType, Func<Script, object, DynValue> converter)
            {
                Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion(clrDataType, converter);
            }

            public void RegisterCSharpToLua()
            {
                UserData.RegisterType<CSharpToLua>();
                script.Globals["CSharpToLua"] = UserData.CreateStatic<CSharpToLua>();
            }

            public DynValue Register(int type)
            {
                switch(type)
                {
                    case 0:
                        return Register(typeof(System.Collections.ArrayList));
                    case 10:
                        return Register(typeof(Cmd.S2c_aoi_skill_calinfo));
                    case 11:
                        return Register(typeof(Cmd.Aoi_one_pmove));
                    case 12:
                        return Register(typeof(Cmd.Aoi_playermove));
                    case 18:
                        return Register(typeof(Cmd.S2c_aoi_syncplayer));
                    case 19:
                        return Register(typeof(Cmd.Aoi_skill_char));
                    case 20:
                        return Register(typeof(Cmd.S2c_aoi_syncnpc));
                    case 21:
                        return Register(typeof(Cmd.S2c_aoi_syncpartner));
                    case 22:
                        return Register(typeof(Cmd.Aoi_add_normalmsg));
                    case 23:
                        return Register(typeof(Cmd.Aoi_ui_skill));
                    case 24:
                        return Register(typeof(Cmd.Aoi_stips_circle));
                    case 25:
                        return Register(typeof(Cmd.Aoi_stips_line));
                    case 26:
                        return Register(typeof(Cmd.S2c_battle_pata_npcrefresh));
                    case 31:
                        return Register(typeof(MartialXls));
                    default:
                        return null;
                }               
            }

            public DynValue DoFile(string fileName)
            {
                string code = script.Options.ScriptLoader.LoadFile(fileName, script.Globals) as string;
                return script.DoString(code);
            }

            public DynValue CallLuaFunction(string name, params object[] args)
            {
                //Debug.Log("call lua function " + name);
                return script.Call(script.Globals[name], args);
            }

            public DynValue GetLuaFunction(string name)
            {
                return script.Globals.Get(name);
            }

            public void Call(DynValue luaFunc, params object[] args)
            {
                script.Call(luaFunc, args);
            }

            private DynValue Register(Type type)
            {
                UserData.RegisterType(type);
                string name = Allocate(type.ToString());
                DynValue obj = UserData.Create(Activator.CreateInstance(type));
                script.Globals.Set(name, obj);
                instances.Add(name, obj);
                return obj;
            }

            private string Allocate(string name)
            {
                string[] temps = name.Split('.');
                if (temps.Length > 0)
                    name = string.Format("{0}_{1}", temps[temps.Length - 1], Time.realtimeSinceStartup.ToString());
                if (instances.ContainsKey(name))
                {
                    script.Globals.Remove(name);
                    instances.Remove(name);
                }
                return name;
            }

            public void Clear()
            {
                foreach (var key in instances.Keys)
                {
                    script.Globals.Remove(key);
                }
            }
#endif
        }

#if UNITY_METRO
        [MoonSharpUserData]
        public class CSharpToLua
        {
            public static DynValue CreateType(int type)
            {
                return LuaManager.Instance.helper.Register(type);
            }

            public static void Debug(object msg)
            {
                UnityEngine.Debug.Log(msg);
            }
        }
#endif
    }
}

