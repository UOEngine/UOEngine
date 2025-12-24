namespace Microsoft.Xna.Framework;

public class PreparingDeviceSettingsEventArgs: EventArgs
{
    public GraphicsDeviceInformation GraphicsDeviceInformation { get; private set; }

    public PreparingDeviceSettingsEventArgs(GraphicsDeviceInformation graphicsDeviceInformation)
    {
        GraphicsDeviceInformation = graphicsDeviceInformation;
    }
}
