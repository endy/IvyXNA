using System;

namespace Blackfire
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Blackfire game = new Blackfire())
            {
                game.Run();
            }
        }
    }
#endif
}

