namespace Tests.Authenticators
{
	public class NoAuth : BaseAuth
	{
		public NoAuth()
		{
			Authed = false;
		}
	}
}
