using System.Text;
using System.Text.RegularExpressions;

namespace FSC.Beauty.Compile
{
    public class Compiler
    {
        private List<string> _code = new List<string>();
        private List<string> _compiledCode = new List<string>();

        public void Compile(string inputCode, out string outputCode)
        {
            outputCode = string.Empty;

            if (string.IsNullOrWhiteSpace(inputCode))
            {
                throw new Exception("Input code may not be empty");
            }

            // Loop to put inputCode into _code
            foreach (string line in inputCode.Replace("\r", "").Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                _code.Add(line);
            }

            _code = _code.Select(x => x.ReplaceLineEndings("")).ToList();

            foreach (string codeLine in _code)
            {
                if (codeLine == string.Empty)
                {
                    continue;
                }

                if (IsVariable(codeLine))
                {
                    TranslateVariable(codeLine);
                    continue;
                }

                if (IsArray(codeLine))
                {
                    TranslateArray(codeLine);
                    continue;
                }
            }

            outputCode = string.Join(Environment.NewLine, _compiledCode);
        }

        private void RemoveComments()
        {
            bool inMultiLineComment = false;
            bool inString = false;
            List<string> newCodeLines = new List<string>();

            foreach (string line in _code)
            {
                StringBuilder newLine = new StringBuilder();
                for (int i = 0; i < line.Length; i++)
                {
                    if (inMultiLineComment)
                    {
                        if (i < line.Length - 1 && line[i] == '*' && line[i + 1] == '/')
                        {
                            inMultiLineComment = false;
                            i++; // Skip the '/' character
                        }
                    }
                    else if (inString)
                    {
                        newLine.Append(line[i]);
                        if (line[i] == '"' && (i == 0 || line[i - 1] != '\\'))
                        {
                            inString = false;
                        }
                    }
                    else
                    {
                        if (i < line.Length - 1)
                        {
                            if (line[i] == '/' && line[i + 1] == '/')
                            {
                                break; // Rest of the line is a single-line comment, so skip it
                            }
                            else if (line[i] == '/' && line[i + 1] == '*')
                            {
                                inMultiLineComment = true;
                                i++; // Skip the '*' character
                            }
                            else if (line[i] == '"')
                            {
                                inString = true;
                                newLine.Append(line[i]);
                            }
                            else
                            {
                                newLine.Append(line[i]);
                            }
                        }
                        else
                        {
                            newLine.Append(line[i]);
                        }
                    }
                }

                if (!inMultiLineComment && newLine.Length > 0)
                {
                    newCodeLines.Add(newLine.ToString());
                }
            }

            _code = newCodeLines;
        }

        private bool Validate()
        {
            foreach (string line in _code)
            {
                //
            }

            return true;
        }

        private bool IsVariable(string codeLine)
        {
            return Regex.IsMatch(codeLine, @"(^\s*(void|text|char|number|bool)\s)|(\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*(=\s*(.+))$)");
        }

        private void TranslateVariable(string codeLine)
        {
            string type, name, value;
            Match match;

            if (Regex.IsMatch(codeLine, @"^\s*(void|text|char|number|bool)\s"))
            {
                if (codeLine.Contains("="))
                {
                    match = Regex.Match(codeLine, @"^\s*(void|text|char|number|bool)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*(=\s*(.+))$");
                    type = match.Groups[1].Value;
                    name = match.Groups[2].Value;
                    value = match.Groups[4].Value;

                    _compiledCode.Add($"var {name} {type}");
                    _compiledCode.Add($"set {name} {value}");
                    return;
                }

                match = Regex.Match(codeLine, @"^\s*(void|text|char|number|bool)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*$");
                type = match.Groups[1].Value;
                name = match.Groups[2].Value;

                _compiledCode.Add($"var {name} {type}");
                return;
            }

            match = Regex.Match(codeLine, @"\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*(=\s*(.+))$");
            name = match.Groups[1].Value;
            value = match.Groups[3].Value;

            _compiledCode.Add($"set {name} {value}");
        }

        private bool IsArray(string codeLine)
        {
            return Regex.IsMatch(codeLine, @"(^\s*(void|text|char|number|bool)\[\d+\]\s)|(\s*([a-zA-Z_][a-zA-Z0-9_]*)\[(\d+)\]\s*(=\s*(.+))$)");
        }

        private void TranslateArray(string codeLine)
        {
            string type, name, value, index;
            Match match;

            if (Regex.IsMatch(codeLine, @"^\s*(void|text|char|number|bool)\[\d+\]\s"))
            {
                match = Regex.Match(codeLine, @"^\s*(void|text|char|number|bool)\[(\d+)\]\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*$");
                type = match.Groups[1].Value;
                index = match.Groups[2].Value;
                name = match.Groups[3].Value;

                _compiledCode.Add($"array {name} {type} {index}");
                return;
            }

            match = Regex.Match(codeLine, @"\s*([a-zA-Z_][a-zA-Z0-9_]*)\[(\d+)\]\s*(=\s*(.+))$");
            name = match.Groups[1].Value;
            index = match.Groups[2].Value;
            value = match.Groups[4].Value;

            _compiledCode.Add($"set {name} at {index} {value}");
        }
    }
}