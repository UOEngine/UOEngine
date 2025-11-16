using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.FnaAdapter;

public class FnaAdapterPlugin: IPlugin
{
    public static FnaAdapterPlugin Instance { get; private set; }

    // These are all public for the fake FNA classes to use what is required under the hood.
    public readonly IWindow Window;
    public readonly InputManager InputManager;

    public readonly IRenderResourceFactory RenderResourceFactory;

    public FnaAdapterPlugin(IWindow window, InputManager inputmanager, IRenderResourceFactory renderResourceFactory)
    {
        Instance = this;

        // Grab what we need here to set things up throughout fake FNA.
        Window = window;
        InputManager = inputmanager;
        RenderResourceFactory = renderResourceFactory;
    }
}
