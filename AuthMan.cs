using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AuthMan
{
	public interface IAuthMan
	{
		Task Setup(HttpContext context);
		bool Authenticated();
		void EnsureAuthenticated();
		Task<bool> Can<T>(string action, params object[] list);
		Task<bool> Can(Type type, string action, params object[] list);
		Task Authenticate<T>(string action, params object[] list);
		IAuthenticate Authenticator { get; }
	}

	public class AuthMan : IAuthMan
	{
		private readonly IServiceProvider _container;
		private readonly AuthManOptions _options;
		public IAuthenticate Authenticator { get; private set; }

		public AuthMan(IServiceProvider container, IOptions<AuthManOptions> options)
		{
			_container = container;
			_options = options.Value;
		}

		public async Task Setup(HttpContext context)
		{
			foreach (var type in _options.Authenticators)
			{
				var auth = (IAuthenticate) _container.GetService(type);
				await auth.Authenticate(context);
				if (!auth.Authenticated) continue;
				Authenticator = auth;
				break;
			}
		}

		public bool Authenticated() =>
			Authenticator != null;


		Task<bool> IAuthMan.Can<T>(string request, params object[] list)
			=> Can(typeof(T), request, list);

		public Task<bool> Can<TPolicy>(string request = null, params object[] list)
			=> Can(typeof(TPolicy), request, list);

		public Task<bool> Can(Type type, string request = null, params object[] list)
		{
			if (!Authenticated())
				return Task.FromResult(false);
			if (list.Length == 1 && list[0] != null && list[0].GetType().IsArray)
				list = (object[]) list[0];
			var policy = (IPolicy) _container.GetService(type);
			policy.Authenticator = Authenticator;
			return policy.Handle(request, list);
		}

		public void EnsureAuthenticated()
		{
			if (!Authenticated())
				throw new Exceptions.NotSignedIn();
		}

		public async Task Authenticate<TPolicy>(string request = null, params object[] list)
		{
			EnsureAuthenticated();
			if (!await Can(typeof(TPolicy), request, list))
				throw new Exceptions.NotAuthorized();
		}

		public IQueryable<TQueriable> Scope<TPolicy, TQueriable>(params object[] args)
		{
			return (IQueryable<TQueriable>)
				Utils.CallMethod(GetPolicy<TPolicy>(), "Scope", args);
		}

		public async Task<IQueryable<TQueriable>> ScopeAsync<TPolicy, TQueriable>(params object[] args)
		{
			return await Utils.ExtractRefTask<IQueryable<TQueriable>>(
				Utils.CallMethod(GetPolicy<TPolicy>(), "ScopeAsync", args)
			);
		}

		private IPolicy GetPolicy<TPolicy>() => GetPolicy(typeof(TPolicy));

		private IPolicy GetPolicy(Type type)
		{
			EnsureAuthenticated();
			var policy = (IPolicy) _container.GetService(type);
			policy.Authenticator = Authenticator;
			return policy;
		}
	}
}
