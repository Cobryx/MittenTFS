using System;

namespace Mitten
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        static void Main(string[] args)
        {
            using (MittenGame game = new MittenGame())
            {
                game.Run();
            }
        }
    }
#endif
}

