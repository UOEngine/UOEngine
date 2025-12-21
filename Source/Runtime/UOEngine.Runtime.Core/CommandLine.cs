namespace UOEngine.Runtime.Core;

public static class CommandLine
{
    private static string[] _parsedArgs;

    public static bool HasOption(string name)
    {
        return _parsedArgs.Any(a => a == name);
    }

    static CommandLine()
    {
        var args = Environment.GetCommandLineArgs();

        _parsedArgs = new string[args.Length];

        for(int i = 0; i < args.Length; i++)
        {
            _parsedArgs[i] = args[i];
        }
    }
}
