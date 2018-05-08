#if UNITY_METRO
using UnityEngine;
using System.Collections;
using Cmd;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Reflection;
#endif

namespace MoonSharp
{
    namespace Interpreter
    {
        public class LuaProtoRegister
        {
#if UNITY_METRO
            public static DynValue ClassToLuaTable(Script script, object obj)
            {
                MoonSharp.Interpreter.Table table = new MoonSharp.Interpreter.Table(script);
                var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                for (int i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];

                    var value = property.GetValue(obj, null);
                    if (value == null)
                        continue;

                    if (value.GetType() == typeof(string))
                    {
                        table.Set(property.Name, DynValue.NewString((string)value));
                    }
                    else if (value.GetType() == typeof(int))
                    {
                        table.Set(property.Name, DynValue.NewNumber((int)value));
                    }
                    else if (value.GetType() == typeof(float))
                    {
                        table.Set(property.Name, DynValue.NewNumber((float)value));
                    }
                    else if (value.GetType() == typeof(bool))
                    {
                        table.Set(property.Name, DynValue.NewBoolean((bool)value));
                    }
                    else if (value is ProtoBuf.IExtensible)
                    {
                        table.Set(property.Name, ClassToLuaTable(script, value));
                    }
                    else if (value is IList)
                    {
                        IList list = value as IList;
                        MoonSharp.Interpreter.Table value_table = new MoonSharp.Interpreter.Table(script);
                        for (int j = 0; j < list.Count; ++j)
                        {
                            DynValue item = null;
                            if (list[j] is ProtoBuf.IExtensible)
                                item = ClassToLuaTable(script, list[j]);
                            else if (list[j] is int)
                                item = DynValue.NewNumber((int)list[j]);
                            else if (list[j] is float)
                                item = DynValue.NewNumber((float)list[j]);
                            else if (list[j] is string)
                                item = DynValue.NewString((string)list[j]);
                            else if (list[j] is bool)
                                item = DynValue.NewBoolean((bool)list[j]);
                            else
                                Debug.LogError("Convert List to lua table error: unsupported types " + list[j].GetType());
                            value_table.Append(item);
                        }
                        table.Set(property.Name, DynValue.NewTable(value_table));
                    }
                    else
                    {
                        Debug.LogError("Convert class to lua table error: unsupported types " + value);
                    }
                }

                return DynValue.NewTable(table);
            }
#endif
        }
    }
}

