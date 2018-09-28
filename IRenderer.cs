using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AuthMan
{
	public interface IRenderer
	{
		Task Handle(HttpContext context);
	}
}
