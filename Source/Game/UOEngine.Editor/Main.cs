﻿namespace UOEngine
{
    internal static class MainProgram
    {
        [STAThread]
        static int Main(string[] args)
        {
            using Engine engine = new Engine(new UOEngine.Editor.Editor());

            return engine.Run(args);
        }
    }
}
