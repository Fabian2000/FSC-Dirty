namespace FSC.Dirty.Runtime
{
    internal static class KeywordRegex
    {
        internal const string VarKeyword = /* lang=regex */ @"^var\s[a-zA-Z_][a-zA-Z0-9_]*\s(void|text|char|number|bytes)$";
        internal const string ArrayKeyword = /* lang=regex */ @"^array\s[a-zA-Z_][a-zA-Z0-9_]*\s(void|text|char|number|bytes)\s\d*$";
        internal const string SetArrayKeyword = /* lang=regex */ @"^set\s[a-zA-Z_][a-zA-Z0-9_]+\sat\s\d+\s((?!\\)""(.*?[^\\])""|(\d+\.\d+)|(\d+)|[a-zA-Z_][a-zA-Z0-9_]+)$";
        internal const string SetVarKeyword = /* lang=regex */ @"^set\s[a-zA-Z_][a-zA-Z0-9_]+\s((?!\\)""(.*?[^\\])""|(\d+\.\d+)|(\d+)|(\*?)[a-zA-Z_][a-zA-Z0-9_]+)$";
        internal const string SetPointerKeyword = /* lang=regex */ @"^set\s[a-zA-Z_][a-zA-Z0-9_]+\s&[a-zA-Z_][a-zA-Z0-9_]+$";
        internal const string JumpKeyword = /* lang=regex */ @"^jump\s[a-zA-Z_][a-zA-Z0-9_]+$";
        internal const string TargetKeyword = /* lang=regex */ @"^target\s[a-zA-Z_][a-zA-Z0-9_]+$";
        internal const string ExternKeyword = /* lang=regex */ @"^extern\s[a-zA-Z_][a-zA-Z0-9_]+\sfrom\s""[a-zA-Z_][a-zA-Z0-9_]+""(,\s[a-zA-Z_][a-zA-Z0-9_]+)*$";
        internal const string EqualsKeyword = /* lang=regex */ @"equals\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+$";
        internal const string GreaterKeyword = /* lang=regex */ @"greater\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+$";
        internal const string LessKeyword = /* lang=regex */ @"less\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+$";
        internal const string AndKeyword = /* lang=regex */ @"and\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+$";
        internal const string OrKeyword = /* lang=regex */ @"or\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+$";
        internal const string IsKeyword = /* lang=regex */ @"is\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+$";
        internal const string InKeyword = /* lang=regex */ @"in\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+\s[a-zA-Z_][a-zA-Z0-9_]+$";
        internal const string DeleteKeyword = /* lang=regex */ @"delete\s[a-zA-Z_][a-zA-Z0-9_]+$";
    }
}
