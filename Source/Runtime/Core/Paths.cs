using System.Reflection.Metadata.Ecma335;

namespace UOEngine.Runtime.Core
{
    public class Paths
    {
        public static readonly string ExecutableDirectory = AppContext.BaseDirectory;

        public static string BinariesDirectory
        {
            get
            {
                var dir = Directory.GetParent(ExecutableDirectory);

                while(dir!.Name != "Binaries" )
                {
                     dir = dir.Parent;
                }

                return dir.ToString();
            }
        }

        public static string UOEngineRoot => Directory.GetParent(BinariesDirectory)!.ToString();

        public static string Intermediate => Path.Combine(UOEngineRoot, "Intermediate");

    }
}
