using NetDiscordRpc;
using NetDiscordRpc.RPC;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace shdr.Shared
{
   public static class __discord_rpc
   {
      private static DiscordRPC _client;
      private static Thread _thread;
      private static bool _running;

      public static void init()
      {
         _client = new DiscordRPC("1043932456606244896");
         _client.Initialize();

         new Thread(() =>
         {
            while (_running && __shdr.discord)
            {
               _client.SetPresence(new RichPresence
               {
                  Assets = new Assets
                  {
                     LargeImageKey = "cover-image",
                     LargeImageText = "GLSLand"
                  },
                  Details = $"Editing {__shdr.path}",
                  State = $"Line {__shdr.dat.end.lin + 1}"
               });
               Thread.Sleep(2000);
            }
         }).Start();
      }
      
      public static void shutdown()
      {
         _running = false;
         _client.Dispose();
      }
   }
}