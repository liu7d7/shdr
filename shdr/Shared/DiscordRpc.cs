using NetDiscordRpc;
using NetDiscordRpc.RPC;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace shdr.Shared
{
   public static class __discord_rpc
   {
      private static DiscordRPC _client;

      public static void init()
      {
         _client = new DiscordRPC("1043932456606244896");
         _client.Initialize();

         new Thread(() =>
         {
            unsafe
            {
               while (!GLFW.WindowShouldClose(__shdr.instance.WindowPtr))
               {
                  _client.SetPresence(new RichPresence
                  {
                     Assets = new Assets
                     {
                        LargeImageKey = "cover-image",
                        LargeImageText = "GLSLand"
                     },
                     Details = $"Editing {__shdr.path}",
                     State = $"Line {__shdr.dat.end.lin}"
                  });
                  Thread.Sleep(2000);
               }
               _client.Dispose();
            }
         }).Start();
      }
   }
}