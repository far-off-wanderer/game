namespace Far_Off_Wanderer;

using System;

public static class Program
{
    [STAThread]
    static void Main()
    {
        using Startup game = new();
        game.Run();
    }
}