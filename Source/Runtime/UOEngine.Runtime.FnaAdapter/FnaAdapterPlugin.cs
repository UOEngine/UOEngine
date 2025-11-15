using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;

namespace UOEngine.Runtime.FnaAdapter;

public class FnaAdapterPlugin: IPlugin
{
    public static FnaAdapterPlugin Instance { get; private set; }

    public readonly IWindow Window;

    public FnaAdapterPlugin(IWindow window)
    {
        Instance = this;

        Window = window;
    }
}
