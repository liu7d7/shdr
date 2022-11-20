using shdr.Engine;

namespace shdr.Shared
{
   public static class __util
   {
      public static readonly float sqrt2 = MathF.Sqrt(2);

      public static float lerp(float start, float end, float delta)
      {
         return start + (end - start) * delta;
      }

      public static void clamp(ref int val, int start, int end)
      {
         val = Math.Min(Math.Max(val, start), end);
      }

      public static void draw_rect(float x, float y, float width, float height, uint color)
      {
         __render_system.basic.begin();
         __render_system.basic.quad(
            __render_system.basic.float3(x, y, 0).float4(color).next(),
            __render_system.basic.float3(x + width, y, 0).float4(color).next(),
            __render_system.basic.float3(x + width, y + height, 0).float4(color).next(),
            __render_system.basic.float3(x, y + height, 0).float4(color).next()
         );
         __render_system.basic.render();
      }
   }
}