using System;

namespace Quicksilver
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Quicksilver game = new Quicksilver())
            {
                game.Run();
            }
        }
    }
#endif
}

