using FSC.Beauty.Compile;
using FSC.Dirty.Runtime;
using FSC.Dirty.Runtime.Template;

namespace FSC.Beauty.Runtime
{
    public class Runtime : IDisposable
    {
        private readonly Compiler _compiler;
        private readonly DirtyRuntime _dirtyRuntime;

        public Runtime()
        {
            _compiler = new Compiler();
            _dirtyRuntime = new DirtyRuntime();
        }

        public Runtime(IFscRuntime runtimeDefaults)
        {
            _compiler = new Compiler();
            _dirtyRuntime = new DirtyRuntime(runtimeDefaults);
        }

        public void AddScript(string code)
        {
            _compiler.Compile(code, out string outputCode);
            _dirtyRuntime.AddScript(outputCode);
        }

        public (FscRuntimeTypes Type, object? Value) GetVariable(string name)
        {
            return _dirtyRuntime.GetVariable(name);
        }

        public (FscRuntimeTypes Type, object[]? Value) GetArray(string name)
        {
            return _dirtyRuntime.GetArray(name);
        }

        public void Run()
        {
            _dirtyRuntime.Run();
        }

        public void CleanUp()
        {
            _dirtyRuntime.CleanUp();
        }

        public void Cancel()
        {
            _dirtyRuntime.Cancel();
        }

        public void Dispose()
        {
            CleanUp();
        }

        ~Runtime()
        {
            Dispose();
        }
    }
}
