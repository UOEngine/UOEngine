using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BindingsGenerator
{
    internal class CSharpOutputBuilder(LibraryImportGenerator generator)
    {
        public const string                         DefaultIndentationString = "\t";
        public const string                         LineEnding = "\n";

        public IEnumerable<string>                  Contents => _contents;

        private readonly LibraryImportGenerator     _generator = generator;
        private readonly StringBuilder              _currentLine = new();
        private readonly List<string>               _contents = [];

        private int                                 _indentionLevel = 0;

        public override string ToString()
        {
            var result = new StringBuilder();

            foreach (var line in _contents)
            {
                _ = result.Append(line);
                _ = result.Append(LineEnding);
            }

            _ = result.Append(_currentLine);

            return result.ToString();
        }

        public void Write<T>(T value) => _currentLine.Append(value);

        public void WriteLine<T>(T value)
        {
            Write(value);
            WriteNewLine();
        }

        public void WriteIndentedLine<T>(T value)
        {
            WriteIndentation();
            WriteLine(value);
        }

        public void WriteNewLine()
        {
            _contents.Add(_currentLine.ToString());
            _currentLine.Clear();
        }

        public void WriteBlockStart()
        {
            WriteIndentedLine("{");
            IncreaseIndentation();
        }

        public void WriteBlockEnd()
        {
            DecreaseIndentation();
            WriteIndentedLine("}");
        }

        public void WriteIndented<T>(T value)
        {
            WriteIndentation();
            Write(value);
        }

        private void WriteIndentation()
        {
            for(int i = 0; i < _indentionLevel; i++)
            {
                _currentLine.Append(DefaultIndentationString);
            }
        }

        private void IncreaseIndentation()
        {
            _indentionLevel++;

        }

        private void DecreaseIndentation()
        {
            _indentionLevel--;

            Debug.Assert(_indentionLevel >= 0);

        }
    }
}
