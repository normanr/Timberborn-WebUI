using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using UnityEngine;
using Timberborn.Modding;
using Timberborn.SingletonSystem;

namespace Mods.WebUI.Scripts {
  public class WebUIServer : ILoadableSingleton, IUnloadableSingleton {

    private readonly ModRepository _modRepository;
    private readonly MainThread _mainThread;
    private readonly WebUISettings _webUISettings;
    private readonly RouteCollection _routes = new RouteCollection();

    internal static string RootPath;
    private HttpListener _listener;

    internal WebUIServer(ModRepository modRepository, MainThread mainThread, WebUISettings webUISettings) {
      _modRepository = modRepository;
      _mainThread = mainThread;
      _webUISettings = webUISettings;
    }

    public void Load() {
      Debug.Log("WebUIServer.Load(), RootPath = " + RootPath);
      AppDomain.CurrentDomain.SetData(".appVPath", "/");
      AppDomain.CurrentDomain.SetData(".appPath", RootPath);

      _webUISettings.Port.ValueChanged += (sender, e) => {
        Debug.Log("WebUIServer._webUISettings.Port.ValueChanged");
        StopServer();
        StartServer();
      };
      StartServer();
    }

    public void Unload() {
      Debug.Log("WebUIServer.Unload()");
      StopServer();
    }

    public void Map(string url, WebUIRequestDelegate requestDelegate, RouteValueDictionary defaults = null, RouteValueDictionary constraints = null) {
      // TODO: If would be nice to move this to _mainThread as MaybeInvokeDelegate.
      if (Attribute.IsDefined(requestDelegate.Method, typeof(OnMainThreadAttribute))) {
        var originalDelegate = requestDelegate;
        requestDelegate = requestContext => {
          return _mainThread.Invoke(() => {
            return originalDelegate(requestContext);
          }).Result;
        };
      }
      _routes.Add(new Route(url.TrimStart('/'), defaults, constraints, new WebUIRouteHandler(requestDelegate)));
    }

    public void MapMethod(HttpMethod method, string url, WebUIRequestDelegate requestDelegate, RouteValueDictionary defaults = null, RouteValueDictionary constraints = null) {
      if (constraints == null) constraints = new RouteValueDictionary();
      constraints.Add("httpmethod", new HttpMethodConstraint(method.Method));
      Map(url, requestDelegate, defaults, constraints);
    }

    public void MapGet(string url, WebUIRequestDelegate requestDelegate, RouteValueDictionary defaults = null, RouteValueDictionary constraints = null) {
      MapMethod(HttpMethod.Get, url, requestDelegate, defaults, constraints);
    }

    private void StartServer() {
      try {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://*:" + _webUISettings.Port.Value + "/");
        listener.Start();
        _webUISettings.Status.SetValue("Listening on port " + _webUISettings.Port.Value);
        _listener = listener;
        Task.Run(() => {
          while (true) {
            HttpListenerContext context = _listener.GetContext();
            Task.Run(() => HandleRequest(context));
          }
        });
      } catch (Exception ex) {
        _webUISettings.Status.SetValue("Failed: " + ex.Message);
        Debug.LogError("WebUIServer.StartServer failed: " + ex);
      }
    }

    private void StopServer() {
      try {
        var listener = _listener;
        _listener = null;
        if (listener == null) {
          return;
        }
        listener.Stop();
        _webUISettings.Status.SetValue("Stopped");
      } catch (Exception e) {
        Debug.LogError("WebUIServer.StopServer failed: " + e);
      }
    }

    private void HandleRequest(HttpListenerContext context) {
      using (var response = context.Response) {
        try {
          var wr = new WebUIWorkerRequest(context.Request, response);
          var httpContext = new HttpContext(wr);
          var contextBase = new HttpContextWrapper(httpContext);
          var routeData = _routes.GetRouteData(contextBase);
          if (routeData == null) {
            Debug.Log("404 Not Found: " + httpContext.Request.Url);
            response.StatusCode = 404;
            response.ContentType = "text/plain; charset=utf-8";
            var buffer = System.Text.Encoding.UTF8.GetBytes("Not Found\n");
            response.ContentLength64 = buffer.Length;
            // Get a response stream and write the response to it.
            response.OutputStream.Write(buffer, 0, buffer.Length);
          } else {
            var ctx = new RequestContext(contextBase, routeData);
            var handler = routeData.RouteHandler.GetHttpHandler(ctx);
            handler.ProcessRequest(httpContext);
            httpContext.Response.Flush();
          }
        } catch (Exception e) {
          Debug.LogError("Error: " + e);
          response.StatusCode = 500;
          response.ContentType = "text/plain; charset=utf-8";
          var buffer = System.Text.Encoding.UTF8.GetBytes("Error: " + e + "\n");
          response.ContentLength64 = buffer.Length;
          // Get a response stream and write the response to it.
          response.OutputStream.Write(buffer, 0, buffer.Length);
        }
      }
    }
  }
}
