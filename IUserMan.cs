using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AuthMan
{
	public interface IUserMan<TUser> : IAuthenticate<TUser>
	{
		TUser Login(HttpContext context);
	  void Login(HttpContext context, TUser user);
		Task<TUser> LoginAsync(HttpContext context);
		TUser CurrentUser { get; }
		Task LoadUserAsync(HttpContext context);
		void LoadUser(HttpContext context);
		void Logout(HttpContext context);
		Task LogoutAsync(HttpContext context);
	}
}
