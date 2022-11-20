using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace shdr.Engine
{
   public static class __render_system
   {
      private static readonly __shader _basic = new("Resource/Shader/basic.vert", "Resource/Shader/basic.frag");

      public static __shader user = null;

      private static Matrix4 _projection;
      private static Matrix4 _lookAt;
      private static readonly Matrix4[] _model = new Matrix4[7];
      private static int _modelIdx;

      public static readonly __mesh basic = new(__mesh.__draw_mode.triangle, _basic, __vao.__attrib.float3, __vao.__attrib.float4);

      public static readonly __mesh post = new(__mesh.__draw_mode.triangle, null, __vao.__attrib.float2);
      public static readonly __fbo frame = new(__shdr.instance.Size.X, __shdr.instance.Size.Y, true);
      public static readonly __fbo prevFrame = new(__shdr.instance.Size.X, __shdr.instance.Size.Y, true);
      public static bool rendering3d;

      static __render_system()
      {
         Array.Fill(_model, Matrix4.Identity);
      }

      public static ref Matrix4 model => ref _model[_modelIdx];

      public static Vector2i size => __shdr.instance.Size;

      public static void push()
      {
         _model[_modelIdx + 1] = model;
         _modelIdx++;
      }

      public static void pop()
      {
         _modelIdx--;
      }

      public static void set_defaults(this __shader shader)
      {
         shader.set_matrix4("_proj", _projection);
         shader.set_float("_time", (float)GLFW.GetTime());
      }

      public static void update_projection()
      {
         Matrix4.CreateOrthographicOffCenter(0, __shdr.instance.Size.X, __shdr.instance.Size.Y, 0, -1, 1, out _projection);
      }
   }
}