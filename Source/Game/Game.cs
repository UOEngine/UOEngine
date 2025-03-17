using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UOEngine
{
    public class Game
    {
        [UnmanagedCallersOnly]
        static public int NativeInitialise()
        {
            game = new Game();

            return game.Initalise()? 1: 0;
        }

        virtual public bool Initalise()
        {
            Debug.WriteLine($"Game.Initialise: Start");

            return true;
        }

        public void Update(float tick)
        {
            Console.WriteLine($"Tick {tick}");
        }

        [UnmanagedCallersOnly]
        static public void NativeUpdate(float tick)
        {
            game.Update(tick);
        }

        private static Game game = null;
    }
}
