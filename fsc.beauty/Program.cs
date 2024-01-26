using FSC.Beauty.Compile;

namespace fsc.beauty
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Compiler compiler = new Compiler();
            compiler.Compile(@"
text input = extern ReadLine()
extern Clear()
extern WriteLine(input)
", out string outputCode);
            Console.WriteLine(outputCode);
            Console.ReadKey(false);
        }
    }
}