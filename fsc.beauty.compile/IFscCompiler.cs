using FSC.Dirty.Runtime.Template;

namespace FSC.Beauty
{
    public interface IFscCompiler : IFscRuntime
    {
        /// <summary>
        /// string defines the function name
        /// FscRuntimeTypes defines the runtime type (text, char, number, void)
        /// bool defines, if it is an array. If true, it is an array
        /// </summary>
        Dictionary<string, (FscRuntimeTypes, bool)> MethodReturnTypes { get; set; }
    }
}
