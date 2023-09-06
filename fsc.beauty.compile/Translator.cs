namespace FSC.Beauty.Compile
{
    internal class Translator
    {
        public List<string> Translate(List<string> beautyCode)
        {
            List<string> dirtyCode = new List<string>();

            foreach (var line in beautyCode)
            {
                if (line.Contains(":")) // Zuweisung oder Deklaration
                {
                    dirtyCode.Add(TranslateVariableDeclarationOrAssignment(line));
                }
                // Hier könnten weitere Übersetzungslogiken für andere Konstrukte hinzugefügt werden
            }

            return dirtyCode;
        }

        private string TranslateVariableDeclarationOrAssignment(string line)
        {
            string translatedLine = "";
            var parts = line.Split(new[] { ':', '=' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2) // Deklaration ohne Zuweisung
            {
                string varName = parts[0].Trim();
                string varType = parts[1].Trim();
                translatedLine = $"var {varName} {varType}";
            }
            else if (parts.Length == 3) // Deklaration mit Zuweisung
            {
                string varName = parts[0].Trim();
                string varType = parts[1].Trim();
                string varValue = parts[2].Trim();
                translatedLine = $"var {varName} {varType}\nset {varName} {varValue}";
            }

            return translatedLine;
        }

        private string TranslateArrayAssignment(string line)
        {
            string translatedLine = "";
            var parts = line.Split(new[] { '=', '[' }, StringSplitOptions.RemoveEmptyEntries);
            string arrayName = parts[0].Trim();
            translatedLine = $"set {arrayName} {parts[1].Trim(',', ' ', ']')}";

            for (int i = 2; i < parts.Length; i++)
            {
                translatedLine += $"\nset {arrayName} {parts[i].Trim(',', ' ', ']')}";
            }

            return translatedLine;
        }

        // Weitere Übersetzungsmethoden könnten hier hinzugefügt werden, z.B. für Funktionen, Kontrollstrukturen, etc.
    }
}
