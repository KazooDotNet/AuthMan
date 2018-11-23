using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AuthMan
{
	public static class Extensions
	{
		public static IServiceCollection AddAuthentication(this IServiceCollection services, Action<AuthManOptions> action)
		{
			services.Configure(action);
			return services;
		}

		public static IApplicationBuilder UseAuthentication(this IApplicationBuilder app) =>
			app.UseMiddleware<AuthenticationMiddleware>();
		
	}
}
