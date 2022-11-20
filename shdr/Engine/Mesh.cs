using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using shdr.Shared;

namespace shdr.Engine
{
   public class __mesh
   {
      private readonly __draw_mode _drawMode;
      private readonly __ibo _ibo;
      private readonly __shader _shader;

      private readonly __vao _vao;
      private readonly __vbo _vbo;
      private bool _building;
      private int _index;
      private int _vertex;

      public __mesh(__draw_mode drawMode, __shader shader, params __vao.__attrib[] attribs)
      {
         _drawMode = drawMode;
         _shader = shader;
         int stride = attribs.Sum(attrib => (int)attrib * sizeof(float));
         _vbo = new __vbo(stride * drawMode.size * 256 * sizeof(float));
         _vbo.bind();
         _ibo = new __ibo(drawMode.size * 512 * sizeof(float));
         _ibo.bind();
         _vao = new __vao(attribs);
         __vbo.unbind();
         __ibo.unbind();
         __vao.unbind();
      }

      public int next()
      {
         return _vertex++;
      }

      public __mesh float1(float p0)
      {
         _vbo.put(p0);
         return this;
      }

      public __mesh float2(float p0, float p1)
      {
         _vbo.put(p0);
         _vbo.put(p1);
         return this;
      }

      public __mesh float2(Vector2 p0)
      {
         _vbo.put(p0.X);
         _vbo.put(p0.Y);
         return this;
      }

      public __mesh float3(float p0, float p1, float p2)
      {
         _vbo.put(p0);
         _vbo.put(p1);
         _vbo.put(p2);
         return this;
      }

      public __mesh float3(Matrix4 transform, float p0, float p1, float p2)
      {
         Vector4 pos = new(p0, p1, p2, 1);
         pos.transform(transform);
         _vbo.put(pos.X);
         _vbo.put(pos.Y);
         _vbo.put(pos.Z);
         return this;
      }

      public __mesh float3(Vector3 p0)
      {
         _vbo.put(p0.X);
         _vbo.put(p0.Y);
         _vbo.put(p0.Z);
         return this;
      }

      public __mesh float4(float p0, float p1, float p2, float p3)
      {
         _vbo.put(p0);
         _vbo.put(p1);
         _vbo.put(p2);
         _vbo.put(p3);
         return this;
      }

      public __mesh float4(uint color)
      {
         return float4(((color >> 16) & 0xff) * 0.003921569f, ((color >> 8) & 0xff) * 0.003921569f,
            (color & 0xff) * 0.003921569f, ((color >> 24) & 0xff) * 0.003921569f);
      }

      public __mesh float4(uint color, float alpha)
      {
         return float4(((color >> 16) & 0xff) * 0.003921569f, ((color >> 8) & 0xff) * 0.003921569f,
            (color & 0xff) * 0.003921569f, alpha);
      }

      public void line(int p0, int p1)
      {
         _ibo.put(p0);
         _ibo.put(p1);
         _index += 2;
      }

      public void tri(int p0, int p1, int p2)
      {
         _ibo.put(p0);
         _ibo.put(p1);
         _ibo.put(p2);
         _index += 3;
      }

      public void quad(int p0, int p1, int p2, int p3)
      {
         _ibo.put(p0);
         _ibo.put(p1);
         _ibo.put(p2);
         _ibo.put(p2);
         _ibo.put(p3);
         _ibo.put(p0);
         _index += 6;
      }

      public void begin()
      {
         if (_building) throw new Exception("Already building");
         _vbo.clear();
         _ibo.clear();
         _vertex = 0;
         _index = 0;
         _building = true;
      }

      public void end()
      {
         if (!_building) throw new Exception("Not building");

         if (_index > 0)
         {
            _vbo.upload();
            _ibo.upload();
         }

         _building = false;
      }

      public void render()
      {
         if (_building) end();

         if (_index <= 0) return;
         __gl_state_manager.save_state();
         __gl_state_manager.enable_blend();
         if (__render_system.rendering3d)
            __gl_state_manager.enable_depth();
         else
            __gl_state_manager.disable_depth();
         _shader?.bind();
         _shader?.set_defaults();
         _vao.bind();
         _ibo.bind();
         _vbo.bind();
         GL.DrawElements(_drawMode.as_gl(), _index, DrawElementsType.UnsignedInt, 0);
         __ibo.unbind();
         __vbo.unbind();
         __vao.unbind();
         __gl_state_manager.restore_state();
      }

      public sealed class __draw_mode
      {
         private static int _cidCounter;

         public static readonly __draw_mode line = new(2);
         public static readonly __draw_mode triangle = new(3);
         public static readonly __draw_mode triangleFan = new(3);
         private readonly int _cid;
         public readonly int size;

         private __draw_mode(int size)
         {
            this.size = size;
            _cid = _cidCounter++;
         }

         public override bool Equals(object obj)
         {
            if (obj is __draw_mode mode) return _cid == mode._cid;

            return false;
         }

         public override int GetHashCode()
         {
            return _cid;
         }

         public BeginMode as_gl()
         {
            return _cid switch
            {
               0 => BeginMode.Lines,
               1 => BeginMode.Triangles,
               2 => BeginMode.TriangleFan,
               _ => throw new Exception("wtf is going on?")
            };
         }
      }
   }
}