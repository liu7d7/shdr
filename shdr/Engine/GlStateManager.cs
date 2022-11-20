using OpenTK.Graphics.OpenGL4;

namespace shdr.Engine
{
   public static class __gl_state_manager
   {
      private static bool _depthEnabled;
      private static bool _blendEnabled;
      private static bool _cullEnabled;

      private static bool _depthSaved;
      private static bool _blendSaved;
      private static bool _cullSaved;

      public static void save_state()
      {
         _depthSaved = _depthEnabled;
         _blendSaved = _blendEnabled;
         _cullSaved = _cullEnabled;
      }

      public static void restore_state()
      {
         if (_depthSaved)
            enable_depth();
         else
            disable_depth();
         if (_blendSaved)
            enable_blend();
         else
            disable_blend();
         if (_cullSaved)
            enable_cull();
         else
            disable_cull();
      }

      public static void enable_depth()
      {
         if (_depthEnabled) return;
         _depthEnabled = true;
         GL.Enable(EnableCap.DepthTest);
      }

      public static void disable_depth()
      {
         if (!_depthEnabled) return;
         _depthEnabled = false;
         GL.Disable(EnableCap.DepthTest);
      }

      public static void enable_blend()
      {
         if (_blendEnabled) return;
         _blendEnabled = true;
         GL.Enable(EnableCap.Blend);
         GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
      }

      public static void disable_blend()
      {
         if (_blendEnabled)
         {
            _blendEnabled = false;
            GL.Disable(EnableCap.Blend);
         }
      }

      public static void enable_cull()
      {
         if (_cullEnabled) return;
         _cullEnabled = true;
         GL.Enable(EnableCap.CullFace);
      }

      public static void disable_cull()
      {
         if (!_cullEnabled) return;
         _cullEnabled = false;
         GL.Disable(EnableCap.CullFace);
      }
   }
}