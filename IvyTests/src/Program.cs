using System;

namespace IvyTests
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (IvyTests game = new IvyTests())
            {
                game.Run();
            }
        }
    }
}

