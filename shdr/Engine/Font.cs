using OpenTK.Graphics.OpenGL4;
using shdr.Shared;
using StbTrueTypeSharp;

namespace shdr.Engine
{
   public static class __font
   {
      private const float ipw = 1.0f / 2048f;
      private const float iph = ipw;
      private static readonly float[] _ascent;
      private static readonly StbTrueType.stbtt_packedchar[][] _chars;

      private static readonly __mesh _mesh;
      private static readonly __texture[] _texture;
      
      private static readonly List<KeyValuePair<string, int>> _paths = new()
      {
         new KeyValuePair<string, int>("Resource/Font/Dank Mono Italic.otf", 18),
         new KeyValuePair<string, int>("Resource/Font/JetBrainsMono-Regular.ttf", 20),
         new KeyValuePair<string, int>("Resource/Font/monofur-regular.ttf", 19)
      };


      private static int _index;
      public static int index
      {
         set => _index = value % _paths.Count;
         get => _index;
      }

      // I have absolutely no idea how to use unsafe :((
      static unsafe __font()
      {
         _mesh = new __mesh(
            __mesh.__draw_mode.triangle,
            new __shader("Resource/Shader/font.vert", "Resource/Shader/font.frag"),
            __vao.__attrib.float3, __vao.__attrib.float2, __vao.__attrib.float4
         );

         _paths.RemoveAll(it => !File.Exists(it.Key));

         foreach (string f in Directory.GetFiles("Resource\\Font"))
         {
            if (_paths.Any(p => p.Key.Replace("/", "\\") == f)) continue;
            if (!f.EndsWith(".ttf") && !f.EndsWith(".otf")) continue;
            int size = File.Exists($"{f}-size.txt") ? int.Parse(File.ReadAllLines($"{f}-size.txt")[0]) : 20;
            _paths.Add(new KeyValuePair<string, int>(f, size));
            Console.WriteLine($"Loaded font: {f}@{size}px");
         }
         
         _chars = new StbTrueType.stbtt_packedchar[_paths.Count][];
         _ascent = new float[_paths.Count];
         _texture = new __texture[_paths.Count];

         for (int i = 0; i < _paths.Count; i++)
         {
            int height = _paths[i].Value;
            _chars[i] = new StbTrueType.stbtt_packedchar[256];
            byte[] buffer = File.ReadAllBytes(_paths[i].Key);
            StbTrueType.stbtt_fontinfo fontInfo = StbTrueType.CreateFont(buffer, 0);
            
            StbTrueType.stbtt_pack_context packContext = new();

            byte[] bitmap = new byte[2048 * 2048];
            fixed (byte* dat = bitmap)
            {
               StbTrueType.stbtt_PackBegin(packContext, dat, 2048, 2048, 0, 1, null);
            }

            StbTrueType.stbtt_PackSetOversampling(packContext, 8, 8);
            fixed (byte* dat = buffer)
            {
               fixed (StbTrueType.stbtt_packedchar* c = _chars[i])
               {
                  StbTrueType.stbtt_PackFontRange(packContext, dat, 0, height, 32, 256, c);
               }
            }

            StbTrueType.stbtt_PackEnd(packContext);

            int asc;
            StbTrueType.stbtt_GetFontVMetrics(fontInfo, &asc, null, null);
            _ascent[i] = asc * StbTrueType.stbtt_ScaleForPixelHeight(fontInfo, height);

            _texture[i] = __texture.load_from_buffer(bitmap, 2048, 2048, PixelFormat.Red, PixelInternalFormat.R8,
               TextureMinFilter.Nearest, TextureMagFilter.Nearest);
         }
      }

      public static void bind()
      {
         _texture[index].bind(TextureUnit.Texture0);
         _mesh.begin();
      }

      public static void render()
      {
         _mesh.render();
         __texture.unbind();
      }

      public static void draw(string text, float x, float y, uint color, bool shadow, float scale = 1.0f)
      {
         int length = text.Length;
         float drawX = x;
         float drawY = y + _ascent[index] * scale;
         float alpha = ((color >> 24) & 0xFF) / 255.0f;
         float red = ((color >> 16) & 0xFF) / 255.0f;
         float green = ((color >> 8) & 0xFF) / 255.0f;
         float blue = (color & 0xFF) / 255.0f;
         for (int i = 0; i < length; i++)
         {
            char charCode = text[i];
            char previous = i > 0 ? text[i - 1] : ' ';
            if (previous == '\u00a7') continue;

            if (charCode == '\u00a7' && i < length - 1)
            {
               char next = text[i + 1];
               if (__fmt.values.TryGetValue(next, out __fmt fmt))
               {
                  uint newColor = fmt.color;
                  red = ((newColor >> 16) & 0xFF) / 255.0f;
                  green = ((newColor >> 8) & 0xFF) / 255.0f;
                  blue = (newColor & 0xFF) / 255.0f;
               }

               continue;
            }

            if (charCode < 32 || charCode > 32 + 256) charCode = ' ';

            StbTrueType.stbtt_packedchar c = _chars[index][charCode - 32];

            float dxs = drawX + c.xoff * scale;
            float dys = drawY + c.yoff * scale;
            float dx1S = drawX + c.xoff2 * scale;
            float dy1S = drawY + c.yoff2 * scale;
            
            if (shadow)
            {
               int j1 = _mesh.float3(dxs + scale * 1.5f, dys + scale * 1.5f, 1).float2(c.x0 * ipw, c.y0 * iph)
                  .float4(red * 0, green * 0, blue * 0, alpha).next();
               int j2 = _mesh.float3(dxs + scale * 1.5f, dy1S + scale * 1.5f, 1).float2(c.x0 * ipw, c.y1 * iph)
                  .float4(red * 0, green * 0, blue * 0, alpha).next();
               int j3 = _mesh.float3(dx1S + scale * 1.5f, dy1S + scale * 1.5f, 1).float2(c.x1 * ipw, c.y1 * iph)
                  .float4(red * 0, green * 0, blue * 0, alpha).next();
               int j4 = _mesh.float3(dx1S + scale * 1.5f, dys + scale * 1.5f, 1).float2(c.x1 * ipw, c.y0 * iph)
                  .float4(red * 0, green * 0, blue * 0, alpha).next();
               _mesh.quad(j1, j2, j3, j4);
            }

            int k1 = _mesh.float3(dxs, dys, 0).float2(c.x0 * ipw, c.y0 * iph)
               .float4(red, green, blue, alpha).next();
            int k2 = _mesh.float3(dxs, dy1S, 0).float2(c.x0 * ipw, c.y1 * iph)
               .float4(red, green, blue, alpha).next();
            int k3 = _mesh.float3(dx1S, dy1S, 0).float2(c.x1 * ipw, c.y1 * iph)
               .float4(red, green, blue, alpha).next();
            int k4 = _mesh.float3(dx1S, dys, 0).float2(c.x1 * ipw, c.y0 * iph)
               .float4(red, green, blue, alpha).next();
            _mesh.quad(k1, k2, k3, k4);

            drawX += c.xadvance * scale;
            drawX -= 0.4f * scale;
         }
      }

      public static float get_width(string text, float scale = 1.0f)
      {
         int length = text.Length;
         float width = 0;
         for (int i = 0; i < length; i++)
         {
            char charCode = text[i];
            char previous = i > 0 ? text[i - 1] : ' ';
            if (previous == '\u00a7') continue;

            if (charCode < 32 || charCode > 32 + 256) charCode = ' ';

            StbTrueType.stbtt_packedchar c = _chars[index][charCode - 32];

            width += c.xadvance * scale;
            width -= 0.4f * scale;
         }

         width += 0.4f * scale;

         return width;
      }

      public static float get_height(float scale = 1.0f)
      {
         return _ascent[index] * scale;
      }
   }
}