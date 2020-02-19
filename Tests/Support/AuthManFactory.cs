using System;
using System.Collections.Generic;
using AuthMan;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Support
{
	public class AuthManFactory
	{
		public static IAuthMan Create<T>()
			=> Create(typeof(T));
		public static IAuthMan Create(Type authenticator) => 
			Create(new List<Type> {authenticator});
		public static IAuthMan Create(List<Type> authenticators)
		{
			IServiceCollection builder = new ServiceCollection();
			var opts = new AuthManOptions();
			opts.AddAuth(authenticators);
			var am = new AuthMan.AuthMan(builder.BuildServiceProvider(), opts);
			am.Setup(null);
			return am;
		}
	}
}
