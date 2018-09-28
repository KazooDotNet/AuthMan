using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace AuthMan
{
	public static class Utils
	{
		
		public static MethodInfo GetMethod(object obj, string methodName, IEnumerable<object> args)
		{
			var type = obj.GetType();
			if (args.Any(arg => arg == null))
				return type.GetMethod(methodName);
			return type.GetMethod(methodName, args.Select(a => a?.GetType()).ToArray()) ??
			       type.GetMethod(methodName);
		}
			

		public static object CallMethod(object obj, MethodInfo method, IEnumerable<object> args)
		{
			var mParams = method.GetParameters();
			var argCount = args.Count();
			if (mParams.Length <= argCount) 
				return method.Invoke(obj, args.ToArray());
			var argList = args.ToList();
			var count = mParams.Length - argCount;
			for (var i=0; i < count; i++)
				argList.Add(Type.Missing);
			return method.Invoke(obj, argList.ToArray());
		}

		public static object CallMethod(object obj, string methodName, IEnumerable<object> args, 
			Func<MethodInfo, MethodInfo> configureMethod = null)
		{
			var method = GetMethod(obj, methodName, args);
			if (configureMethod != null)
				method = configureMethod.Invoke(method);
			return CallMethod(obj, method, args);
		}

		public static async Task<T> ExtractRefTask<T>(object obj) =>
			(T) await ExtractRefTask(obj);

		public static async Task<object> ExtractRefTask(object obj)
		{
			try
			{
				switch (obj)
				{
					case Task task:
						await task;
						return task.GetType().GetProperty("Result")?.GetValue(task);
					default: 
						return obj;
				}	
			}
			catch (AggregateException e)
			{
				ExceptionDispatchInfo.Capture(e.InnerExceptions.First()).Throw();
			}
			return default;
		}

		public static async Task<T?> ExtractValTask<T>(object obj) where T : struct
		{
			try
			{
				switch (obj)
				{
					case Task<T> objTask: return await objTask;
					case Task task:  await task; return null;
					default: return (T?) obj;
				}	
			}
			catch (AggregateException e)
			{
				ExceptionDispatchInfo.Capture(e.InnerExceptions.First()).Throw();
			}
			return default;
		}
	}
}