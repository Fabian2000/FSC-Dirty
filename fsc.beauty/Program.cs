using FSC.Beauty.Compile;
using FSC.Beauty.Runtime;

namespace fsc.beauty
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            string code = @"
extern Title(""ProcessKiller"")

extern WriteLine(""Welcome in ProcessKiller"")
extern Write(""Please enter the process name you want to get: "")

text processName = extern ReadLine()

number processId = extern GetProcessByName(processName, 0)

extern WriteLine("" ---------------- "")

text question = ""Do you want to close this process? ""
question = extern StrConcat(question, processName)

extern WriteLine(question)

text answer = extern ReadLine()

var yes = ""yes""
if answer == yes
	extern ProcessKill(processId)
	extern WriteLine(""Done!"")
	extern Sleep(2000)

extern Exit(0)
";
            Compiler compiler = new Compiler();
            compiler.Compile(code, out string outputCode);
            Console.WriteLine(outputCode);
            if (Console.ReadKey(false).Key == ConsoleKey.Y)
            {
                Runtime runtime = new Runtime();
                runtime.AddScript(code);
                runtime.Run();
            }

#else
            Runtime runtime = new Runtime();
            runtime.AddScript(File.ReadAllText(args[0]));
            runtime.Run();
#endif
        }
    }
}