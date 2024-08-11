using System.Web;
using System.Web.Routing;

namespace Mods.WebUI.Scripts {
  class WebUIRouteHandler : IRouteHandler {
    private readonly WebUIRequestDelegate _requestDelegate;

    internal WebUIRouteHandler(WebUIRequestDelegate requestDelegate) {
      _requestDelegate = requestDelegate;
    }

    public IHttpHandler GetHttpHandler(RequestContext requestContext) {
      return new WebUIHttpHandler(_requestDelegate, requestContext);
    }
  }
}
