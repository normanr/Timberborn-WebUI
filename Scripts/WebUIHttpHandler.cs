using System;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace Mods.WebUI.Scripts {
  class WebUIHttpHandler : IHttpHandler {
    public bool IsReusable => throw new NotImplementedException();
    private readonly WebUIRequestDelegate _requestDelegate;
    private readonly RequestContext _requestContext;

    internal WebUIHttpHandler(WebUIRequestDelegate requestDelegate, RequestContext requestContext) {
      _requestDelegate = requestDelegate;
      _requestContext = requestContext;
    }

    public void ProcessRequest(HttpContext context) {
      var text = _requestDelegate(_requestContext);
      if (!string.IsNullOrEmpty(text)) {
        var buffer = Encoding.UTF8.GetBytes(text);
        // Get a response stream and write the response to it.
        context.Response.AddHeader("Content-Length", buffer.Length.ToString());
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
      }
    }
  }
}
