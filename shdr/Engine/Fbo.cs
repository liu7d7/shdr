using OpenTK.Graphics.OpenGL4;

namespace shdr.Engine
{
   public class __fbo
   {
      private static int _active;
      private static readonly Dictionary<int, __fbo> _frames = new();
      private readonly bool _useDepth;
      private int _colorAttachment;
      private int _depthAttachment;
      public int handle;
      private int _height;
      private int _width;

      public __fbo(int width, int height)
      {
         _width = width;
         _height = height;
         _useDepth = false;
         handle = -1;
         init();
         _frames[handle] = this;
      }

      public __fbo(int width, int height, bool useDepth)
      {
         _width = width;
         _height = height;
         _useDepth = useDepth;
         handle = -1;
         init();
         _frames[handle] = this;
      }

      private void dispose()
      {
         GL.DeleteFramebuffer(handle);
         GL.DeleteTexture(_colorAttachment);
         if (_useDepth) GL.DeleteTexture(_depthAttachment);
      }

      private void init()
      {
         if (handle != -1) dispose();

         handle = GL.GenFramebuffer();
         bind();
         _colorAttachment = GL.GenTexture();
         GL.BindTexture(TextureTarget.Texture2D, _colorAttachment);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToBorder);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToBorder);
         GL.TexStorage2D(TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba8, _width, _height);
         GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, _colorAttachment, 0);
         if (_useDepth)
         {
            _depthAttachment = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _depthAttachment);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
               (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
               (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode,
               (int)TextureCompareMode.None);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
               (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
               (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, _width, _height, 0,
               PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
               TextureTarget.Texture2D, _depthAttachment, 0);
         }

         FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
         if (status != FramebufferErrorCode.FramebufferComplete)
            throw new Exception(
               $"Incomplete Framebuffer! {status} should be {FramebufferErrorCode.FramebufferComplete}");

         unbind();
      }

      private void _resize(int width, int height)
      {
         _width = width;
         _height = height;
         init();
      }

      public static void resize(int width, int height)
      {
         foreach (KeyValuePair<int, __fbo> frame in _frames) frame.Value._resize(width, height);
      }

      public void bind_color(TextureUnit unit)
      {
         GL.ActiveTexture(unit);
         GL.BindTexture(TextureTarget.Texture2D, _colorAttachment);
      }

      public int bind_depth(TextureUnit unit)
      {
         if (!_useDepth) throw new Exception("Trying to bind depth texture of a framebuffer without depth!");
         GL.ActiveTexture(unit);
         GL.BindTexture(TextureTarget.Texture2D, _depthAttachment);
         return _depthAttachment;
      }

      public void bind()
      {
         if (handle == _active) return;
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);
         _active = handle;
      }

      public void blit(int handle = 0)
      {
         GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, this.handle);
         GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, handle);
         GL.BlitFramebuffer(0, 0, _width, _height, 0, 0, _width, _height, ClearBufferMask.ColorBufferBit,
            BlitFramebufferFilter.Nearest);
         unbind();
      }

      public static void unbind()
      {
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
         _active = 0;
      }
   }
}