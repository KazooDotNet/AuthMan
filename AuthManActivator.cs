using System;
using Microsoft.Extensions.DependencyInjection;

namespace AuthMan
{
	public class AuthManActivator<T> where T : IAuthMan
	{
		private readonly IServiceProvider _provider;
		public AuthManActivator(IServiceProvider provider) => _provider = provider;
		public T Activate() => (T) ActivatorUtilities.CreateInstance(_provider, typeof(T));
	}
}
