using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
				var auth = (IAuthenticate) ActivatorUtilities.CreateInstance(_container, type);
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
			var policy = GetPolicy(type);
			var before = policy.Before();
			if (before != null) return Task.FromResult((bool)before);
			return !Authenticated() ? Task.FromResult(false) : policy.Handle(request, list);
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

		public IQueryable<TQueriable> Scope<TPolicy, TQueriable>(params object[] args) where TPolicy : IPolicy
		{
			var policy = GetPolicyOrRaise<TPolicy>();
			policy.Before();
			return (IQueryable<TQueriable>) Utils.CallMethod(policy, "Scope", args);
		}

		public async Task<IQueryable<TQueriable>> ScopeAsync<TPolicy, TQueriable>(params object[] args) where TPolicy : IPolicy
		{
			return await Utils.ExtractRefTask<IQueryable<TQueriable>>(
				Utils.CallMethod(GetPolicyOrRaise<TPolicy>(), "ScopeAsync", args)
			);
		}

		private IPolicy GetPolicy<TPolicy>() => GetPolicy(typeof(TPolicy));

		private IPolicy GetPolicy(Type type)
		{	
			var policy = (IPolicy) ActivatorUtilities.CreateInstance(_container, type);
			policy.Authenticator = Authenticator;
			return policy;
		}

		private IPolicy GetPolicyOrRaise<TPolicy>() => GetPolicyOrRaise(typeof(TPolicy));

		private IPolicy GetPolicyOrRaise(Type type)
		{
			EnsureAuthenticated();
			return GetPolicy(type);
		}
	}
}
