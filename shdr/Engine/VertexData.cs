using OpenTK.Mathematics;

namespace shdr.Engine
{
   public class __vertex_data
   {
      public uint color = 0xffffffff;
      public Vector3 normal;
      public Vector3 pos;
      public Vector2 uv;

      public __vertex_data(Vector3 pos, Vector2 uv)
      {
         this.pos = pos;
         this.uv = uv;
      }
   }
}