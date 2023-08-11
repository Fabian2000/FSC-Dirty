using System.Text.RegularExpressions;

using FSC.Dirty.Runtime.Template;

namespace FSC.Dirty.Runtime
{
    internal class DirtyEngine
    {
        private readonly List<string> _code;
        private readonly IFscRuntime _runtimeDefaults;

        internal DirtyEngine(List<string> code, IFscRuntime runtimeDefaults)
        {
            _code = code;
            _runtimeDefaults = runtimeDefaults;
        }

        internal void Run()
        {
            for (int i = 0; i < _code.Count; i++)
            {
                if (_code[i].StartsWith("jump"))
                {
                    i = FindTarget(_code[i].Split(' ')[1]);
                    continue;
                }
                else if (_code[i].StartsWith("var"))
                {
                    string name = _code[i].Split(' ')[1];
                    string type = _code[i].Split(' ')[2];
                    if (VariableManagement.Exists(name)) throw new Exception($"Variable [{name}] in line [{i}] already exists");
                    VariableManagement.Variables.Add(name, new Variable
                    {
                        RuntimeType = GetType(type),
                    });
                    continue;
                }
                else if (_code[i].StartsWith("array"))
                {
                    string name = _code[i].Split(' ')[1];
                    string type = _code[i].Split(' ')[2];
                    int size = Convert.ToInt32(_code[i].Split(' ')[3]);
                    if (VariableManagement.Exists(name)) throw new Exception($"Array [{name}] in line [{i}] already exists");
                    VariableManagement.Arrays.Add(name, new Array
                    {
                        RuntimeType = GetType(type),
                        Value = new object[size]
                    });
                    continue;
                }
                else if (_code[i].StartsWith("set") && _code[i].Split(' ')[2] != "at")
                {
                    string name = _code[i].Split(' ')[1];

                    if (!VariableManagement.Variables.ContainsKey(name)) throw new Exception($"Variable [{name}] in line [{i}] does not exist");

                    string value = _code[i].Substring(_code[i].IndexOf(" ", 4) + 1);

                    if (value.Length > 1 && !value.StartsWith("\"") && !value.EndsWith("\"") && !Regex.IsMatch(value, @"^(\d+.\d+|\d+)$"))
                    {
                        if (!VariableManagement.Variables.ContainsKey(value)) throw new Exception($"[{value}] in line [{i}] is not a variable");

                        Variable variable1 = VariableManagement.Variables[name];
                        Variable variable2 = VariableManagement.Variables[(value?.ToString() ?? "")];

                        if (variable1.RuntimeType != variable2.RuntimeType) throw new Exception($"Invalid cast in line [{i}]");

                        VariableManagement.Variables[name] = variable2;
                    }
                    else
                    {
                        if (WhichType(value?.ToString() ?? "") == VariableManagement.Variables[name].RuntimeType && VariableManagement.Variables[name].RuntimeType != FscRuntimeTypes.Void)
                        {
                            VariableManagement.Variables[name].Value = VariableManagement.ConvertToObject(VariableManagement.Variables[name].RuntimeType, value ?? string.Empty);
                        }
                        else if (VariableManagement.Variables[name].RuntimeType == FscRuntimeTypes.Void)
                        {
                            throw new Exception($"void can't be set in line [{i}]");
                        }
                    }
                    continue;
                }
                else if (_code[i].StartsWith("set"))
                {
                    string name = _code[i].Split(' ')[1];

                    if (!VariableManagement.Arrays.ContainsKey(name)) throw new Exception($"Variable [{name}] in line [{i}] does not exist");

                    int index = Convert.ToInt32(_code[i].Split(' ')[3]);
                    string value = _code[i];
                    value = value.Remove(0, value.IndexOf(" ", 4) + 1);
                    value = value.Remove(0, value.IndexOf(" ", 4) + 1);

                    if (value.Length > 1 && !value.StartsWith("\"") && !value.EndsWith("\"") && !Regex.IsMatch(value, @"^(\d+.\d+|\d+)$"))
                    {
                        if (!VariableManagement.Variables.ContainsKey(value)) throw new Exception($"[{value}] in line [{i}] is not a variable");

                        Array array = VariableManagement.Arrays[name];
                        Variable variable = VariableManagement.Variables[(value?.ToString() ?? "")];

                        if (array.RuntimeType != variable.RuntimeType) throw new Exception($"Invalid cast in line [{i}]");

                        VariableManagement.Arrays[name].Value![index] = variable.Value!;
                    }
                    else
                    {
                        if (WhichType(value?.ToString() ?? "") == VariableManagement.Arrays[name].RuntimeType && VariableManagement.Arrays[name].RuntimeType != FscRuntimeTypes.Void)
                        {
                            VariableManagement.Arrays[name].Value![index] = VariableManagement.ConvertToObject(VariableManagement.Arrays[name].RuntimeType, value ?? string.Empty)!;
                        }
                        else if (VariableManagement.Arrays[name].RuntimeType == FscRuntimeTypes.Void)
                        {
                            throw new Exception($"void can't be set in line [{i}]");
                        }
                    }
                    continue;
                }
                else if (_code[i].StartsWith("equals"))
                {
                    string[] splits = _code[i].Split(' ');
                    string target = splits[1];
                    string var1 = splits[2];
                    string var2 = splits[3];

                    if (!VariableManagement.Variables.ContainsKey(var1)) throw new Exception($"Variable [{var1}] in line [{i}] does not exist");
                    if (!VariableManagement.Variables.ContainsKey(var2)) throw new Exception($"Variable [{var2}] in line [{i}] does not exist");

                    bool resultValue = VariableManagement.Variables[var1].Value!.Equals(VariableManagement.Variables[var2].Value);
                    bool resultTypes = VariableManagement.Variables[var1].RuntimeType!.Equals(VariableManagement.Variables[var2].RuntimeType);
                    bool result = resultValue && resultTypes;

                    if (VariableManagement.Variables[target].RuntimeType != FscRuntimeTypes.Number) throw new Exception($"Only number may be used as boolean in line {i}");

                    VariableManagement.Variables[target].Value = result ? 1 : 0;
                    continue;
                }
                else if (_code[i].StartsWith("greater"))
                {
                    string[] splits = _code[i].Split(' ');
                    string target = splits[1];
                    string var1 = splits[2];
                    string var2 = splits[3];

                    if (!VariableManagement.Variables.ContainsKey(var1)) throw new Exception($"Variable [{var1}] in line [{i}] does not exist");
                    if (!VariableManagement.Variables.ContainsKey(var2)) throw new Exception($"Variable [{var2}] in line [{i}] does not exist");

                    if (VariableManagement.Variables[var1].RuntimeType != FscRuntimeTypes.Number) throw new Exception($"Variable must be a number in line {i}");
                    if (VariableManagement.Variables[var2].RuntimeType != FscRuntimeTypes.Number) throw new Exception($"Variable must be a number in line {i}");

                    bool result = (double)VariableManagement.Variables[var1].Value! > (double)VariableManagement.Variables[var2].Value!;

                    if (VariableManagement.Variables[target].RuntimeType != FscRuntimeTypes.Number) throw new Exception($"Only number may be used as boolean in line {i}");

                    VariableManagement.Variables[target].Value = result ? 1 : 0;
                    continue;
                }
                else if (_code[i].StartsWith("less"))
                {
                    string[] splits = _code[i].Split(' ');
                    string target = splits[1];
                    string var1 = splits[2];
                    string var2 = splits[3];

                    if (!VariableManagement.Variables.ContainsKey(var1)) throw new Exception($"Variable [{var1}] in line [{i}] does not exist");
                    if (!VariableManagement.Variables.ContainsKey(var2)) throw new Exception($"Variable [{var2}] in line [{i}] does not exist");

                    if (VariableManagement.Variables[var1].RuntimeType != FscRuntimeTypes.Number) throw new Exception($"Variable must be a number in line {i}");
                    if (VariableManagement.Variables[var2].RuntimeType != FscRuntimeTypes.Number) throw new Exception($"Variable must be a number in line {i}");

                    bool result = (double)VariableManagement.Variables[var1].Value! < (double)VariableManagement.Variables[var2].Value!;

                    if (VariableManagement.Variables[target].RuntimeType != FscRuntimeTypes.Number) throw new Exception($"Only number may be used as boolean in line {i}");

                    VariableManagement.Variables[target].Value = result ? 1 : 0;
                    continue;
                }
                else if (_code[i].StartsWith("is"))
                {
                    string name = _code[i].Split(' ')[1];

                    if (!VariableManagement.Variables.ContainsKey(name)) throw new Exception($"Variable [{name}] in line [{i}] does not exist");

                    if (VariableManagement.Variables[name].RuntimeType != FscRuntimeTypes.Number) throw new Exception($"Variable {name} must be a number in line {i}");

                    if ((int)VariableManagement.Variables[name].Value! == 1)
                    {
                        i = FindTarget(_code[i].Split(' ')[2]);
                    }
                    continue;
                }
                else if (_code[i].StartsWith("in"))
                {
                    string[] splits = _code[i].Split(' ');
                    string target = splits[1];
                    string variable = splits[2];
                    string array = splits[3];

                    if (!VariableManagement.Variables.ContainsKey(target)) throw new Exception($"Variable [{target}] in line [{i}] does not exist");
                    if (!VariableManagement.Variables.ContainsKey(variable)) throw new Exception($"Variable [{variable}] in line [{i}] does not exist");
                    if (!VariableManagement.Arrays.ContainsKey(array)) throw new Exception($"Variable [{array}] in line [{i}] does not exist");

                    Variable vari = VariableManagement.Variables[variable];
                    Array arr = VariableManagement.Arrays[array];

                    VariableManagement.Variables[target].Value = arr.Value!.Contains(vari.Value) ? 1 : 0;

                    continue;
                }
                else if (_code[i].StartsWith("extern"))
                {
                    string[] splits = _code[i].Split(' ');
                    string result = splits[1];
                    string method = splits[3].TrimEnd(',').Trim('"');
                    string[] args = splits.Skip(4).ToArray();
                    List<object> objArgs = new List<object>();

                    foreach (string arg in args)
                    {
                        if (VariableManagement.Variables.ContainsKey(arg))
                        {
                            objArgs.Add(VariableManagement.Variables[arg].Value!);
                        }
                        else if (VariableManagement.Arrays.ContainsKey(arg))
                        {
                            objArgs.Add(VariableManagement.Arrays[arg].Value!);
                        }
                        else
                        {
                            throw new Exception($"{arg} is not a valid variable or array in line [{i}]");
                        }
                    }

                    if (!VariableManagement.Exists(result))
                    {
                        throw new Exception($"{result} is not a valid variable or array in line [{i}]");
                    }

                    if (!_runtimeDefaults.ExternCallMethods.ContainsKey(method))
                    {
                        throw new Exception($"{method}() => is not a valid extern method in line [{i}]");
                    }

                    (FscRuntimeTypes type, object? @return) = _runtimeDefaults.ExternCallMethods[method](objArgs.ToArray());

                    if (VariableManagement.Variables.ContainsKey(result) && VariableManagement.Variables[result].RuntimeType == type)
                    {
                        if (@return is null)
                        {
                            continue;
                        }

                        VariableManagement.Variables[result].Value = @return;
                        continue;
                    }
                    else if (VariableManagement.Arrays.ContainsKey(result) && VariableManagement.Arrays[result].RuntimeType == type)
                    {
                        if (@return is null)
                        {
                            continue;
                        }

                        VariableManagement.Arrays[result].Value = (object[])@return;
                        continue;
                    }

                    throw new Exception($"Return type does not match to the return value in line [{i}]");
                }
            }
        }

        private int FindTarget(string name)
        {
            int index = _code.FindIndex(x => x == $"target {name}");

            if (index == -1)
            {
                throw new Exception(@$"Missing target ""{name}""");
            }

            return index;
        }

        private FscRuntimeTypes GetType(string typename)
        {
            switch (typename)
            {
                case "void":
                    return FscRuntimeTypes.Void;
                case "text":
                    return FscRuntimeTypes.Text;
                case "char":
                    return FscRuntimeTypes.Char;
                case "number":
                    return FscRuntimeTypes.Number;
            }

            return FscRuntimeTypes.Void;
        }

        private FscRuntimeTypes WhichType(string value)
        {
            if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length > 1)
                return FscRuntimeTypes.Text;

            if (Regex.IsMatch(value, @"^(\d+.\d+|\d+)$"))
                return FscRuntimeTypes.Number;

            if (value.Length == 1)
                return FscRuntimeTypes.Char;

            return FscRuntimeTypes.Void;
        }
    }
}


/*
 
 using System;
using System.Collections.Generic;

namespace CustomLanguageInterpreter
{
    class Program
    {
        // Definition des Func-Delegaten
        delegate object CustomFunc(params object[] parameters);

        static void Main(string[] args)
        {
            // Dictionary von Aktionen erstellen und mit Methoden füllen
            Dictionary<string, CustomFunc> funcDictionary = new Dictionary<string, CustomFunc>();
            funcDictionary["ReadLine"] = ReadLineFunc;
            // Weitere Methoden hinzufügen...

            // Code-Ausführung
            string[] _code = new string[] { "result from \"ReadLine\" parameterVariable" }; // Beispielcode
            VariableManagement.Variables["parameterVariable"] = new Variable { Value = "Enter a value: " }; // Beispielvariable

            for (int i = 0; i < _code.Length; i++)
            {
                string[] splits = _code[i].Split(' ');
                string result = splits[0];
                string method = splits[2].Trim('"');
                string[] args = splits.Skip(3).ToArray();

                List<object> objArgs = new List<object>();

                foreach (string arg in args)
                {
                    if (VariableManagement.Variables.ContainsKey(arg))
                    {
                        objArgs.Add(VariableManagement.Variables[arg].Value!);
                    }
                    else if (VariableManagement.Arrays.ContainsKey(arg))
                    {
                        objArgs.Add(VariableManagement.Arrays[arg].Value!);
                    }
                    else
                    {
                        throw new Exception($"{arg} is not a valid variable or array");
                    }
                }

                if (funcDictionary.TryGetValue(method, out CustomFunc func))
                {
                    object returnValue = func(objArgs.ToArray());
                    Console.WriteLine($"Returned value: {returnValue}");
                }
                else
                {
                    throw new Exception($"Method '{method}' not found in dictionary");
                }
            }
        }

        // Beispielaktion für ReadLine.
        static object ReadLineFunc(params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                Console.Write(parameters[0]);
                return Console.ReadLine();
            }
            return null;
        }
    }
}
 
 */