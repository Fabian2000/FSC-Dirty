using FSC.Dirty.Runtime;

namespace FSC.Dirty
{
    internal static class Program
    {
        static void Main(string[] args)
        {            
            FileInfo fileInfo = new FileInfo(args[0]);

            if (!fileInfo.Exists)
            {
                return;
            }

            DirtyMethods dirtyMethods = new DirtyMethods();
            dirtyMethods.LoadMethods();
            DirtyRuntime dirtyRuntime = new DirtyRuntime(dirtyMethods);
            dirtyRuntime.AddScript(File.ReadAllText(fileInfo.FullName));

            try
            {
                dirtyRuntime.Run();
            }
            catch (Exception ex)
            {
                ConsoleColor backupColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = backupColor;
                Console.ReadKey(true);
            }
        }
    }
}

