using System;
using System.Threading.Tasks;
using AuthMan;
using Tests.Authenticators;

namespace Tests.Policies
{
	public class BasePolicy : PolicyBase
	{
		protected AdminAuth AdminAuth { get; private set; }
		protected UserAuth UserAuth { get; private set; }

		public override Task<bool?> Before()
		{
			switch (Authenticator)
			{
				case AdminAuth aa:
					AdminAuth = aa;
					break;
				case UserAuth au:
					UserAuth = au;
					break;
				case null:
					// do nothing
					break;
				default:
					throw new InvalidOperationException($"Unknown auth type passed: {Authenticator?.GetType().FullName} ?? (null)");
			}

			return Task.FromResult((bool?)null);
		}
		
		
	}
}
