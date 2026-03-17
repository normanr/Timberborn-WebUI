using System.Threading.Tasks;
using System.Web.Routing;

namespace Mods.WebUI.Scripts;

public delegate Task<string> WebUIRequestDelegate(RequestContext requestContext);
