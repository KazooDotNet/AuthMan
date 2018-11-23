using System;
using System.Collections.Generic;

namespace AuthMan
{
	public class AuthManOptions
	{
		public List<Type> Authenticators { get; } = new List<Type>();
		public Type RendererType { get; private set; }
		public Type AuthMan { get; private set; }

		public AuthManOptions AddUserAuth<TUserMan, TUser>() where TUserMan : IUserMan<TUser>
		{
			Authenticators.Add(typeof(TUserMan));
			return this;
		}

		public AuthManOptions AddAuth<T>() where T : IAuthenticate
		{
			Authenticators.Add(typeof(T));
			return this;
		}

		public AuthManOptions Renderer<T>() where T : IRenderer
		{
			RendererType = typeof(T);
			return this;
		}

		public AuthManOptions AuthManClass<T>() where T : IAuthMan
		{
			AuthMan = typeof(T);
			return this;
		}

	}
}
