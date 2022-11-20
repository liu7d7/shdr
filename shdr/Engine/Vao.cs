using OpenTK.Graphics.OpenGL4;

namespace shdr.Engine
{
   public class __vao
   {
      public enum __attrib
      {
         float1 = 1,
         float2 = 2,
         float3 = 3,
         float4 = 4
      }

      private static int _active;
      private readonly int _handle;

      public __vao(params __attrib[] attribs)
      {
         _handle = GL.GenVertexArray();
         bind();
         int stride = attribs.Sum(attrib => (int)attrib);
         int offset = 0;
         for (int i = 0; i < attribs.Length; i++)
         {
            GL.EnableVertexAttribArray(i);
            GL.VertexAttribPointer(i, (int)attribs[i], VertexAttribPointerType.Float, false, stride * sizeof(float),
               offset);
            offset += (int)attribs[i] * sizeof(float);
         }

         unbind();
      }

      public void bind()
      {
         if (_handle == _active) return;
         GL.BindVertexArray(_handle);
         _active = _handle;
      }

      public static void unbind()
      {
         GL.BindVertexArray(0);
         _active = 0;
      }
   }
}