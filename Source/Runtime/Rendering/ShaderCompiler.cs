using System.Diagnostics;

using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Rendering
{
    internal class ShaderCompiler
    {
        public ShaderCompiler() 
        {
            if (File.Exists(Compiler) == false)
            {
                Debug.Assert(false);
            }
        }

        public void Compile(string destination)
        {
            if (Directory.Exists(destination) == false)
            {
                Directory.CreateDirectory(destination);
            }

            var shaderFiles = Directory.GetFiles(ShaderSource, "*.*", SearchOption.AllDirectories);

            var glslscStartInfo = new ProcessStartInfo(Compiler);

            foreach (var shaderFile in shaderFiles)
            {
                FileInfo shaderFileInfo = new FileInfo(shaderFile);

                if((shaderFile.EndsWith(".vert") == false) && (shaderFile.EndsWith(".frag") == false))
                {
                    continue;
                }

                var args = $"{shaderFileInfo.FullName} -o {destination}/{shaderFileInfo.Name}.spv";

                var process = Process.Start(Compiler, args);
                
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"glslc.exe returned {process.ExitCode}");
                }
            }
        }

        public readonly string  Compiler = Environment.GetEnvironmentVariable("VULKAN_SDK") + "/bin/glslc.exe";
        public readonly string  ShaderSource = Paths.UOEngineRoot + "/Shaders";
    }
}
