using System.Text.RegularExpressions;

using FSC.Dirty.Runtime.Template;

namespace FSC.Dirty.Runtime
{
    public class DirtyRuntime
    {
        private readonly IFscRuntime _runtimeDefaults;
        private List<string> _code = new List<string>();

        public DirtyRuntime()
        {
            DefaultTemplate defaultTemplate = new DefaultTemplate();
            defaultTemplate.LoadMethods();
            _runtimeDefaults = defaultTemplate;
        }

        public DirtyRuntime(IFscRuntime runtimeDefaults)
        {
            _runtimeDefaults = runtimeDefaults;

            if (_runtimeDefaults.UseDefaultTemplate)
            {
                DefaultTemplate defaultTemplate = new DefaultTemplate();
                defaultTemplate.LoadMethods();

                foreach (KeyValuePair<string, ExternFunction> method in defaultTemplate.ExternCallMethods)
                {
                    if (!_runtimeDefaults.ExternCallMethods.ContainsKey(method.Key))
                    {
                        _runtimeDefaults.ExternCallMethods.Add(method.Key, method.Value);
                    }
                }
            }
        }

        public void AddScript(string code)
        {
            int count = -1;
            foreach (string line in code.Replace("\r", "").Split('\n'))
            {
                count++;

                if (string.IsNullOrWhiteSpace(line)) continue;
                else if (Regex.IsMatch(line, @"^(var|array|set|jump|target|extern|equals|greater|less|is|in)\s"))
                {
                    _code.Add(line);
                    continue;
                }
                throw new Exception($"Invalid starting keyword in line [{count}] => {line}");
            }

            ValidateCode();
        }

        private void ValidateCode()
        {
            int count = -1;
            foreach (string line in _code)
            {
                count++;

                if (line.StartsWith("var"))
                {
                    Match match = Regex.Match(line, KeywordRegex.VarKeyword);
                    if (match.Success) continue;
                    else
                    {
                        throw new Exception($"Error in line [{count}] => {line}");
                    }
                }
                else if (line.StartsWith("array"))
                {
                    Match match = Regex.Match(line, KeywordRegex.ArrayKeyword);
                    if (match.Success) continue;
                    else
                    {
                        throw new Exception($"Error in line [{count}] => {line}");
                    }
                }
                else if (line.StartsWith("set"))
                {
                    Match match = Regex.Match(line, KeywordRegex.Set1Keyword);
                    Match match2 = Regex.Match(line, KeywordRegex.Set2Keyword);
                    if (match.Success) continue;
                    else if (match2.Success) continue;
                    else
                    {
                        throw new Exception($"Error in line [{count}] => {line}");
                    }
                }
                else if (line.StartsWith("jump"))
                {
                    Match match = Regex.Match(line, KeywordRegex.JumpKeyword);
                    if (match.Success) continue;
                    else
                    {
                        throw new Exception($"Error in line [{count}] => {line}");
                    }
                }
                else if (line.StartsWith("target"))
                {
                    Match match = Regex.Match(line, KeywordRegex.TargetKeyword);
                    if (match.Success) continue;
                    else
                    {
                        throw new Exception($"Error in line [{count}] => {line}");
                    }
                }
                else if (line.StartsWith("extern"))
                {
                    Match match = Regex.Match(line, KeywordRegex.ExternKeyword);
                    if (match.Success) continue;
                    else
                    {
                        throw new Exception($"Error in line [{count}] => {line}");
                    }
                }
                else if (line.StartsWith("equals"))
                {
                    Match match = Regex.Match(line, KeywordRegex.ExternKeyword);
                    if (match.Success) continue;
                    else
                    {
                        throw new Exception($"Error in line [{count}] => {line}");
                    }
                }
                else if (line.StartsWith("greater"))
                {
                    Match match = Regex.Match(line, KeywordRegex.GreaterKeyword);
                    if (match.Success) continue;
                    else
                    {
                        throw new Exception($"Error in line [{count}] => {line}");
                    }
                }
                else if (line.StartsWith("less"))
                {
                    Match match = Regex.Match(line, KeywordRegex.LessKeyword);
                    if (match.Success) continue;
                    else
                    {
                        throw new Exception($"Error in line [{count}] => {line}");
                    }
                }
                else if (line.StartsWith("is"))
                {
                    Match match = Regex.Match(line, KeywordRegex.IsKeyword);
                    if (match.Success) continue;
                    else
                    {
                        throw new Exception($"Error in line [{count}] => {line}");
                    }
                }
                else if (line.StartsWith("in"))
                {
                    Match match = Regex.Match(line, KeywordRegex.InKeyword);
                    if (match.Success) continue;
                    else
                    {
                        throw new Exception($"Error in line [{count}] => {line}");
                    }
                }
            }
        }

        public  (FscRuntimeTypes Type, object? Value) GetVariable(string name)
        {
            return (VariableManagement.Variables[name].RuntimeType, VariableManagement.Variables[name].Value);
        }

        public (FscRuntimeTypes Type, object[]? Value) GetArray(string name)
        {
            return (VariableManagement.Arrays[name].RuntimeType, VariableManagement.Arrays[name].Value);
        }

        public void Run()
        {
            if (!_code.Any())
            {
                throw new Exception("Missing or empty script");
            }

            DirtyEngine engine = new DirtyEngine(_code, _runtimeDefaults);
            engine.Run();
        }
    }
}