using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AuthMan
{
	public abstract class PolicyBase : IPolicy
	{
		public IAuthenticate Authenticator { get; set; }

		public async Task<bool> Handle(string request, params object[] list)
		{
			var beforeMethod = Utils.GetMethod(this, "Before", list);
			if (beforeMethod != null)
			{
				var resp = await GetResponse(beforeMethod, new object[] { });
				if (resp != null)
					return (bool) resp;
			}

			if (request == null)
				return false;

			var handleMethod = Utils.GetMethod(this, request, list);
			if (handleMethod == null)
				throw new ArgumentException($"{request} does not exist");
			return (bool) await GetResponse(handleMethod, list);
		}

		private Task<bool?> GetResponse(MethodInfo method, object[] list)
			=> Utils.ExtractValTask<bool>(Utils.CallMethod(this, method, list));
	}
}
