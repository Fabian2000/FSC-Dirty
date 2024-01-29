using System.Dynamic;
using System.Text.RegularExpressions;

namespace fsc.beauty.compile
{
    internal static class ValidationRegex
    {
        internal static Regex Get(ValidationRegexTypes validationRegexTypes, long line)
        {
            return validationRegexTypes switch
            {
                ValidationRegexTypes.NewVariable => new Regex(@"^\s*(var|text|number|char)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(((?!\\)""(.*?[^\\])""|(\d+\.\d+)|(\d+)|[a-zA-Z_][a-zA-Z0-9_]+))\s*$"),
                ValidationRegexTypes.SetVariable => new Regex(@"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(((?!\\)""(.*?[^\\])""|(\d+\.\d+)|(\d+)|[a-zA-Z_][a-zA-Z0-9_]+))\s*$"),
                ValidationRegexTypes.NewArray => new Regex(@"^\s*(var|text|number|char)\[\]\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(\[\s*(.+)\s*\]\s*$)\s*$"),
                ValidationRegexTypes.SetArray => new Regex(@"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\[\]\s*=\s*(\[\s*(.+)\s*\]\s*$)\s*$"),
                ValidationRegexTypes.SetArrayIndex => new Regex(@"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\[(\d+)\]\s*=\s*(((?!\\)""(.*?[^\\])""|(\d+\.\d+)|(\d+)|[a-zA-Z_][a-zA-Z0-9_]+))\s*$"),
                ValidationRegexTypes.NewExternFunctionVariable => new Regex(@"^\s*(text|number|char|void)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*extern\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*)\)\s*$"),
                ValidationRegexTypes.SetExternFunctionVariable => new Regex(@"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*extern\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*)\)"),
                ValidationRegexTypes.CallExternFunction => new Regex(@"^\s*extern\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*)\)"),
                ValidationRegexTypes.GoTo => new Regex(@"^\s*goto\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*$"),
                ValidationRegexTypes.GoToTarget => new Regex(@"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*:\s*$"),
                ValidationRegexTypes.IfStatement => new Regex(@"^\s*if\s\s*(.*)\s*$"),
                ValidationRegexTypes.WhileStatement => new Regex(@"^\s*while\s\s*(.*)\s*$"),
                _ => throw new($"Invalid code in line {line}")
            };
        }

        internal static string[] SplitArguments(string arguments)
        {
            string[] result; // Array instead of list for performance reasons (no need to resize - List resize will allocate at any add() a new array)

            bool inString = false;
            char lastChar = '\0';
            ReadOnlySpan<char> args = arguments.AsSpan();

            // Handle case with no commas - single argument
            if (args.IndexOf(',') == -1)
            {
                result = [ args.ToString() ];
                return result;
            }

            int commaCount = 0;
            // Count commas in arg
            for (int i = 0; i < args.Length; i++)
            {
                char c = args[i];

                if (c == '"' && lastChar != '\\')
                {
                    inString = !inString;
                }

                if (c == ',' && !inString)
                {
                    commaCount++;
                }

                lastChar = c;
            }
            lastChar = '\0';

            result = new string[commaCount + 1];
            int lastCommaIndex = 0;

            // Split args at the correct position
            for (int i = 0; i < args.Length; i++)
            {
                char c = args[i];

                if (c == '"' && lastChar != '\\')
                {
                    inString = !inString;
                }

                if (c == ',' && !inString)
                {
                    ReadOnlySpan<char> trimmedArg = args.Slice(0, i).Trim();

                    result[lastCommaIndex] = trimmedArg.ToString();
                    lastCommaIndex++;
                    args = args.Slice(i + 1);
                    i = 0;
                }

                lastChar = c;
            }

            // Handle the last argument if it doesn't end with a comma
            if (lastCommaIndex < result.Length)
            {
                result[lastCommaIndex] = args.Trim().ToString();
            }

            return result;
        }

        internal enum ValidationRegexTypes
        {
            NewVariable,
            SetVariable,
            NewArray,
            SetArray,
            SetArrayIndex,
            NewExternFunctionVariable,
            SetExternFunctionVariable,
            CallExternFunction,
            GoTo,
            GoToTarget,
            IfStatement,
            WhileStatement
        }
    }
}
