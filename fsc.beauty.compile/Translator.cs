using FSC.Dirty.Runtime.Template;
using System.Text.RegularExpressions;
using static FSC.Beauty.Compile.ValidationRegex;

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

                // If condition found, make conditional operation to variable
                if (line.Contains("==") || line.Contains("<") || line.Contains(">") || line.Contains("!=") || line.Contains(">=") || line.Contains("<="))
                {
                    int startIndex = line.Contains("if ") ? line.IndexOf("if ") + "if ".Length : line.IndexOf('=') + 1;
                    string valueSplit = line.Substring(startIndex).Trim();

                    string variableName = ConditionalOperationToVariable(valueSplit, i);

                    line = line.Replace(valueSplit, variableName);
                }

                if (ValidationRegex.Get(ValidationRegexTypes.NewVariable, i).IsMatch(line))
                {
                    NewVariable(line, i);
                    continue;
                }
                else if (ValidationRegex.Get(ValidationRegexTypes.SetVariable, i).IsMatch(line))
                {
                    SetVariable(line, i);
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
                else if (ValidationRegex.Get(ValidationRegexTypes.IfStatement, i).IsMatch(line))
                {
                    IfStatement(line, i, ref code);
                    continue;
                }
                else if (ValidationRegex.Get(ValidationRegexTypes.WhileStatement, i).IsMatch(line))
                {
                    IfStatement(line, i, ref code, true);
                    continue;
                }
                else if (line.StartsWith("raw "))
                {
                    Raw(line, i);
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
            var name = PrefixVar(match.Groups[2].Value);
            var value = match.Groups[3].Value;

            if (type == "var")
            {
                type = GetVariableType(value);
            }

            if (type == "variable")
            {
                try
                {
                    type = _foundVariables.First(v => v.Name == value).FscRuntimeType.ToString().ToLower();
                }
                catch
                {
                    type = _foundVariables.First(v => v.Name == PrefixVar(value)).FscRuntimeType.ToString().ToLower();
                }
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
            var name = PrefixVar(match.Groups[1].Value);
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
            var name = PrefixVar(match.Groups[2].Value);
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
                    if (!Regex.IsMatch(args[i], @"^(""|\d|'')"))
                    {
                        args[i] = PrefixVar(args[i]);
                    }

                    NewVariable($"var {variableName} = {args[i]}", row);
                    argVariables.Add(variableName);
                }
            }

            string argSplitter = argVariables.Any() ? "," : string.Empty;

            _code.Add($"extern {name} from \"{function}\"{argSplitter} {string.Join(", ", argVariables)}".TrimEnd());
        }

        private void SetExternFunctionVariable(string line, long row)
        {
            Match match = ValidationRegex.Get(ValidationRegexTypes.SetExternFunctionVariable, row).Match(line);
            var name = PrefixVar(match.Groups[1].Value);
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
                    if (!Regex.IsMatch(args[i], @"^(""|\d|'')"))
                    {
                        args[i] = PrefixVar(args[i]);
                    }

                    NewVariable($"var {variableName} = {args[i]}", row);
                    argVariables.Add(variableName);
                }
            }

            string argSplitter = argVariables.Any() ? "," : string.Empty;

            _code.Add($"extern {name} from \"{function}\"{argSplitter} {string.Join(", ", argVariables)}".TrimEnd());
        }

        private void CallExternFunction(string line, long row)
        {
            string name = $"{_uniqueVariableName}COMPILERvoidFUNCTION{row}";
            NewExternFunctionVariable($"void {name} = {line}", row);
        }

        private string GetVariableType(string value)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
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

            _code.Add($"target {jumpPoint}");
        }

        private void IfStatement(string line, long row, ref List<string> code, bool isLoop = false)
        {
            line = Regex.Replace(line, @"^\s*while\s", m => m.Value.Replace("while", "if"));

            Match match = ValidationRegex.Get(ValidationRegexTypes.IfStatement, row).Match(line);
            var conditionalValue = PrefixVar(match.Groups[1].Value);

            string variableName = $"{_uniqueVariableName}COMPILERIF{row}";
            string endIf = EndIfStatement(variableName, row, ref code, isLoop, !isLoop ? "" : $"{variableName}STARTBEFORE");
            NewVariable($"var {variableName} = {conditionalValue}", row);
            string startIf = $"{variableName}START";
            _code.Add($"target {startIf}BEFORE");
            _code.Add($"is {variableName} {startIf}");
            _code.Add($"jump {endIf}");
            _code.Add($"target {startIf}");
        }

        private string EndIfStatement(string ifName, long lastRow, ref List<string> code, bool isLoop = false, string loopReceiver = "")
        {
            string afterIfLine = code[(int)(lastRow + 1)];
            char spaces = afterIfLine[0];
            int countSpaces = afterIfLine.TakeWhile(c => c == spaces).Count();

            if (spaces != ' ' && spaces != '\t')
            {
                throw new($"Invalid if statement in line {lastRow}");
            }

            for (long i = lastRow + 1; i < code.Count; i++)
            {
                string line = code[(int)i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.TakeWhile(c => c == spaces).Count() < countSpaces)
                {
                    string endIfName = $"{_uniqueVariableName}COMPILERENDIF{lastRow}";
                    code.Insert((int)i, $"{endIfName}:");

                    if (isLoop)
                    {
                        code.Insert((int)i, $"raw jump {loopReceiver}");
                    }

                    return $"{endIfName}";
                }
            }

            throw new($"Invalid if statement in line {lastRow}");
        }

        private string /* New Variable*/ ConditionalOperationToVariable(string value, long row)
        {
            string variableName = $"{_uniqueVariableName}COMPILEROPERATOR{row}";

            string value1 = Regex.Split(value, @"==|<|>|!=|>=|<=|&&|\|\|")[0].Trim();
            string value2 = Regex.Split(value, @"==|<|>|!=|>=|<=|&&|\|\|")[1].Trim();

            NewVariable($"var {variableName}Temp1 = {PrefixVar(value1)}", row);
            NewVariable($"var {variableName}Temp2 = {PrefixVar(value2)}", row);

            return CalculateConditionalOperation($"{variableName}Temp1", $"{variableName}Temp2", Regex.Match(value, @"==|<|>|!=|>=|<=|&&|\|\|").Value, row);
        }

        private string /* New Variable */ CalculateConditionalOperation(string variable1, string variable2, string @operator, long row)
        {
            string returnVariableName = $"{_uniqueVariableName}COMPILEROPERATORResult{row}";
            string variableGreater = $"{returnVariableName}Greater";
            string variableLess = $"{returnVariableName}Less";
            string variableFalse = $"{returnVariableName}False";
            NewVariable($"var {returnVariableName} = 0", row);
            switch (@operator)
            {
                case "==":
                    _code.Add($"equals {returnVariableName} {variable1} {variable2}");
                    break;
                case "<":
                    _code.Add($"less {returnVariableName} {variable1} {variable2}");
                    break;
                case ">":
                    _code.Add($"greater {returnVariableName} {variable1} {variable2}");
                    break;
                case "!=":
                    NewVariable($"var {variableGreater} = 0", row);
                    NewVariable($"var {variableFalse} = 0", row);

                    _code.Add($"greater {variableGreater} {variable1} {variable2}");
                    _code.Add($"less {variableLess} {variable1} {variable2}");

                    _code.Add($"or {returnVariableName} {variableLess} {variableGreater}");
                    _code.Add($"equals {returnVariableName} {returnVariableName} {variableFalse}");
                    break;
                case ">=":
                    NewVariable($"var {variableGreater} = 0", row);

                    _code.Add($"greater {variableGreater} {variable1} {variable2}");
                    _code.Add($"equals {returnVariableName} {variable1} {variable2}");
                    _code.Add($"or {returnVariableName} {returnVariableName} {variableGreater}");
                    break;
                case "<=":
                    NewVariable($"var {variableLess} = 0", row);

                    _code.Add($"less {variableLess} {variable1} {variable2}");
                    _code.Add($"equals {returnVariableName} {variable1} {variable2}");
                    _code.Add($"or {returnVariableName} {returnVariableName} {variableLess}");
                    break;
                default:
                    throw new($"Invalid conditional operator {@operator} in line {row}");
            }

            return returnVariableName;
        }

        private void Raw(string line, long row)
        {
            line = line.Substring("raw ".Length);
            _code.Add(line);
        }

        private string PrefixVar(string @var)
        {
            if (@var.StartsWith(_uniqueVariableName))
            {
                return @var;
            }

            return $"{_uniqueVariableName}{var}";
        }
    }
}
