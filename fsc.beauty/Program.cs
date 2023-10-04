using FSC.Beauty.Compile;

namespace fsc.beauty
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Compiler compiler = new Compiler();
            compiler.Compile(@"
text halloWelt = ""Hello, World!""
text[] halloWeltArray[5]
halloWeltArray[0] = ""Hello""
halloWeltArray[1] = ""World""
halloWeltArray[2] = ""!""
", out string outputCode);
            Console.WriteLine(outputCode);
            Console.ReadKey(false);
        }
    }
}