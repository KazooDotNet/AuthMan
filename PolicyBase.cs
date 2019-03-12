using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace AuthMan
{
	public abstract class PolicyBase : IPolicy
	{
		public IAuthenticate Authenticator { get; set; }

		public async Task<bool> Handle(string request, params object[] list)
		{
			if (request == null)
				return false;
			var handleMethod = Utils.GetMethod(this, request, list);
			if (handleMethod == null)
				throw new ArgumentException($"{request} does not exist on {GetType().FullName}");
			return (bool) await GetResponse(handleMethod, list);
		}

		public virtual Task<bool?> Before() => Task.FromResult<bool?>(null);

		private Task<bool?> GetResponse(MethodInfo method, IEnumerable<object> list)
			=> Utils.ExtractValTask<bool>(Utils.CallMethod(this, method, list));
	}
}
