using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace shdr.Engine
{
   // taken from https://github.com/opentk/LearnOpenTK/blob/master/Common/Texture.cs
   public class __texture
   {
      private static int _active;
      private static readonly Dictionary<int, __texture> _textures = new();
      private readonly int _handle;
      public readonly float height;

      public readonly float width;

      private __texture(int glHandle, int width, int height)
      {
         _handle = glHandle;
         this.width = width;
         this.height = height;
         _textures[glHandle] = this;
      }

      public static __texture load_from_file(string path)
      {
         int handle = GL.GenTexture();

         GL.ActiveTexture(TextureUnit.Texture0);
         GL.BindTexture(TextureTarget.Texture2D, handle);

         StbImage.stbi_set_flip_vertically_on_load(1);
         using Stream stream = File.OpenRead(path);
         ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

         GL.TexImage2D(TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            image.Width,
            image.Height,
            0,
            PixelFormat.Bgra,
            PixelType.UnsignedByte,
            image.Data);

         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.NearestMipmapNearest);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

         GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

         return new __texture(handle, image.Width, image.Height);
      }

      public static __texture load_from_buffer(byte[] buffer, int width, int height, PixelFormat format,
         PixelInternalFormat internalFormat, TextureMinFilter minFilter = TextureMinFilter.LinearMipmapLinear,
         TextureMagFilter magFilter = TextureMagFilter.Linear)
      {
         int handle = GL.GenTexture();

         GL.ActiveTexture(TextureUnit.Texture0);
         GL.BindTexture(TextureTarget.Texture2D, handle);

#pragma warning disable CA1416
         GL.TexImage2D(TextureTarget.Texture2D,
            0,
            internalFormat,
            width,
            height,
            0,
            format,
            PixelType.UnsignedByte,
            buffer);

         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);

         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

         if (minFilter is TextureMinFilter.LinearMipmapLinear or TextureMinFilter.LinearMipmapNearest or TextureMinFilter.NearestMipmapNearest)
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

         return new __texture(handle, width, height);
#pragma warning restore CA1416
      }

      public void bind(TextureUnit unit)
      {
         if (_handle == _active) return;
         GL.ActiveTexture(unit);
         GL.BindTexture(TextureTarget.Texture2D, _handle);
         _active = _handle;
      }

      public static void unbind()
      {
         GL.BindTexture(TextureTarget.Texture2D, 0);
         _active = 0;
      }

      public static Vector2 current_bounds()
      {
         if (!_textures.ContainsKey(_active)) return new Vector2(1, 1);
         __texture current = _textures[_active];
         return new Vector2(current.width, current.height);
      }
   }
}