using UnityEngine;

namespace Mods.WebUI.Scripts {
  public static class TextureExtensions {
    public static Texture2D DuplicateAsReadable(this Texture2D source, int width, int height) {
      if (width == 0) {
        width = source.width;
      }
      if (height == 0) {
        height = source.height * width / source.width;
      }
      RenderTexture renderTex = RenderTexture.GetTemporary(
                width,
                height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Default);
      try {
        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        try {
          RenderTexture.active = renderTex;
          Texture2D readableText = new Texture2D(width, height);
          readableText.ReadPixels(new Rect(0, 0, width, height), 0, 0);
          readableText.Apply();
          return readableText;
        } finally {
          RenderTexture.active = previous;
        }
      } finally {
        RenderTexture.ReleaseTemporary(renderTex);
      }
    }
  }
}
