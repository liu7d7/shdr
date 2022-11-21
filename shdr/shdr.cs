using System.Drawing;
using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using shdr.Engine;
using shdr.Shared;
using Boolean = System.Boolean;

namespace shdr
{
   public class __shdr : GameWindow
   {
      public static __shdr instance;
      public static __text_box.__data dat;
      public static __text_box.__data err;
      public static string path;
      private static __mesh _mesh;
      public static bool discord;

      public __shdr(GameWindowSettings windowSettings, NativeWindowSettings nativeWindowSettings) : base(
         windowSettings, nativeWindowSettings)
      {
         string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         Directory.SetCurrentDirectory(exeDir);
         Console.WriteLine($"Set working directory to {exeDir}");
         
         instance = this;
         {
            path = Console.ReadLine();
            if (path.Trim().Length == 0)
            {
               int i = 0;
               while (File.Exists($"shader{i}.frag"))
               {
                  i++;
               }

               path = $"shader{i}.frag";
            }
         }
         List<string> defaultText = File.ReadLines("Resource/Shader/user.frag").ToList();
         dat = new __text_box.__data
         {
            text = File.Exists(path) ? File.ReadAllLines(path).ToList() : defaultText,
            typing = true
         };
         __text_box.update(ref dat, true);
         string str = string.Join("\n", dat.text);
         File.WriteAllText(path, str);
         
         err = new __text_box.__data
         {
            text = new List<string>(),
         };

         reload();
         _mesh = new __mesh(__mesh.__draw_mode.triangle, null, __vao.__attrib.float2);
         _mesh.begin();
         _mesh.quad(
            _mesh.float2(-1, -1).next(),
            _mesh.float2(1, -1).next(),
            _mesh.float2(1, 1).next(),
            _mesh.float2(-1, 1).next()
         );
         _mesh.end();
         
         string config = File.Exists("config.txt") ? File.ReadAllText("config.txt") : "1|true";

         string[] split = config.Split('|');
         __font.index = int.Parse(split[0]);
         discord = bool.Parse(split[1]);
         if (discord)
            __discord_rpc.init();

         Closing += _ =>
         {
            File.WriteAllText($"{Directory.GetCurrentDirectory()}/config.txt", $"{__font.index}|{discord}");
            __discord_rpc.shutdown();
         };
      }

      public static void reload()
      {
         string str = string.Join("\n", dat.text);
         File.WriteAllText(__shdr.path, str);
         try
         {
            __render_system.user = new __shader("Resource/Shader/user.vert", __shdr.path);
            __shdr.err.text.Clear();
            __shdr.err.text.Add($"{__fmt.gray}no {__fmt.gray}errors...{__fmt.reset}");
            __text_box.update(ref __shdr.err, true);
         }
         catch (Exception e)
         {
            __render_system.user = new __shader("Resource/Shader/user.vert", "Resource/Shader/user.frag");
            __shdr.err.text = e.Message.Split("\n").ToList();
            __text_box.update(ref __shdr.err, true);
            Console.WriteLine(e.Message);
         }
      }

      protected override void OnLoad()
      {
         base.OnLoad();

         GL.ClearColor(0f, 0f, 0f, 0f);
         GL.DepthFunc(DepthFunction.Lequal);
         __gl_state_manager.enable_blend();
      }

      protected override void OnResize(ResizeEventArgs e)
      {
         base.OnResize(e);

         if (e.Size == Vector2i.Zero)
            return;

         GL.ClearColor(0f, 0f, 0f, 0f);
         __render_system.update_projection();
         GL.Viewport(new Rectangle(0, 0, Size.X, Size.Y));
         __fbo.resize(Size.X, Size.Y);
      }

      protected override void OnMouseWheel(MouseWheelEventArgs e)
      {
         base.OnMouseWheel(e);
         
         __text_box.wheel(ref dat, e.OffsetY);
      }

      protected override void OnTextInput(TextInputEventArgs e)
      {
         base.OnTextInput(e);
         
         __text_box.type(ref dat, e.AsString);
      }
      
      private bool _renderTextBox = true;

      protected override void OnKeyDown(KeyboardKeyEventArgs e)
      {
         base.OnKeyDown(e);

         if (e.Key == Keys.F1)
         {
            _renderTextBox = !_renderTextBox;
         }
         else if (e.Key == Keys.F2)
         {
            __font.index++;
         }
         else if (e.Key == Keys.F10)
         {
            discord = !discord;
            if (discord)
            {
               __discord_rpc.init();
            }
            else
            {
               __discord_rpc.shutdown();
            }
         }
         
         __text_box.key(ref dat, e.Key, KeyboardState.IsKeyDown(Keys.LeftShift), KeyboardState.IsKeyDown(Keys.LeftControl));
      }

      protected override void OnRenderFrame(FrameEventArgs args)
      {
         base.OnRenderFrame(args);

         __render_system.frame.bind();
         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

         __render_system.user.bind();
         __render_system.prevFrame.bind_color(TextureUnit.Texture15);
         __render_system.user.set_int("_prevFrame", 15);
         __render_system.user.set_vector2("_resolution", Size.ToVector2());
         __render_system.user.set_float("_time", (float)GLFW.GetTime());
         _mesh.render();
         
         __render_system.frame.blit(__render_system.prevFrame.handle);
         
         __render_system.frame.bind();

         if (_renderTextBox)
         {
            __text_box.render(ref dat, 10, 10, Size.X - 20, Size.Y - 160);
            __text_box.render(ref err, 10, Size.Y - 140, Size.X - 20, 130);
         }
         
         __render_system.frame.blit();

         SwapBuffers();
      }
   }
}