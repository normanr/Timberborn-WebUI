using System;

namespace Mods.WebUI.Scripts {
  public static class UserDataSanitizer {

    public static string Sanitize(string path) {
      var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      if (path.StartsWith(myDocuments, StringComparison.InvariantCulture)) {
        path = "..." + path[myDocuments.Length..];
      }
      var i = path.IndexOf("\\1062090\\", StringComparison.InvariantCulture);
      if (i < 0) {
        i = path.IndexOf("/1062090/", StringComparison.InvariantCulture);
      }
      if (i >= 0) {
        path = "..." + path[i..];
      }
      return path;
    }

  }
}
