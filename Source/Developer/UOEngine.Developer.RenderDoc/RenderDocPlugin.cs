using UOEngine.Runtime.Core;
using UOEngine.Runtime.Plugin;

namespace UOEngine.Developer.RenderDoc;

public class RenderDocPlugin: IPlugin
{
    private RenderDoc _renderDoc;

    public void Startup() 
    {
        if(CommandLine.HasOption("-renderdoc") == false)
        {
            return;
        }

        _renderDoc = new RenderDoc();

        _renderDoc.TryLoad();
    }

}
