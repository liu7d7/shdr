using OpenTK.Graphics.OpenGL4;

namespace shdr.Engine
{
   public class __vbo
   {
      private static int _active;
      private readonly byte[] _bitDest = new byte[4];
      private readonly int _handle;
      private readonly MemoryStream _vertices;

      public __vbo(int initialCapacity)
      {
         _handle = GL.GenBuffer();
         _vertices = new MemoryStream(initialCapacity);
      }

      public void bind()
      {
         if (_handle == _active) return;
         GL.BindBuffer(BufferTarget.ArrayBuffer, _handle);
         _active = _handle;
      }

      public static void unbind()
      {
         GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
         _active = 0;
      }

      public void put(float element)
      {
         BitConverter.TryWriteBytes(_bitDest, element);
         _vertices.Write(_bitDest);
      }

      public void upload(bool unbindAfter = true)
      {
         if (_active != _handle) bind();
         GL.BufferData(BufferTarget.ArrayBuffer, (int)_vertices.Length, _vertices.GetBuffer(),
            BufferUsageHint.DynamicDraw);
         if (unbindAfter) unbind();
      }

      public void clear()
      {
         _vertices.SetLength(0);
      }
   }
}