using System;
using System.Threading.Tasks;
using AuthMan.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthMan
{
	public class AuthenticationMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<AuthenticationMiddleware> _logger;

		public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task Invoke(HttpContext context, IOptions<AuthManOptions> optsThing, IServiceProvider provider)
		{
			var opts = optsThing.Value;
			if (context.Features.Get<ISessionFeature>() != null && !context.Session.IsAvailable)
				await context.Session.LoadAsync();
			var authMan = (IAuthMan) ActivatorUtilities.CreateInstance(provider, opts.AuthMan ?? typeof(AuthMan));
			var resp = await authMan.Setup(context);
			if (resp != null) // Cancel any further requests if null 
			{
				context.Items["authMan"] = authMan;
				await _next(context);
			}
		}
		
	}
}
