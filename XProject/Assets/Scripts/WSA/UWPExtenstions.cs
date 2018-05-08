using System;
using System.Collections;
using System.Reflection;

/// <summary>
/// 通用windows平台相关扩展
/// </summary>
public static partial class UWPExtensions
{

    #region  Type Extensions
    public static bool IsInstanceOfType(this Type type, object obj)
    {
#if NETFX_CORE
        return obj != null && type.GetTypeInfo().IsAssignableFrom(obj.GetType().GetTypeInfo());
#else
        return type.IsInstanceOfType(obj);
#endif
    }

    public static bool IsPublic(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsPublic;
#else
        return type.IsPublic;
#endif
    }

    public static bool IsNotPublic(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsNotPublic;
#else
        return type.IsNotPublic;
#endif
    }

    public static bool IsNestedPublic(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsNestedPublic;
#else
        return type.IsNestedPublic;
#endif
    }

    public static bool IsGenericType(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsGenericType;
#else
        return type.IsGenericType;
#endif
    }

    public static bool IsEnum(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsEnum;
#else
        return type.IsEnum;
#endif
    }

    public static bool IsValueType(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsValueType;
#else
        return type.IsValueType;
#endif
    }

    public static bool IsPrimitive(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsPrimitive;
#else
        return type.IsPrimitive;
#endif
    }

    public static bool IsGenericTypeDefinition(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsGenericTypeDefinition;
#else
        return type.IsGenericTypeDefinition;
#endif
    }

    public static bool IsNested(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsNested;
#else
        return type.IsNested;
#endif
    }

    public static bool IsNestedAssembly(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsNestedAssembly;
#else
        return type.IsNestedAssembly;
#endif
    }

    public static bool IsNestedFamORAssem(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsNestedFamORAssem;
#else
        return type.IsNestedFamORAssem;
#endif
    }

    public static bool IsNestedFamANDAssem(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsNestedFamANDAssem;
#else
        return type.IsNestedFamANDAssem;
#endif
    }

    public static bool IsAbstract(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsAbstract;
#else
        return type.IsAbstract;
#endif
    }

    public static bool IsSealed(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsSealed;
#else
        return type.IsSealed;
#endif
    }

    public static bool IsInterface(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsInterface;
#else
        return type.IsInterface;
#endif
    }
    

    public static bool IsNestedFamily(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsNestedFamily;
#else
        return type.IsNestedFamily;
#endif
    }

    public static bool IsNestedPrivate(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsNestedPrivate;
#else
        return type.IsNestedPrivate;
#endif
    }

    public static Assembly Assembly(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().Assembly;
#else
        return type.Assembly;
#endif
    }

    public static Type BaseType(this Type type)
    {
#if NETFX_CORE
        return type.GetTypeInfo().BaseType;
#else
        return type.BaseType;
#endif
    }

    public static bool IsAssignableFrom(this Type type, Type c)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
#else
        return type.IsAssignableFrom(c);
#endif
    }

    public static Type[] GetGenericArguments(this Type type)
    {
#if NETFX_CORE
        return type.GenericTypeArguments;
#else
        return type.GetGenericArguments();
#endif
    }

    public static bool IsSubclassOf(this Type type, Type t)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsSubclassOf(t);
#else
        return type.IsSubclassOf(t);
#endif
    }

    public static bool IsDefined(this Type type, Type t, bool inherit)
    {
#if NETFX_CORE
        return type.GetTypeInfo().IsDefined(t, inherit);
#else
        return type.IsDefined(t, inherit);
#endif
    }

    /// <summary>
    /// Type.GetCustomAttributes
    /// </summary>
    /// <param name="type"></param>
    /// <param name="attributeType"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static object[] GetCustomAttributes(this Type type, Type attributeType, bool inherit)
    {
#if NETFX_CORE
        return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit) as object[];
#else
        return type.GetCustomAttributes(attributeType, inherit);
#endif
    }

    #endregion

    #region Delegate Extensions

    public static MethodInfo Method(this Delegate del)
    {
#if NETFX_CORE
        return System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(del);
#else
        return del.Method;
#endif
    }

    #endregion 
}
