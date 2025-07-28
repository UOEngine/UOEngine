using System.Reflection;
using ClangSharp;
using ClangSharp.Interop;

namespace BindingsGenerator
{
    public class LibraryImportGenerator
    {
        private List<string> _functions = [];
        public LibraryImportGenerator() { }

        public int Main(string[] args)
        {
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

            return 0;
        }

        private void GenerateBindingsForHeader(string file, string destination)
        {
            string[] clangArgs = new[]
{
                "-x", "c++",                // or "c" for C headers
                "-std=c++20",               // match your header
            };

            string filename = Path.GetFileNameWithoutExtension(file);

            using var index = CXIndex.Create();

            CXTranslationUnit translationUnit = CXTranslationUnit.Parse(index, file, clangArgs, Array.Empty<CXUnsavedFile>(), CXTranslationUnit_Flags.CXTranslationUnit_None);

            var managedTranslationUnit = TranslationUnit.GetOrCreate(translationUnit);

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
            if (cursor.CursorKind == CXCursorKind.CXCursor_FunctionDecl)
            {
                EmitFunction(cursor.Handle);
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
                _ => throw new NotImplementedException($"Unhandled type {type.kind}")
            };
        }

        private string MapPointer(CXType type)
        {
            CXType pointee = clang.getPointeeType(type);

            if (pointee.kind == CXTypeKind.CXType_Char_S)
            {
                return "string";
            }

            return "UIntPtr"; // raw pointer fallback
        }
    }
}
