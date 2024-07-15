using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Timberborn.Characters;
using Timberborn.EntitySystem;
using Timberborn.EntityPanelSystem;
using Timberborn.Modding;
using Timberborn.SingletonSystem;
using Timberborn.Wellbeing;
using UnityEngine;

namespace Mods.WebUI.Scripts {
  internal class WebUIServer : ILoadableSingleton, IUnloadableSingleton {

    private readonly EntityRegistry _entityRegistry;
    private readonly EntityBadgeService _entityBadgeService;
    private readonly HttpListener _listener;
    private readonly MainThread _mainThread;
    private readonly string _rootPath;

    public WebUIServer(EntityRegistry entityRegistry, EntityBadgeService entityBadgeService, ModRepository modRepository, MainThread mainThread) {
      Debug.Log("WebUIServer()");
      _entityRegistry = entityRegistry;
      _entityBadgeService = entityBadgeService;
      _mainThread = mainThread;

      _rootPath = modRepository.Mods
        .First((m) => m.Manifest.Name == "Web UI")
        .ModDirectory.Path;
      Debug.Log("rootPath = " + _rootPath);

      _listener = new HttpListener();
      _listener.Prefixes.Add("http://*:8080/");
      _listener.Start();
      Task.Run(() => { RunServer(_listener); });
    }

    public void Load() {
      Debug.Log("WebUIServer.Load()");
    }

    public void Unload() {
      Debug.Log("WebUIServer.Unload()");
      _listener.Stop();
    }

    private void RunServer(HttpListener listener) {
      while (true) {
        HttpListenerContext context = listener.GetContext();
        Task.Run(() => HandleRequest(context));
      }
    }

    private void HandleRequest(HttpListenerContext context) {
      HttpListenerRequest request = context.Request;
      HttpListenerResponse response = context.Response;

      byte[] buffer;
      try {
        buffer = ProcessRequest(request, response);
      }
      catch (Exception e) {
        response.StatusCode = 500;
        response.ContentType = "text/plain; charset=utf-8";
        buffer = System.Text.Encoding.UTF8.GetBytes("Error: " + e + "\n");
      }

      // Get a response stream and write the response to it.
      response.ContentLength64 = buffer.Length;
      System.IO.Stream output = response.OutputStream;
      output.Write(buffer, 0, buffer.Length);
      // You must close the output stream.
      output.Close();
    }

    private string ProcessCharacters(HttpListenerRequest request, HttpListenerResponse response) {
      response.ContentType = "application/json";
      // CharacterBatchControlRowFactory has:
      // ✔ CharacterBatchControlRowItemFactory for EntityAvatar from EntityBadgeService
      // * BeaverBuildingsBatchControlRowItemFactory for Home and Workplace from BeaverBuildingsBatchControlRowItem
      // * DeteriorableBatchControlRowItemFactory for Bot.Durability
      // * AdulthoodBatchControlRowItemFactory for Beaver.Adulthood
      // * WellbeingBatchControlRowItemFactory for ✔Wellbeing *Bonuses
      // * StatusBatchControlRowItemFactory for Active Statuses
      return JsonConvert.SerializeObject(_entityRegistry.Entities
          .Select((EntityComponent entity) => entity.GetComponentFast<Character>())
          .Where((c) => c)
          .Select((c) => new Dictionary<string, object>() {
            {"Name", c.FirstName },
            {"Age", c.Age },
            {"Avatar", _entityBadgeService.GetEntityAvatar(c).name },
            {"Wellbeing", c.GetComponentFast<WellbeingTracker>().Wellbeing},
          })
          .OrderBy((c) => c["Avatar"])  // TODO: Improve sort key
          .ThenBy((c) => c["Name"])
        );
    }

    private static readonly Dictionary<string, string> mimeTypes = new Dictionary<string, string>() {
      {".html", "text/html"},
      {".js", "text/javascript"},
      {".css", "text/css"},
      {".ico", "image/vnd.microsoft.icon"},
      {".png", "image/png"},
    };

    private byte[] ReturnFile(string fileName, HttpListenerResponse response) {
      response.ContentType = mimeTypes[Path.GetExtension(fileName)];
      return File.ReadAllBytes(Path.Combine(_rootPath, fileName));
    }

    private byte[] ProcessRequest(HttpListenerRequest request, HttpListenerResponse response) {
      if (request.Url.AbsolutePath.ToLower().StartsWith("/assets/", StringComparison.Ordinal))
      {
        return ReturnFile(request.Url.AbsolutePath.Substring(1), response);
      }
      switch (request.Url.AbsolutePath.ToLower()) {
        case "/characters":
          return _mainThread.Invoke(() => {
            try {
              var responseString = ProcessCharacters(request, response);
              byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
              return buffer;
            }
            catch (Exception e) {
              response.StatusCode = 500;
              response.ContentType = "text/plain; charset=utf-8";
              byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Error: " + e + "\n");
              return buffer;
            }
          }).Result;
        case "/":
          return ReturnFile("Assets/index.html", response);
        case "/favicon.ico":
          return ReturnFile("Assets/favicon.ico", response);
        default:
          Debug.Log("404 Not Found: " + request.Url);
          response.StatusCode = 404;
          return System.Text.Encoding.UTF8.GetBytes("Not Found\n");
      }
    }
  }
}
