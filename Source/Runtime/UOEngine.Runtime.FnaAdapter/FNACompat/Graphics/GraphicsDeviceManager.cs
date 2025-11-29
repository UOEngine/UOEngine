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
        game.Services.AddService(typeof(GraphicsDeviceManager), this);

        GraphicsDevice = new GraphicsDevice(game.ServiceProvider);
    }

    public void ApplyChanges()
    {

    }
}
