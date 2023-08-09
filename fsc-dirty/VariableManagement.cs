using System.Text.RegularExpressions;

using FSC.Dirty.Runtime.Template;

namespace FSC.Dirty.Runtime
{
    internal static class VariableManagement
    {
        internal static Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        internal static Dictionary<string, Array> Arrays { get; set; } = new Dictionary<string, Array>();

        internal static bool Exists(string name)
        {
            return Variables.ContainsKey(name) || Arrays.ContainsKey(name);
        }

        internal static object? ConvertToObject(FscRuntimeTypes type, string value)
        {
            switch (type)
            {
                case FscRuntimeTypes.Void:
                    return null;
                case FscRuntimeTypes.Text:
                    return Regex.Unescape(value.Trim('"'));
                case FscRuntimeTypes.Char:
                    return Convert.ToChar(value);
                case FscRuntimeTypes.Number: 
                    return Convert.ToDouble(value);
            }

            return null;
        }
    }

    internal class Variable
    {
        internal FscRuntimeTypes RuntimeType { get; set; }
        internal object? Value { get; set; }
    }

    internal class Array
    {
        internal FscRuntimeTypes RuntimeType { get; set; }
        internal object[]? Value { get; set; }
    }
}
