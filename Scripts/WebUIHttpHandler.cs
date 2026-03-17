using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace Mods.WebUI.Scripts;

class WebUIHttpHandler : IHttpHandler {
  public bool IsReusable => throw new NotImplementedException();
  private readonly WebUIRequestDelegate _requestDelegate;
  private readonly RequestContext _requestContext;

  internal WebUIHttpHandler(WebUIRequestDelegate requestDelegate, RequestContext requestContext) {
    _requestDelegate = requestDelegate;
    _requestContext = requestContext;
  }

  private async Task<string> ProcessRequestAsync() {
    return await _requestDelegate(_requestContext);
  }

  public void ProcessRequest(HttpContext context) {
    // Run ProcessRequestAsync in this thread
    var text = ProcessRequestAsync().GetAwaiter().GetResult();
    if (!string.IsNullOrEmpty(text)) {
      var buffer = Encoding.UTF8.GetBytes(text);
      // Get a response stream and write the response to it.
      var response = context.Response;
      response.AddHeader("Content-Length", buffer.Length.ToString());
      response.OutputStream.Write(buffer, 0, buffer.Length);
    }
  }
}
