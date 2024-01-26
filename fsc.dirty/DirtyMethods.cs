using FSC.Dirty.Runtime.Template;

namespace FSC.Dirty
{
    internal class DirtyMethods : IFscRuntime
    {
        public bool UseDefaultTemplate => true;

        public CallMethodDictionary ExternCallMethods { get; set; } = new CallMethodDictionary();

        internal void LoadMethods()
        {
            ExternCallMethods.Add("WriteLine", (object[] args) =>
            {
                object arg = args[0];
                Console.WriteLine((string)arg);
                return null;
            });

            ExternCallMethods.Add("ReadLine", (object[] args) =>
            {
                return Console.ReadLine();
            });

            ExternCallMethods.Add("Clear", (object[] args) =>
            {
                Console.Clear();
                return null;
            });

            ExternCallMethods.Add("Pause", (object[] args) =>
            {
                Console.ReadKey(true);
                return null;
            });

            ExternCallMethods.Add("Title", (object[] args) =>
            {
                object arg = args[0];
                Console.Title = $"{arg}";
                return null;
            });
        }
    }
}
