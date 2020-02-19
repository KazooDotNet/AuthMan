namespace Tests.Authenticators
{
	public class UserAuth : BaseAuth
	{
		public UserAuth()
		{
			User = new User { IsAdmin = false };
			Authed = true;
		}
	}
}
