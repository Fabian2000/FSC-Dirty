using FSC.Dirty.Runtime.Template;
using System.Text.RegularExpressions;

namespace FSC.Beauty.Compile
{
    internal class Translator
    {
        private List<string> _foundVariables = new List<string>();
        private IFscCompiler _compilerInfo;
        private List<string> _code = new List<string>();
        private string _uniqueVariableName = string.Empty;

        public Translator(IFscCompiler compilerInfo, string uniqueVariablePrefix)
        {
            _compilerInfo = compilerInfo;
            _uniqueVariableName = uniqueVariablePrefix;
        }

        internal List<string> Translate(List<string> code)
        {
            if (!code.Any())
            {
                return code;
            }

            for (int i = 0; i < code.Count; i++)
            {
                string line = code[i];

                if (!string.IsNullOrEmpty(line))
                {
                    continue;
                }

                Regex variable = new Regex(@"^([A-Za-z0-9]*)(\s)*=(\s)*(.+)$");
                if (variable.IsMatch(line))
                {
                    string name = variable.Match(line).Groups[1].Value;
                    string value = variable.Match(line).Groups[4].Value;
                    
                    WriteVariable(name, value, ArrayCheck(value));
                }
            }
        }

        private (bool IsArray, FscRuntimeTypes FscRuntimeType) ArrayCheck(string value)
        {
            if (value.StartsWith("\"") && value.EndsWith("\"")) // Text Check
            {
                return (false, FscRuntimeTypes.Text);
            }
            else if (Regex.IsMatch(value, "^([0-9]+|[0-9.]+)$")) // Number Check
            {
                return (false, FscRuntimeTypes.Number);
            }
            else if (Regex.IsMatch(value, "^'.'$")) // Char Check
            {
                return (false, FscRuntimeTypes.Char);
            }
            Regex method = new Regex("(.*)\\(.*\\)$");
            if (method.IsMatch(value))
            {
                (FscRuntimeTypes fscRuntimeType, bool isArray) = _compilerInfo.MethodReturnTypes[method.Match(value).Groups[1].Value];
                return (isArray, fscRuntimeType);
            }
            
            if (Regex.IsMatch(value, @"\[\s*(("".*?""|\d+\s*:\s*text)\s*|"".*?"")\s*]"))
            {
                return (true, FscRuntimeTypes.Text);
            }
            else if (!Regex.IsMatch(value, @"\[\s*((\d+(\.\d+)?)(\s*,\s*\d+(\.\d+)?)*\s*(:\s*number)?)\s*]"))
            {
                return (true, FscRuntimeTypes.Number);
            }
            else if (!Regex.IsMatch(value, @"\[\s*(('.*?'|\d+\s*:\s*char)\s*|'.*?')\s*]"))
            {
                return (true, FscRuntimeTypes.Char);
            }

            return (false, FscRuntimeTypes.Void);
        }

        private void WriteVariable(string name, string value, (bool IsArray, FscRuntimeTypes FscRuntimeType) value2)
        {
            string dirtyCode;
            string fullVariableName = _uniqueVariableName + name;

            if (!_foundVariables.Contains(fullVariableName))
            {
                _foundVariables.Add(fullVariableName);

                if (value2.IsArray)
                {
                    int arraySize = CalculateArraySize(value, value2.FscRuntimeType);
                    dirtyCode = $"array {fullVariableName} {value2.FscRuntimeType.ToString().ToLower()} {arraySize}";
                    _code.Insert(0, dirtyCode);
                    InitializeArray(fullVariableName, value, arraySize);
                }
                else
                {
                    dirtyCode = $"var {fullVariableName} {value2.FscRuntimeType.ToString().ToLower()}";
                    _code.Insert(0, dirtyCode);
                    dirtyCode = $"set {fullVariableName} {value}";
                    _code.Add(dirtyCode);
                }
            }
            else if (value2.IsArray)
            {
                // Setze Array-Werte, falls die Variable bereits initialisiert wurde
                InitializeArray(fullVariableName, value, CalculateArraySize(value, value2.FscRuntimeType));
            }
        }

        private int CalculateArraySize(string arrayInitialization, FscRuntimeTypes type)
        {
            if (type == FscRuntimeTypes.Number)
            {
                // Für Number, zähle einfach die Kommas und addiere 1
                return arrayInitialization.Count(c => c == ',') + 1;
            }

            int arraySize = 0;
            bool inElement = false;
            char elementStartChar = type == FscRuntimeTypes.Char ? '\'' : '\"';

            foreach (char c in arrayInitialization)
            {
                if (c == elementStartChar && !inElement)
                {
                    inElement = true;
                }
                else if (c == elementStartChar && inElement)
                {
                    inElement = false;
                    arraySize++; // Ende eines Elements
                }
                else if (c == ',' && !inElement)
                {
                    // Für Text und Char, erhöhe die Größe nur, wenn das Komma außerhalb eines Elements ist
                    arraySize++;
                }
            }

            return arraySize;
        }

        private void InitializeArray(string arrayName, string initialValue, int size)
        {
            string targetLoopStart = $"loopStart_{arrayName}";
            string targetLoopEnd = $"loopEnd_{arrayName}";
            string indexVariable = $"{arrayName}_index";

            _code.Add($"var {indexVariable} number");
            _code.Add($"set {indexVariable} 0");
            _code.Add($"target {targetLoopStart}");

            _code.Add($"equals {indexVariable} {indexVariable} {size}");
            _code.Add($"is {indexVariable} {targetLoopEnd}");

            _code.Add($"set {arrayName} at {indexVariable} {initialValue}");

            // Externe Methode Plus einbinden, um den Index zu erhöhen
            _code.Add($"extern {indexVariable} from \"Plus\", {indexVariable}, 1");

            _code.Add($"jump {targetLoopStart}");
            _code.Add($"target {targetLoopEnd}");
        }
    }
}
