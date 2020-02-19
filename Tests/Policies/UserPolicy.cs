namespace Tests.Policies
{
	public class UserPolicy : BasePolicy
	{
		public bool CanDoThing() => true;
		public bool CantDoThing() => false;
		public bool UsersOnly => !(UserAuth?.Object.IsAdmin ?? true);
	}
}
