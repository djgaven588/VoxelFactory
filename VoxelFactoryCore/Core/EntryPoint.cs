using System;

namespace VoxelFactoryCore
{
    public class EntryPoint
    {
        static void Main(string[] args)
        {
            WindowManager manager = new WindowManager();
            manager.Run();
            Console.WriteLine("The game has exited.");
        }
    }
}
