using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AuthMan
{
  public interface IAuthenticate
  {
    Task<bool?> Authenticate(HttpContext context);
    bool? Authenticated { get; }
  }
  
  public interface IAuthenticate<out T> : IAuthenticate
  {
    // The authenticated object (usually a user or a token)
    T Object { get; }
  }
}
