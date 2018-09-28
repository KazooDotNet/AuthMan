using System;

namespace AuthMan.Exceptions
{
	public class NotAuthorized : Exception
	{
		public NotAuthorized() : base()
		{
		}

		public NotAuthorized(string message) : base(message)
		{
		}

		public NotAuthorized(string message, Exception exception) : base(message, exception)
		{
		}
	}
}
