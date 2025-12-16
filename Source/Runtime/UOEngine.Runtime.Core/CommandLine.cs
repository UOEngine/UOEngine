namespace UOEngine.Runtime.Core;

public static class CommandLine
{
    private static string[] _passedArgs;

    public static bool HasOption(string name)
    {
        return _passedArgs.Any(a => a == name);
    }

    static CommandLine()
    {
        var args = Environment.GetCommandLineArgs();

        _passedArgs = new string[args.Length];

        for(int i = 0; i < args.Length; i++)
        {
            _passedArgs[i] = args[i];
        }
    }
}
