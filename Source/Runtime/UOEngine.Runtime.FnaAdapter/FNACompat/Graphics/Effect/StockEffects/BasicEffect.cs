using System.Numerics;

namespace Microsoft.Xna.Framework.Graphics;
public class BasicEffect : Effect
{
    public Matrix World
    {
        get { return world; }

        set
        {
            world = value;
            dirtyFlags |= EffectDirtyFlags.World | EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
        }
    }


    /// <summary>
    /// Gets or sets the view matrix.
    /// </summary>
    public Matrix View
    {
        get { return view; }

        set
        {
            view = value;
            dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.EyePosition | EffectDirtyFlags.Fog;
        }
    }


    /// <summary>
    /// Gets or sets the projection matrix.
    /// </summary>
    public Matrix Projection
    {
        get { return projection; }

        set
        {
            projection = value;
            dirtyFlags |= EffectDirtyFlags.WorldViewProj;
        }
    }

    public bool TextureEnabled
    {
        get { return textureEnabled; }

        set
        {
            if (textureEnabled != value)
            {
                textureEnabled = value;
                dirtyFlags |= EffectDirtyFlags.ShaderIndex;
            }
        }
    }

    public Texture2D Texture
    {
        get { return textureParam.GetValueTexture2D(); }
        set { textureParam.SetValue(value); }
    }

    public bool VertexColorEnabled
    {
        get { return vertexColorEnabled; }

        set
        {
            if (vertexColorEnabled != value)
            {
                vertexColorEnabled = value;
                dirtyFlags |= EffectDirtyFlags.ShaderIndex;
            }
        }
    }

    private Matrix world = Matrix.Identity;
    private Matrix view = Matrix.Identity;
    private Matrix projection = Matrix.Identity;

    private Matrix worldView;

    private Vector3 diffuseColor = Vector3.One;
    //private Vector3 emissiveColor = Vector3.Zero;
    //private Vector3 ambientLightColor = Vector3.Zero;

    private EffectParameter textureParam = null!;
    private EffectParameter diffuseColorParam = null!;
    //private EffectParameter emissiveColorParam;
    //private EffectParameter specularColorParam;
    //private EffectParameter specularPowerParam;
    //private EffectParameter eyePositionParam;
    //private EffectParameter fogColorParam;
    private EffectParameter fogVectorParam = null!;
    //private EffectParameter worldParam;
    //private EffectParameter worldInverseTransposeParam;
    private EffectParameter worldViewProjParam = null!;
    //private EffectParameter shaderIndexParam;

    //bool lightingEnabled;
    //bool preferPerPixelLighting;
    //bool oneLight;
    bool fogEnabled = false;
    bool textureEnabled;
    bool vertexColorEnabled;

    float fogStart = 0;
    float fogEnd = 1;

    private EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

    public BasicEffect(GraphicsDevice device)
        : base(device, device.EffectRemapper.GetEffect<BasicEffect>())
    {
        CacheEffectParameters(null);

    }

    private void CacheEffectParameters(BasicEffect? cloneSource)
    {
        textureParam = Parameters["Texture"];
        diffuseColorParam = Parameters["DiffuseColor"];
        //emissiveColorParam = Parameters["EmissiveColor"];
        //specularColorParam = Parameters["SpecularColor"];
        //specularPowerParam = Parameters["SpecularPower"];
        //eyePositionParam = Parameters["EyePosition"];
        //fogColorParam = Parameters["FogColor"];
        //fogVectorParam = Parameters["FogVector"];
        //worldParam = Parameters["World"];
        //worldInverseTransposeParam = Parameters["WorldInverseTranspose"];
        worldViewProjParam = Parameters["WorldViewProj"];
        //shaderIndexParam = Parameters["ShaderIndex"];

        //light0 = new DirectionalLight(Parameters["DirLight0Direction"],
        //                              Parameters["DirLight0DiffuseColor"],
        //                              Parameters["DirLight0SpecularColor"],
        //                              (cloneSource != null) ? cloneSource.light0 : null);

        //light1 = new DirectionalLight(Parameters["DirLight1Direction"],
        //                              Parameters["DirLight1DiffuseColor"],
        //                              Parameters["DirLight1SpecularColor"],
        //                              (cloneSource != null) ? cloneSource.light1 : null);

        //light2 = new DirectionalLight(Parameters["DirLight2Direction"],
        //                              Parameters["DirLight2DiffuseColor"],
        //                              Parameters["DirLight2SpecularColor"],
        //                              (cloneSource != null) ? cloneSource.light2 : null);
    }

    protected internal override void OnApply()
    {
        // Recompute the world+view+projection matrix or fog vector?
        dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(dirtyFlags, ref world, ref view, ref projection, ref worldView, fogEnabled, fogStart, fogEnd, worldViewProjParam, fogVectorParam);

        // Todo - hard coded to white for now.
        Vector4 diffuse = new Vector4();

        diffuse.X = diffuseColor.X;
        diffuse.Y = diffuseColor.Y;
        diffuse.Z = diffuseColor.Z;
        diffuse.W = 1.0f;

        diffuseColorParam.SetValue(diffuse);

        // Missed out several things here that were in the original.
    }
}
