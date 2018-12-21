using System.Threading.Tasks;

namespace AuthMan
{
	public interface IPolicy
	{
		IAuthenticate Authenticator { get; set; }
		Task<bool> Handle(string request, params object[] list);
		Task<bool?> Before();
	}
}
