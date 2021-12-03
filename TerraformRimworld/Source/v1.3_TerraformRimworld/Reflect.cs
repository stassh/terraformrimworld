using System;
using System.Linq;
using System.Reflection;

namespace TerraformRimworld
{
	public static class Reflect
	{
		public static object GetMemberValue(this Type type, string name)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			var field = type?.GetField(name, bindingFlags);
			return field?.GetValue(null);
		}
		public static void SetMemberValue(this Type type, string name, object value)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			var field = type?.GetField(name, bindingFlags);
			field?.SetValue(null, value);
		}
		public static object GetMemberValue(this object obj, string name)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var field = obj?.GetType().GetField(name, bindingFlags);
			return field?.GetValue(obj);
		}
		public static void SetMemberValue(this object obj, string name, object value)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var field = obj?.GetType().GetField(name, bindingFlags);
			field?.SetValue(obj, value);
		}
		public static object CallMethod(this object obj, string name, object[] param)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			return obj?.GetType().GetMethod(name, bindingFlags)?.Invoke(obj, param);
			//return obj?.GetType().GetMethods().Where(x => x.Name == name).FirstOrDefault(x => x.IsGenericMethod)?.Invoke(obj, param);
		}
		public static object CallMethod(this Type type, string name, object[] param)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			return type?.GetMethod(name, bindingFlags)?.Invoke(null, param);
		}
		public static object CallMethod(this MethodInfo mi, object[] param, object instance = null)
		{
			return mi?.Invoke(instance, param);
		}
		public static MethodInfo GetMethodInfo(this Type type, string name)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			return type?.GetMethod(name, bindingFlags);
		}
		public static MethodInfo GetMethodInfo(this object obj, string name)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			return obj?.GetType().GetMethod(name, bindingFlags);
		}
		public static Type GetAssemblyType(string name, string type)
		{
			Assembly a = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == name);
			return a.GetType(type);
		}
		public static Type GetAType(string nameSpace, string className)
		{			
			return Verse.GenTypes.GetTypeInAnyAssembly(nameSpace + "." + className, nameSpace);
		}
	}
}
