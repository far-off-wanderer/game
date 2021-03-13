using System;

namespace Far_Off_Wanderer
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new Startup();
            game.Run();
        }
    }
}
