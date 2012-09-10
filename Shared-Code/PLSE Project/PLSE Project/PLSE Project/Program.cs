using System;

namespace PLSE_Project
{
#if WINDOWS || XBOX
    static class Program
    {
        static void Main(string[] args)
        {
            using (PhysicsPlayground game = new PhysicsPlayground())
            {
                game.Run();
            }
        }
    }
#endif
}

