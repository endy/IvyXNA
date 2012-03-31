using System;

using Ivy;

namespace Ceres
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Ceres game = new Ceres())
            {
                game.Run();
            }
        }
    }
}

