using System.Threading.Tasks;
using AuthMan;
using Microsoft.AspNetCore.Http;

namespace Tests.Authenticators
{
	public class AdminAuth : BaseAuth
	{
		public AdminAuth()
		{
			User = new User {IsAdmin = true};
			Authed = true;
		}
		
	}
}
