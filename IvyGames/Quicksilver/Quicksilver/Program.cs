using System;

namespace Quicksilver
{
#if WINDOWS || XBOX || PSS
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            using (Quicksilver game = new Quicksilver())
            {
                game.Run();
            }
        }
    }
#endif
}

