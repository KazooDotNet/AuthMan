using System.Threading.Tasks;

namespace Tests.Policies
{
	public class AdminPolicy : BasePolicy
	{
		public bool CanDoAdmin(int dummy) =>
			AdminAuth?.Object.IsAdmin ?? false;

		public Task<bool> CanDoAdminAsync() =>
			Task.FromResult(CanDoAdmin(3));

	}
}
