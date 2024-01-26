using fsc.beauty.compile;
using FSC.Dirty.Runtime.Template;
using System.Text.RegularExpressions;
using static fsc.beauty.compile.ValidationRegex;

namespace FSC.Beauty.Compile
{
    internal class Translator
    {
        private List<VarArrInfo> _foundVariables = new List<VarArrInfo>();
        private List<string> _code = new List<string>();
        private string _uniqueVariableName = string.Empty;

        internal struct VarArrInfo
        {
            internal required string Name;
            internal required bool IsArray;
            internal required FscRuntimeTypes FscRuntimeType;
        }

        public Translator(string uniqueVariablePrefix)
        {
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

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (ValidationRegex.Get(ValidationRegexTypes.NewVariable, i).IsMatch(line))
                {
                    NewVariable(line, i);
                    continue;
                }
                else if (ValidationRegex.Get(ValidationRegexTypes.SetVariable, i).IsMatch(line))
                {
                    SetVariable(line);
                    continue;
                }
                else if (ValidationRegex.Get(ValidationRegexTypes.NewArray, i).IsMatch(line))
                {
                    NewArray(line, i);
                    continue;
                }
                else if (ValidationRegex.Get(ValidationRegexTypes.SetArray, i).IsMatch(line))
                {
                    SetArray(line, i);
                    continue;
                }
                else if (ValidationRegex.Get(ValidationRegexTypes.SetArrayIndex, i).IsMatch(line))
                {
                    SetArrayIndex(line, i);
                    continue;
                }
                else if (ValidationRegex.Get(ValidationRegexTypes.NewExternFunctionVariable, i).IsMatch(line))
                {
                    NewExternFunctionVariable(line, i);
                    continue;
                }
                else if (ValidationRegex.Get(ValidationRegexTypes.SetExternFunctionVariable, i).IsMatch(line))
                {
                    SetExternFunctionVariable(line, i);
                    continue;
                }
                else if (ValidationRegex.Get(ValidationRegexTypes.CallExternFunction, i).IsMatch(line))
                {
                    CallExternFunction(line, i);
                    continue;
                }
                else if (ValidationRegex.Get(ValidationRegexTypes.GoTo, i).IsMatch(line))
                {
                    GoTo(line, i);
                    continue;
                }
                else if (ValidationRegex.Get(ValidationRegexTypes.GoToTarget, i).IsMatch(line))
                {
                    GoToTarget(line, i);
                    continue;
                }
                else
                {
                    throw new($"Invalid line {i}: {line}");
                }
            }

            return _code;
        }

        private void NewVariable(string line, long row)
        {
            Match match = ValidationRegex.Get(ValidationRegexTypes.NewVariable, row).Match(line);
            var type = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            var value = match.Groups[3].Value;

            if (type == "var")
            {
                type = GetVariableType(value);
            }

            if (type == "variable")
            {
                type = _foundVariables.First(v => v.Name == value).FscRuntimeType.ToString().ToLower();
            }

            if (_foundVariables.Any(v => v.Name == name))
            {
                throw new($"Variable {name} in line {row} already exists");
            }

            _foundVariables.Add(new VarArrInfo
            {
                Name = name,
                IsArray = false,
                FscRuntimeType = type switch
                {
                    "text" => FscRuntimeTypes.Text,
                    "number" => FscRuntimeTypes.Number,
                    "char" => FscRuntimeTypes.Char,
                    "void" => FscRuntimeTypes.Void,
                    _ => throw new($"Invalid type {type} in line {row}")
                }
            });

            _code.Insert(0, $"var {name} {type}");
            _code.Add($"set {name} {value}");
        }

        private void SetVariable(string line, long row)
        {
            Match match = ValidationRegex.Get(ValidationRegexTypes.SetVariable, row).Match(line);
            var name = match.Groups[1].Value;
            var value = match.Groups[2].Value;

            if (!_foundVariables.Any(v => v.Name == name))
            {
                throw new($"Variable {name} in line {row} already exists");
            }

            var foundVariable = _foundVariables.First(v => v.Name == name);
            
            if (foundVariable.IsArray)
            {
                throw new($"Variable {name} in line {row} may not be an array");
            }

            _code.Add($"set {name} {value}");
        }

        private void NewArray(string line, long row)
        {
            Match match = ValidationRegex.Get(ValidationRegexTypes.NewArray, row).Match(line);
            var type = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            var value = ValidationRegex.SplitArguments(match.Groups[4].Value);

            if (type == "var")
            {
                type = GetVariableType(value[0]);
            }

            if (!value.All(v => GetVariableType(v) == type))
            {
                throw new($"Invalid value {value} in line {row}");
            }

            if (_foundVariables.Any(v => v.Name == name))
            {
                throw new($"Array {name} in line {row} already exists");
            }

            _foundVariables.Add(new VarArrInfo
            {
                Name = name,
                IsArray = true,
                FscRuntimeType = type switch
                {
                    "text" => FscRuntimeTypes.Text,
                    "number" => FscRuntimeTypes.Number,
                    "char" => FscRuntimeTypes.Char,
                    _ => throw new($"Invalid type {type} in line {row}")
                }
            });

            _code.Insert(0, $"array {name} {type} {value.Count()}");
            
            for (int i = 0; i < value.Count(); i++)
            {
                _code.Add($"set {name} at {i} {value[i]}");
            }
        }

        private void SetArray(string line, long row)
        {
            Match match = ValidationRegex.Get(ValidationRegexTypes.SetArray, row).Match(line);
            var type = string.Empty;
            var name = match.Groups[1].Value;
            var value = ValidationRegex.SplitArguments(match.Groups[3].Value);

            if (!_foundVariables.Any(v => v.Name == name))
            {
                throw new($"Array {name} in line {row} does not exists");
            }

            var foundVariable = _foundVariables.First(v => v.Name == name);
            type = foundVariable.FscRuntimeType.ToString().ToLower();

            if (!foundVariable.IsArray)
            {
                throw new($"Array {name} in line {row} may not be a variable");
            }

            if (!value.All(v => GetVariableType(v) == type))
            {
                throw new($"Invalid value {value} in line {row}");
            }

            for (int i = 0; i < value.Count(); i++)
            {
                _code.Add($"set {name} at {i} {value[i]}");
            }
        }

        private void SetArrayIndex(string line, long row)
        {
            Match match = ValidationRegex.Get(ValidationRegexTypes.SetArrayIndex, row).Match(line);
            var type = string.Empty;
            var name = match.Groups[1].Value;
            var index = match.Groups[2].Value;
            var value = match.Groups[3].Value;

            if (!_foundVariables.Any(v => v.Name == name))
            {
                throw new($"Array {name} in line {row} does not exists");
            }

            var foundVariable = _foundVariables.First(v => v.Name == name);
            type = foundVariable.FscRuntimeType.ToString().ToLower();

            if (!foundVariable.IsArray)
            {
                throw new($"Array {name} in line {row} may not be a variable");
            }

            if (GetVariableType(value) != type)
            {
                throw new($"Invalid value {value} in line {row}");
            }
            
            _code.Add($"set {name} at {index} {value}");
        }

        private void NewExternFunctionVariable(string line, long row)
        {
            Match match = ValidationRegex.Get(ValidationRegexTypes.NewExternFunctionVariable, row).Match(line);
            var type = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            var function = match.Groups[3].Value;
            var args = ValidationRegex.SplitArguments(match.Groups[4].Value);

            if (type == "var")
            {
                throw new($"Function return variable type may not be var in line {row}");
            }

            if (_foundVariables.Any(v => v.Name == name))
            {
                throw new($"Variable {name} in line {row} already exists");
            }

            _foundVariables.Add(new VarArrInfo
            {
                Name = name,
                IsArray = false,
                FscRuntimeType = type switch
                {
                    "text" => FscRuntimeTypes.Text,
                    "number" => FscRuntimeTypes.Number,
                    "char" => FscRuntimeTypes.Char,
                    "void" => FscRuntimeTypes.Void,
                    _ => throw new($"Invalid type {type} in line {row}")
                }
            });

            _code.Insert(0, $"var {name} {type}");
            List<string> argVariables = new List<string>();

            for (int i = 0; i < args.Count(); i++)
            {
                //_code.Add($""); // TODO: Put all values into variables. Not needed, if already a variable or array.
                if (_foundVariables.Any(v => v.Name == args[i]))
                {
                    argVariables.Add(args[i]);
                    continue;
                }

                var variableName = $"{_uniqueVariableName}COMPILER{name}ARG{i}";

                if (!string.IsNullOrWhiteSpace(args[i]))
                {
                    NewVariable($"var {variableName} = {args[i]}", row);
                    argVariables.Add(variableName);
                }
            }

            string argSplitter = argVariables.Any() ? "," : string.Empty;

            _code.Add($"extern {name} from \"{function}\"{argSplitter} {string.Join(", ", argVariables)}");
        }

        private void SetExternFunctionVariable(string line, long row)
        {
            Match match = ValidationRegex.Get(ValidationRegexTypes.SetExternFunctionVariable, row).Match(line);
            var name = match.Groups[1].Value;
            var function = match.Groups[2].Value;
            var args = ValidationRegex.SplitArguments(match.Groups[3].Value);

            if (!_foundVariables.Any(v => v.Name == name))
            {
                throw new($"Variable {name} in line {row} doesn't exists");
            }

            List<string> argVariables = new List<string>();

            for (int i = 0; i < args.Count(); i++)
            {
                //_code.Add($""); // TODO: Put all values into variables. Not needed, if already a variable or array.
                if (_foundVariables.Any(v => v.Name == args[i]))
                {
                    argVariables.Add(args[i]);
                    continue;
                }

                var variableName = $"{_uniqueVariableName}COMPILER{name}ARG{i}";

                if (!string.IsNullOrWhiteSpace(args[i]))
                {
                    NewVariable($"var {variableName} = {args[i]}", row);
                    argVariables.Add(variableName);
                }
            }

            string argSplitter = argVariables.Any() ? "," : string.Empty;

            _code.Add($"extern {name} from \"{function}\"{argSplitter} {string.Join(", ", argVariables)}");
        }

        private void CallExternFunction(string line, long row)
        {
            string name = $"{_uniqueVariableName}COMPILERvoidFUNCTION{row}";
            NewExternFunctionVariable($"void {name} = {line}", row);
        }

        private string GetVariableType(string value)
        {
            if (value.StartsWith('"') && value.EndsWith('"'))
            {
                return "text";
            }
            else if (value.Contains('.'))
            {
                return "number";
            }
            else if (int.TryParse(value, out int _))
            {
                return "number";
            }
            else if (value.StartsWith("'") && value.EndsWith("'"))
            {
                return "char";
            }
            else
            {
                return "variable";
            }
        }

        private void GoTo(string line, long row)
        {
            Match match = ValidationRegex.Get(ValidationRegexTypes.GoTo, row).Match(line);
            var jumpPoint = match.Groups[1].Value;

            _code.Add($"jump {jumpPoint}");
        }

        private void GoToTarget(string line, long row)
        {
            Match match = ValidationRegex.Get(ValidationRegexTypes.GoToTarget, row).Match(line);
            var jumpPoint = match.Groups[1].Value;

            _code.Add($"{jumpPoint}:");
        }
    }
}
