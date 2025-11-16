using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework;

public class GraphicsDeviceManager
{
    public bool IsFullScreen;
    public DepthFormat PreferredDepthStencilFormat;
    public readonly GraphicsDevice GraphicsDevice;
    public GraphicsProfile GraphicsProfile;

    public event EventHandler<PreparingDeviceSettingsEventArgs> PreparingDeviceSettings;

    public GraphicsDeviceManager(Game game)
    {
        GraphicsDevice = new GraphicsDevice(game.RenderResourceFactory);
    }

    public void ApplyChanges()
    {

    }
}
