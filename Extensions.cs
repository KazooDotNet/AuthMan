using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AuthMan
{
	public static class Extensions
	{
	  
	  public static IServiceCollection AddAuthentication(this IServiceCollection services, Action<AuthManOptions> action, Assembly assembly = null)
	  {
	    AddAuthentication(services, action, typeof(global::AuthMan.AuthMan), assembly);
	    return services;
	  }
	  
		public static IServiceCollection AddAuthentication<TAuthMan>(this IServiceCollection services, Action<AuthManOptions> action, Assembly assembly = null)
			where TAuthMan : class, IAuthMan
		{
		  AddAuthentication(services, action, typeof(TAuthMan), assembly);
			return services;
		}

	  private static void AddAuthentication(IServiceCollection services, Action<AuthManOptions> action, Type type, Assembly assembly)
	  {
	    services.Configure(action);
	    if (assembly == null)
	    {
	      RegisterClasses(Assembly.GetCallingAssembly(), services);
	      RegisterClasses(Assembly.GetEntryAssembly(), services);
	    }
	    else
	      RegisterClasses(assembly, services);
	    services.AddTransient(typeof(IAuthMan), type);
	  }

	  public static IApplicationBuilder UseAuthentication(this IApplicationBuilder app) =>
        app.UseMiddleware<AuthenticationMiddleware>();

		private static void RegisterClasses(Assembly assembly, IServiceCollection services)
		{
			var policyType = typeof(IPolicy);
			var rendererType = typeof(IRenderer);
		  var authType = typeof(IAuthenticate);
			foreach (var type in assembly.ExportedTypes)
			{	
				if (policyType.IsAssignableFrom(type) && type != policyType && !type.IsAbstract)
					services.AddTransient(type);
			  else if (authType.IsAssignableFrom(type) && type != authType && !type.IsAbstract)
				  services.AddTransient(type);
				else if (rendererType.IsAssignableFrom(type) && type != rendererType && !type.IsAbstract)
					services.AddTransient(rendererType, type);
			}
		}
		
	}
}
