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
		Task<bool?> Setup(HttpContext context);
		bool Authenticated();
		void EnsureAuthenticated();
		Task<bool> Can<T>(Func<T, object> del) where T : IPolicy;
		Task<bool> Can<T>(string action, params object[] list) where T : IPolicy;
		Task<bool> Can(Type type, string action, params object[] list);
		Task Authenticate<T>(string action, params object[] list) where T : IPolicy;
		Task Authenticate<T>(Func<T, object> del) where T : IPolicy;
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

		public AuthMan(IServiceProvider container, AuthManOptions options)
		{
			_container = container;
			_options = options;
		}

		public async Task<bool?> Setup(HttpContext context)
		{
			foreach (var type in _options.Authenticators)
			{
				var auth = (IAuthenticate) ActivatorUtilities.CreateInstance(_container, type);
				var resp = await auth.Authenticate(context);
				if (resp == null)
					return null;
				if (!resp.Value) continue;
				Authenticator = auth;
				return true;
			}
			return false;
		}
		
		public bool Authenticated() =>
			Authenticator != null;


		public async Task<bool> Can<T>(Func<T, object> del) where T : IPolicy
		{
			var policy = GetPolicy<T>();
			var before = await policy.Before();
			if (before != null) return (bool)before;
			return await Utils.ExtractRefTask<bool>(del(policy));
		}

		Task<bool> IAuthMan.Can<T>(string request, params object[] list)
			=> Can(typeof(T), request, list);

		public Task<bool> Can<TPolicy>(string request = null, params object[] list)
			=> Can(typeof(TPolicy), request, list);

		public async Task<bool> Can(Type type, string request = null, params object[] list)
		{
			var policy = GetPolicy(type);
			var before = await policy.Before();
			if (before != null) return (bool)before;
			return Authenticated() && await policy.Handle(request, list);
		}

		public void EnsureAuthenticated()
		{
			if (!Authenticated())
				throw new Exceptions.NotSignedIn();
		}

		public async Task Authenticate<TPolicy>(string request = null, params object[] list) where TPolicy : IPolicy
		{
			EnsureAuthenticated();
			if (!await Can(typeof(TPolicy), request, list))
				throw new Exceptions.NotAuthorized();
		}
		
		public async Task Authenticate<T>(Func<T, object> del) where T : IPolicy
		{
			if (!await Can(del))
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

		private TPolicy GetPolicy<TPolicy>() where TPolicy : IPolicy => (TPolicy) GetPolicy(typeof(TPolicy));

		private IPolicy GetPolicy(Type type)
		{	
			var policy = (IPolicy) ActivatorUtilities.CreateInstance(_container, type);
			policy.Authenticator = Authenticator;
			return policy;
		}

		private TPolicy GetPolicyOrRaise<TPolicy>() => (TPolicy) GetPolicyOrRaise(typeof(TPolicy));

		private IPolicy GetPolicyOrRaise(Type type)
		{
			EnsureAuthenticated();
			return GetPolicy(type);
		}
	}
}
