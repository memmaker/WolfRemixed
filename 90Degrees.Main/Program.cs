using raycaster;
using System;

namespace _90Degrees.Main
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (RaycastGame game = new RaycastGame())
            {
                game.Run();
            }
        }
    }
}
