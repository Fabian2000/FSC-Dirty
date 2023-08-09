namespace FSC.Dirty.Runtime.Template
{
    /// <summary>
    /// IFSCRuntime
    /// </summary>
    public interface IFscRuntime
    {
        /// <summary>
        /// Dirty has an own IFscRuntime class with some prepared methods. To load them next to this one, set the value to true
        /// </summary>
        bool UseDefaultTemplate { get; }

        /// <summary>
        /// Loads extern methods into the runtime to make it possible to call them from dirty script
        /// </summary>
        CallMethodDictionary ExternCallMethods { get; set; }
    }
}
