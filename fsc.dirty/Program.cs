using FSC.Dirty.Runtime;

namespace FSC.Dirty
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            string code = @"
var result void
var input text
extern input from ""ReadLine""
target here
extern result from ""WriteLine"", input
jump here
";

            DirtyRuntime dirtyRuntime = new DirtyRuntime();
            dirtyRuntime.AddScript(code);
            dirtyRuntime.Run();

            Console.ReadKey();
        }
    }
}