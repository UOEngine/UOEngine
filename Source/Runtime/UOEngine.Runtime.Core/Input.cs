using UOEngine.Runtime.Core.Input;

namespace UOEngine.Runtime.Core;

public class InputManager
{
    public Mouse Mouse { get; private set; }

    public Keyboard Keyboard { get; private set; }

    public InputManager(PlatformEventLoop platformEventLoop, ApplicationLoop applicationLoop)
    {
        Mouse = new Mouse(platformEventLoop, applicationLoop);
        Keyboard = new Keyboard(platformEventLoop, applicationLoop);
    }
}
