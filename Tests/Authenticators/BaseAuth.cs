using System.Threading.Tasks;
using AuthMan;
using Microsoft.AspNetCore.Http;

namespace Tests.Authenticators
{

	public class BaseAuth : IAuthenticate<User>
	{
		protected User User { get; set; }
		protected bool? Authed { get; set; }
		public Task<bool?> Authenticate(HttpContext context) => Task.FromResult(Authed);
		public bool? Authenticated { get => Authed; }
		public User Object { get => User; }
	}
}
