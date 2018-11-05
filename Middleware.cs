using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using AuthMan.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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

		public async Task Invoke(HttpContext context, IAuthMan authMan, IRenderer renderer)
		{
			try
			{
				if (!context.Session.IsAvailable)
					await context.Session.LoadAsync();
				await authMan.Setup(context);
				context.Items["authMan"] = authMan;
				await _next(context);
			}
			catch (Exception e)
			{
				await RescueFromException(e, context, renderer);
			}
		}

		private Task RescueFromException(Exception e, HttpContext context, IRenderer renderer)
		{
			var originalError = e;
			while (e.InnerException != null)
				e = e.InnerException;

			switch (e)
			{
				case NotSignedIn _:
					_logger.LogDebug(e.ToString());
					return renderer.Handle(context);
				/*case NotAuthorized _:
					Console.WriteLine(e);
					context.Response.StatusCode = 403;
					break;*/
				default:
					ExceptionDispatchInfo.Capture(originalError).Throw();
					break;
			}

			return Task.CompletedTask;
		}
	}
}
