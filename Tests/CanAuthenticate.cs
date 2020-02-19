using System.Threading.Tasks;
using AuthMan.Exceptions;
using Tests.Authenticators;
using Tests.Policies;
using Tests.Support;
using Xunit;

namespace Tests
{
	public class CanAuthenticate
	{
		[Fact]
		public async Task TypedCan()
		{
			var auth = AuthManFactory.Create<AdminAuth>();
			Assert.True(await auth.Can<AdminPolicy>( ap => ap.CanDoAdmin(3)));
			auth = AuthManFactory.Create<UserAuth>();
			Assert.False(await auth.Can<AdminPolicy>(ap => ap.CanDoAdminAsync()));
			auth = AuthManFactory.Create<NoAuth>();
			Assert.False(await auth.Can<AdminPolicy>(ap => ap.CanDoAdmin(3)));
		}

		[Fact]
		public async Task TypedAuthenticate()
		{
			var auth = AuthManFactory.Create<AdminAuth>();
			await auth.Authenticate<AdminPolicy>(ap => ap.CanDoAdmin(3)); // should throw nothing
			auth = AuthManFactory.Create<UserAuth>();
			await Assert.ThrowsAsync<NotAuthorized>(async () => 
				await auth.Authenticate<AdminPolicy>(ap => ap.CanDoAdminAsync()));
			auth = AuthManFactory.Create<NoAuth>();
			await Assert.ThrowsAsync<NotAuthorized>(async () => 
				await auth.Authenticate<AdminPolicy>(ap => ap.CanDoAdmin(3)));
		}
	}
}
