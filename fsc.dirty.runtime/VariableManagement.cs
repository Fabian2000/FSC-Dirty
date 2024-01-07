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

        internal static object? GetValueFromPointer(string name)
        {
            if (!Variables.ContainsKey(name))
            {
                return null;
            }

            Variable pvariable = Variables[name];

            if (pvariable.RuntimeType != FscRuntimeTypes.Pointer)
            {
                return null;
            }

            long pointer = Convert.ToInt64(pvariable.Value);

            if (pointer < 0)
            {
                return null;
            }

            if (Variables.FirstOrDefault(x => x.Value.Address == pointer).Value is Variable variable)
            {
                return variable.Value;
            }
            else if (Arrays.FirstOrDefault(x => x.Value.Address == pointer).Value is Array array)
            {
                return array.Value;
            }

            return null;
        }

        internal static void SetValueFromPointer(string name, object? value)
        {
            if (!Variables.ContainsKey(name))
            {
                return;
            }

            Variable pvariable = Variables[name];

            if (pvariable.RuntimeType != FscRuntimeTypes.Pointer)
            {
                return;
            }

            long pointer = Convert.ToInt64(pvariable.Value);

            if (pointer < 0)
            {
                return;
            }

            if (Variables.FirstOrDefault(x => x.Value.Address == pointer).Value is Variable variable)
            {
                variable.Value = value;
            }
            else if (Arrays.FirstOrDefault(x => x.Value.Address == pointer).Value is Array array)
            {
                array.Value = (object[]?)value;
            }
        }
    }

    internal class Variable
    {
        public Variable()
        {
            long latestAddress = VariableManagement.Variables.Count + VariableManagement.Arrays.Count;
            latestAddress++;
            Address = latestAddress;
        }

        internal FscRuntimeTypes RuntimeType { get; set; }
        internal object? Value { get; set; }
        internal long Address { get; init; }
    }

    internal class Array
    {
        public Array()
        {
            long latestAddress = VariableManagement.Variables.Count + VariableManagement.Arrays.Count;
            latestAddress++;
            Address = latestAddress;
        }

        internal FscRuntimeTypes RuntimeType { get; set; }
        internal object[]? Value { get; set; }
        internal long Address { get; init; }
    }
}
