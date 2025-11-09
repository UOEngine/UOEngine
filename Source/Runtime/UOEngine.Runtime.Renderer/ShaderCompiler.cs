using System.Diagnostics;

namespace UOEngine.Runtime.Renderer;

public static class ShaderCompiler
{
    public static bool Compile(string file, string destination)
    {
        if (File.Exists(file) == false)
        {
            return false;
        }

        string filename = Path.GetFileName(file);

        string fxc = "C:\\Program Files (x86)\\Windows Kits\\10\\bin\\10.0.26100.0\\x64\fxc.exe";

        string output = Path.Combine(destination, filename);

        output = Path.ChangeExtension(output, ".fxo");

        var startInfo = new ProcessStartInfo
        {
            FileName = fxc,
            Arguments = $"/T fx_2_0 /Fo Minimal.fxo {filename}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var p = Process.Start(startInfo);

        p.WaitForExit();

        return true;
    }
}
