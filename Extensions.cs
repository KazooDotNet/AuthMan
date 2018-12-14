using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AuthMan
{
	public static class Extensions
	{
		public static IServiceCollection AddAuthMan(this IServiceCollection services, Action<AuthManOptions> action)
		{
			services.Configure(action);
			services.AddTransient(typeof(AuthManActivator<>));
			return services;
		}

		public static IApplicationBuilder UseAuthMan(this IApplicationBuilder app) =>
			app.UseMiddleware<AuthenticationMiddleware>();
		
	}
}
