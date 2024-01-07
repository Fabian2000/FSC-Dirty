using FSC.Dirty.Runtime.Template;
using System.Text.RegularExpressions;

namespace FSC.Beauty.Compile
{
    internal class Translator
    {
        private List<VarArrInfo> _foundVariables = new List<VarArrInfo>();
        private IFscCompiler _compilerInfo;
        private List<string> _code = new List<string>();
        private string _uniqueVariableName = string.Empty;

        internal struct VarArrInfo
        {
            internal required string Name;
            internal required bool IsArray;
            internal required FscRuntimeTypes FscRuntimeType;
        }

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
                    
                    WriteVariable(name, value, ArrayCheck(ref value));
                }
            }
        }

        private (bool IsArray, FscRuntimeTypes FscRuntimeType) ArrayCheck(ref string value)
        {
            Regex variable = new Regex("[A-Za-z0-9]+"); // Variable Check
            if (variable.IsMatch(value))
            {
                string val = value;
                VarArrInfo variableInfo = _foundVariables.Find(x => x.Name.Equals(_uniqueVariableName + val));
                if (!variableInfo.Equals(default))
                {
                    if (variableInfo.IsArray)
                    {
                        return (true, variableInfo.FscRuntimeType);
                    }
                    else
                    {
                        return (false, variableInfo.FscRuntimeType);
                    }
                }
            }

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
            
            Regex method = new Regex("(.*)\\(.*\\)$"); // Method Check
            if (method.IsMatch(value))
            {
                (FscRuntimeTypes fscRuntimeType, bool isArray) = _compilerInfo.MethodReturnTypes[method.Match(value).Groups[1].Value];
                return (isArray, fscRuntimeType);
            }
            
            if (Regex.IsMatch(value, @"\[\s*(("".*?""|\d+\s*:\s*text)\s*|"".*?"")\s*]")) //Text Array Check
            {
                return (true, FscRuntimeTypes.Text);
            }
            else if (!Regex.IsMatch(value, @"\[\s*((\d+(\.\d+)?)(\s*,\s*\d+(\.\d+)?)*\s*(:\s*number)?)\s*]")) // Number Array Check
            {
                return (true, FscRuntimeTypes.Number);
            }
            else if (!Regex.IsMatch(value, @"\[\s*(('.*?'|\d+\s*:\s*char)\s*|'.*?')\s*]")) // Char Array Check
            {
                return (true, FscRuntimeTypes.Char);
            }

            return (false, FscRuntimeTypes.Void);
        }

        private void WriteVariable(string name, string value, (bool IsArray, FscRuntimeTypes FscRuntimeType) info)
        {
            string dirtyCode;
            string fullVariableName = _uniqueVariableName + name;

            if (!_foundVariables.Find(x => x.Name.Equals(fullVariableName, StringComparison.OrdinalIgnoreCase)).Equals(default))
            {
                _foundVariables.Add(new VarArrInfo { Name = fullVariableName, IsArray = info.IsArray, FscRuntimeType = info.FscRuntimeType });

                if (info.IsArray)
                {
                    int arraySize = CalculateArraySize(value, info.FscRuntimeType);
                    dirtyCode = $"array {fullVariableName} {info.FscRuntimeType.ToString().ToLower()} {arraySize}";
                    _code.Insert(0, dirtyCode);
                    InitializeArray(fullVariableName, value, arraySize);
                }
                else
                {
                    dirtyCode = $"var {fullVariableName} {info.FscRuntimeType.ToString().ToLower()}";
                    _code.Insert(0, dirtyCode);
                    dirtyCode = $"set {fullVariableName} {value.SingleTrim('\'')}";
                    _code.Add(dirtyCode);
                }
            }
            else if (info.IsArray)
            {
                // Setze Array-Werte, falls die Variable bereits initialisiert wurde
                InitializeArray(fullVariableName, value, CalculateArraySize(value, info.FscRuntimeType));
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
