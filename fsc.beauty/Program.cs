using FSC.Beauty.Runtime;
using FSC.Dirty.Runtime.Template;
using System.Text;

namespace fsc.beauty
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            string code = @"
extern Title(""Example Script"")

extern WriteLine(""This script shows, how you can write plugins with FSC.Beauty in your .NET Application including the call of own functions"")
extern LongBeep(10)
extern Pause()

extern Exit(0)
";
            //Compiler compiler = new Compiler();
            //compiler.Compile(code, out string outputCode);
            //Console.WriteLine(outputCode);
            //if (Console.ReadKey(false).Key == ConsoleKey.Y)
            //{

            CustomFunctions customFunctions = new CustomFunctions();
            customFunctions.LoadFunctions();
            Runtime runtime = new Runtime(customFunctions);
            runtime.AddScript(code);
            runtime.Run();
            //}

#else
            Runtime runtime = new Runtime();
            runtime.AddScript(File.ReadAllText(args[0]));
            runtime.Run();
#endif
        }
    }
}

public class CustomFunctions : IFscRuntime
{
    public bool UseDefaultTemplate => true;

    public CallMethodDictionary ExternCallMethods { get; set; } = new CallMethodDictionary();

    public void LoadFunctions()
    {
        ExternCallMethods.Add("LongBeep", (object[] args) =>
        {
            int name = Convert.ToInt32(args[0]);
            StringBuilder beep = new StringBuilder("B");
            for (int i = 0; i < name; i++)
            {
                beep.Append("e");
            }
            beep.Append("p");
            Console.WriteLine(beep.ToString());
            return null;
        });
    }
}