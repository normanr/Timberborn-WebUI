using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Timberborn.Modding;
using Timberborn.SingletonSystem;

namespace Mods.WebUI.Scripts {
  internal class WebUIServer : ILoadableSingleton, IUnloadableSingleton {

    private readonly ModRepository _modRepository;
    private readonly MainThread _mainThread;
    private readonly WebUISettings _webUISettings;

    private string _rootPath;
    private HttpListener _listener;

    public WebUIServer(ModRepository modRepository, MainThread mainThread, WebUISettings webUISettings) {
      Debug.Log("WebUIServer()");
      _modRepository = modRepository;
      _mainThread = mainThread;
      _webUISettings = webUISettings;
    }

    public void Load() {
      Debug.Log("WebUIServer.Load()");
      _rootPath = _modRepository.Mods
        .First((m) => m.Manifest.Name == "Web UI")
        .ModDirectory.Path;
      Debug.Log("WebUIServer.rootPath = " + _rootPath);

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
        Debug.LogError("WebUIServer.StartServer(): " + ex);
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
        Debug.LogError("WebUIServer.StopServer(): " + e);
      }
    }

    private void HandleRequest(HttpListenerContext context) {
      HttpListenerRequest request = context.Request;
      HttpListenerResponse response = context.Response;

      byte[] buffer;
      try {
        buffer = ProcessRequest(request, response);
      } catch (Exception e) {
        response.StatusCode = 500;
        response.ContentType = "text/plain; charset=utf-8";
        buffer = System.Text.Encoding.UTF8.GetBytes("Error: " + e + "\n");
      }

      // Get a response stream and write the response to it.
      response.ContentLength64 = buffer.Length;
      var output = response.OutputStream;
      output.Write(buffer, 0, buffer.Length);
      // You must close the output stream.
      output.Close();
    }

    private static readonly Dictionary<string, string> mimeTypes = new Dictionary<string, string>() {
      {".log", "text/plain"},
      {".html", "text/html"},
      {".js", "text/javascript"},
      {".css", "text/css"},
      {".ico", "image/vnd.microsoft.icon"},
      {".png", "image/png"},
    };

    protected byte[] ReturnFile(string fileName, HttpListenerResponse response) {
      response.ContentType = mimeTypes[Path.GetExtension(fileName)];
      // File.ReadAllBytes opens the file with FileShare.Read, so it can't open
      // files already open for writing (like Unity's Player.log).
      using (var fs = new FileStream(Path.Combine(_rootPath, fileName),
        FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
        using (var rdr = new BinaryReader(fs)) {
          return rdr.ReadBytes((int)fs.Length);
        }
      }
    }

    protected byte[] ReturnJson(Func<object> func, HttpListenerResponse response) {
      return _mainThread.Invoke(() => {
        try {
          var responseString = JsonConvert.SerializeObject(func());
          response.ContentType = "application/json";
          return System.Text.Encoding.UTF8.GetBytes(responseString);
        } catch (Exception e) {
          response.StatusCode = 500;
          response.ContentType = "text/plain; charset=utf-8";
          return System.Text.Encoding.UTF8.GetBytes("Error: " + e + "\n");
        }
      }).Result;
    }

    protected virtual byte[] ProcessRequest(HttpListenerRequest request, HttpListenerResponse response) {
      if (request.Url.AbsolutePath.ToLower().StartsWith("/assets/", StringComparison.Ordinal)) {
        return ReturnFile(request.Url.AbsolutePath.Substring(1), response);
      }
      switch (request.Url.AbsolutePath.ToLower()) {
        case "/":
          return ReturnFile("Assets/index-menu.html", response);
        case "/favicon.ico":
          return ReturnFile("Assets/favicon.ico", response);
        case "/player.log":
          if (_webUISettings.AllowPlayerLog.Value) {
            return ReturnFile(Application.persistentDataPath + "/Player.log", response);
          }
          Debug.Log("403 Forbidden: " + request.Url);
          response.StatusCode = 403;
          return System.Text.Encoding.UTF8.GetBytes("Forbidden\n");
        default:
          Debug.Log("404 Not Found: " + request.Url);
          response.StatusCode = 404;
          return System.Text.Encoding.UTF8.GetBytes("Not Found\n");
      }
    }
  }

  internal class WebUIInGameServer : WebUIServer {

    private readonly CharacterInformation _characterInformation;

    public WebUIInGameServer(ModRepository modRepository, MainThread mainThread, WebUISettings webUISettings, CharacterInformation characterInformation)
      : base(modRepository, mainThread, webUISettings) {
      Debug.Log("WebUIInGameServer()");
      _characterInformation = characterInformation;
    }

    protected override byte[] ProcessRequest(HttpListenerRequest request, HttpListenerResponse response) {
      switch (request.Url.AbsolutePath.ToLower()) {
        case "/":
          return ReturnFile("Assets/index.html", response);
        case "/characters":
          return ReturnJson(_characterInformation.GetJson, response);
        default:
          return base.ProcessRequest(request, response);
      }
    }
  }
}
