using System.Text;

namespace FSC.Beauty.Compile
{
    public class Compiler
    {
        private List<string> _code = new List<string>();

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
    }
}