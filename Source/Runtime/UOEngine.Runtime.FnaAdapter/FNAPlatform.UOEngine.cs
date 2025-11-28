using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Platform;

namespace Microsoft.Xna.Framework;

// UOEngine additions to make life easier.

public partial class FNAPlatform
{
    public static void SetupPlatform(IWindow window)
    {
        _gameWindow = new UOEGameWindow(window);
    }
}
