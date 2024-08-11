using System;
using System.IO;
using System.Net;
using System.Web;

namespace Mods.WebUI.Scripts {
  class WebUIWorkerRequest : HttpWorkerRequest {
    private readonly string _app_virtual_dir;
    private readonly string _app_physical_dir;
    private readonly string _page;
    private readonly string _path_info;
    private readonly HttpListenerRequest _request;
    private readonly HttpListenerResponse _response;

    internal WebUIWorkerRequest(HttpListenerRequest request, HttpListenerResponse response) {
      _app_virtual_dir = HttpRuntime.AppDomainAppVirtualPath;
      _app_physical_dir = HttpRuntime.AppDomainAppPath;
      _request = request;
      _response = response;
      _page = request.Url.AbsolutePath.Substring(1);
      int num = _page.IndexOf('/');
      if (num >= 0) {
        _path_info = _page.Substring(num);
        _page = _page.Substring(0, num);
      } else {
        _path_info = "";
      }
    }

    public override string GetHttpVerbName() {
      return _request.HttpMethod;
    }

    public override string GetHttpVersion() {
      return "HTTP/" + _request.ProtocolVersion;
    }

    public override string GetLocalAddress() {
      return _request.LocalEndPoint.Address.ToString();
    }

    public override int GetLocalPort() {
      return _request.LocalEndPoint.Port;
    }

    public override string GetRemoteAddress() {
      return _request.RemoteEndPoint.Address.ToString();
    }

    public override int GetRemotePort() {
      return _request.RemoteEndPoint.Port;
    }

    public override string GetFilePath() {
      string text = Path.Combine(_app_virtual_dir, _page);
      if (text == "") {
        if (!(_app_virtual_dir == "/")) {
          return _app_virtual_dir + "/";
        }
        return _app_virtual_dir;
      }
      return text;
    }

    public override string GetPathInfo() {
      return _path_info;
    }

    public override string GetUriPath() {
      return _request.Url.AbsolutePath;
    }

    public override string GetQueryString() {
      return _request.Url.Query;
    }

    public override string GetRawUrl() {
      return _request.RawUrl;
    }

    public override string MapPath(string virtualPath) {
      if (virtualPath != null && virtualPath.Length == 0) {
        return _app_physical_dir;
      }
      if (!virtualPath.StartsWith(_app_virtual_dir, StringComparison.Ordinal)) {
        throw new ArgumentException("virtualPath is not rooted in the virtual directory");
      }
      string text = virtualPath.Substring(_app_virtual_dir.Length);
      if (text.Length > 0 && text[0] == '/') {
        text = text.Substring(1);
      }
      if (Path.DirectorySeparatorChar != '/') {
        text = text.Replace('/', Path.DirectorySeparatorChar);
      }
      return Path.Combine(_app_physical_dir, text);
    }

    public override void SendStatus(int statusCode, string statusDescription) {
      _response.StatusCode = statusCode;
      _response.StatusDescription = statusDescription;
    }

    public override void SendKnownResponseHeader(int index, string value) {
      _response.AddHeader(GetKnownResponseHeaderName(index), value);
    }

    public override void SendUnknownResponseHeader(string name, string value) {
      _response.AddHeader(name, value);
    }

    public override void SendResponseFromFile(IntPtr handle, long offset, long length) {
      throw new NotImplementedException();
    }

    public override void SendResponseFromFile(string filename, long offset, long length) {
      // File.ReadAllBytes opens the file with FileShare.Read, so it can't open
      // files already open for writing (like Unity's Player.log).
      using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
        using (var rdr = new BinaryReader(fs)) {
          var buffer = rdr.ReadBytes((int)fs.Length);
          _response.ContentLength64 = buffer.Length;
          _response.OutputStream.Write(buffer, 0, buffer.Length);
        }
      }
    }

    public override void SendResponseFromMemory(byte[] data, int length) {
      _response.OutputStream.Write(data, 0, length);
    }

    public override void FlushResponse(bool finalFlush) {
      _response.OutputStream.Flush();
    }

    public override void EndOfRequest() {
    }
  }
}
