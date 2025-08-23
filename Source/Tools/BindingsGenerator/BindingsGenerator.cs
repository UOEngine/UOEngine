using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using ClangSharp;
using ClangSharp.Interop;

namespace BindingsGenerator;

public class LibraryImportGenerator
{
    private List<string> _functions = [];
    public LibraryImportGenerator() { }

    private readonly string[] _modules = ["Core"];

    private string[] _includeDirectories = [];

    private readonly Dictionary<string, string> _cxTypeToUoEngineType = new()
    {
        ["int32"] = "int",
        ["uint32"] = "uint"
    };

    public int Main(string[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine("BindingsGenerator: Running...");

        string exePath = Assembly.GetExecutingAssembly().Location;
        string exeFolder = Path.GetDirectoryName(exePath);

        string sourcePath = "";

        var repoRoot = new DirectoryInfo(exePath);

        while (repoRoot != null)
        {
            sourcePath = Path.Combine(repoRoot.FullName, "Source");

            if (Directory.Exists(sourcePath))
            {
                break;
            }

            repoRoot = repoRoot.Parent;
        }

        string runtimeFolder = Path.Combine(sourcePath, "Runtime");

        _includeDirectories = [.. _includeDirectories, runtimeFolder];

        string nativeInteropPath = Path.Combine(sourcePath, @"Runtime\NativeInterop");

        if (Directory.Exists(nativeInteropPath) == false)
        {
            Console.WriteLine($"BindingsGenerator: Native Interop directory {nativeInteropPath} does not exist.");

            return -1;
        }

        string destination = Path.Combine(sourcePath, @"Game\UOEngine.Interop\Generated");

        if (Directory.Exists(destination) == false)
        {
            Directory.CreateDirectory(destination);
        }

        var files = Directory.GetFiles(nativeInteropPath, "*.h");

        foreach (var file in files)
        {
            GenerateBindingsForHeader(file, destination);

            _functions.Clear();
        }

        stopwatch.Stop();

        Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");

        return 0;
    }

    private void GenerateBindingsForHeader(string file, string destination)
    {
        string filename = Path.GetFileNameWithoutExtension(file);

        var translationFlags = CXTranslationUnit_Flags.CXTranslationUnit_None;

        translationFlags |= CXTranslationUnit_Flags.CXTranslationUnit_IncludeAttributedTypes;               // Include attributed types in CXType
        translationFlags |= CXTranslationUnit_Flags.CXTranslationUnit_VisitImplicitAttributes;              // Implicit attributes should be visited

        var managedTranslationUnit = ParseFile(file, translationFlags, _includeDirectories);

        foreach (var cursor in managedTranslationUnit.TranslationUnitDecl.CursorChildren)
        {
            Visit(cursor);
        }

        var builder = new CSharpOutputBuilder(this);

        builder.WriteLine("using System.Runtime.InteropServices;");

        builder.WriteLine("");

        builder.WriteLine("namespace UOEngine.Interop");
        builder.WriteBlockStart();

        builder.WriteIndentedLine($"public static partial class {filename}");
        builder.WriteBlockStart();

        foreach (var line in _functions)
        {
            builder.WriteIndentedLine("[LibraryImport(\"UOEngine.Native.dll\", StringMarshalling = StringMarshalling.Utf8)]");
            builder.WriteIndented(line);
            builder.Write(";");
            builder.WriteNewLine();
            builder.WriteNewLine();
        }

        builder.WriteBlockEnd();
        builder.WriteBlockEnd();

        var csharpFileContents = builder.ToString();

        string csharpFile = $"{destination}/{filename}.cs";

        if(File.Exists(csharpFile))
        {
            string existingContents = File.ReadAllText(csharpFile);

            existingContents.Replace("\r\n", "\n").Replace("\r", "\n");

            if(existingContents == csharpFileContents)
            {
                // Do not change if the same, otherwise timestamp will change and build system may see it as changed
                // and do rebuilds.
                return;
            }
        }

        File.WriteAllText(csharpFile, csharpFileContents);
    }

    private unsafe void Visit(Cursor cursor)
    {
        switch(cursor.CursorKind)
        {
            case CXCursorKind.CXCursor_MacroDefinition:
                {
                    var macroName = cursor.Spelling.ToString();

                    Debug.Print($"macro name {macroName}");
                    break;
                }

            case CXCursorKind.CXCursor_FunctionDecl:
                {
                    EmitFunction(cursor.Handle);
                    break;
                }

            case CXCursorKind.CXCursor_InclusionDirective:
                {
                    break;
                }

            case CXCursorKind.CXCursor_StructDecl:
                {
                    break;
                }

            case CXCursorKind.CXCursor_ClassTemplate:
                {
                    break;
                }

            default:
                break;
        }

        foreach (var child in cursor.CursorChildren)
        {
            Visit(child);
        }
    }

    private void EmitFunction(CXCursor func)
    {
        var name = func.Spelling;
        CXType returnType = func.ResultType;

        string returnTypeName = GetCSharpType(returnType);

        int paramCount = clang.getNumArgTypes(func.Type);

        List<string> paramDecls = new();

        for (int i = 0; i < paramCount; i++)
        {
            CXType paramType = clang.getArgType(func.Type, (uint)i);
            string paramTypeName = GetCSharpType(paramType);

            var paramName = func.GetArgument((uint)i).Spelling.ToString();

            if (string.IsNullOrEmpty(paramName))
                paramName = $"param{i}";

            paramDecls.Add($"{paramTypeName} {paramName}");
        }

        string paramList = string.Join(", ", paramDecls);

        string function = $"public static partial {returnTypeName} {name}({paramList})";

        _functions.Add(function);
    }

    private string GetCSharpType(CXType type)
    {
        return type.kind switch
        {
            CXTypeKind.CXType_Int => "int",
            CXTypeKind.CXType_Void => "void",
            CXTypeKind.CXType_Pointer => MapPointer(type),
            CXTypeKind.CXType_Char_S => "sbyte",
            CXTypeKind.CXType_UChar => "byte",
            CXTypeKind.CXType_UInt => "uint",
            CXTypeKind.CXType_Float => "float",
            CXTypeKind.CXType_Double => "double",
            CXTypeKind.CXType_Elaborated => MapElaborated(type),
            _ => throw new NotImplementedException($"Unhandled type {type.kind}")
        };
    }

    private string MapPointer(CXType type)
    {
        CXType pointee = clang.getPointeeType(type);

        switch(pointee.kind)
        {
            case CXTypeKind.CXType_Char_S:
                {
                    return "string";
                }

            //case CXTypeKind.CXType_Float:
            //    {
            //        return "float*";
            //    }

            default:
                break;
        }

        return "UIntPtr"; // raw pointer fallback
    }

    private string MapElaborated(CXType type)
    {
        if(_cxTypeToUoEngineType.TryGetValue(type.ToString(), out string value))
        {
            return value;
        }

        return type.ToString();
    }

    TranslationUnit ParseFile(string file, CXTranslationUnit_Flags flags, string[]? headers = null)
    {
        string[] clangCommandLineArgs =
        {
            "-x", "c++",                
            "-std=c++20",            
            "-Wno-pragma-once-outside-header"       // We are processing files which may be header files
        };

        clangCommandLineArgs = [.. clangCommandLineArgs, .. headers?.Select(x => $"-I{x}") ?? []];

        using var index = CXIndex.Create();

        var translationUnitError = CXTranslationUnit.TryParse(index, file, clangCommandLineArgs, [], flags, out var handle);

        if (translationUnitError != CXErrorCode.CXError_Success)
        {
            Console.WriteLine($"Error: Parsing failed for '{file}' due to '{translationUnitError}'.");

            throw new Exception("Parsing failed");
        }
        else if (handle.NumDiagnostics != 0)
        {
            Console.WriteLine($"Diagnostics for '{file}':");

            for (uint i = 0; i < handle.NumDiagnostics; ++i)
            {
                using var diagnostic = handle.GetDiagnostic(i);

                Console.Write("    ");
                Console.WriteLine(diagnostic.Format(CXDiagnostic.DefaultDisplayOptions).ToString());
            }

            throw new Exception("Diagnostic issues detected. Check log.");
        }

        return TranslationUnit.GetOrCreate(handle);
    }
}
