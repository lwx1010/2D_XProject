using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AL
{
	public static class Reflection
	{
		public static FieldInfo GetRuntimeField(this Type type, string name)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeField(type, name);
#else
			return type.GetField(name);
#endif
		}
		public static IEnumerable<FieldInfo> GetRuntimeFields(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeFields(type);
#else
			return type.GetFields();
#endif
		}

		public static PropertyInfo GetRuntimeProperty(this Type type, string name)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeProperty(type, name);
#else
			return type.GetProperty(name);
#endif
		}
		public static IEnumerable<PropertyInfo> GetRuntimeProperties(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeProperties(type);
#else
			return type.GetProperties();
#endif
		}

		public static EventInfo GetRuntimeEvent(this Type type, string name)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeEvent(type, name);
#else
			return type.GetEvent(name);
#endif
		}
		public static IEnumerable<EventInfo> GetRuntimeEvents(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeEvents(type);
#else
			return type.GetEvents();
#endif
		}

		public static MethodInfo GetRuntimeMethod(this Type type, string name, params Type[] parameters)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeMethod(type, name, parameters);
#else
			return type.GetMethod(name, parameters);
#endif
		}
		public static IEnumerable<MethodInfo> GetRuntimeMethods(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeMethods(type);
#else
			return type.GetMethods();
#endif
		}
	}
}
