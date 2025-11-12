using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework;

public class GraphicsDeviceManager
{
    public bool IsFullScreen;
    public DepthFormat PreferredDepthStencilFormat;
    public GraphicsDevice GraphicsDevice;
    public GraphicsProfile GraphicsProfile;

    public event EventHandler<PreparingDeviceSettingsEventArgs> PreparingDeviceSettings;

    public GraphicsDeviceManager(Game game)
    {
        throw new NotImplementedException();
    }

    public void ApplyChanges()
    {
        throw new NotImplementedException();
    }
}
