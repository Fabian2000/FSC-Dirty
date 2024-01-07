namespace FSC.Dirty.Runtime.Template
{
    /// <summary>
    /// RuntimeTypes - Dirty comes with some own and new types
    /// </summary>
    public enum FscRuntimeTypes
    {
        /// <summary>
        /// Empty return, nothing, no content
        /// </summary>
        Void,
        /// <summary>
        /// A text is a sequence of chars
        /// </summary>
        Text,
        /// <summary>
        /// Represents one symbol of a text
        /// </summary>
        Char,
        /// <summary>
        /// Represents any number like 1, 2, 3.5, 3555,5889, ...
        /// </summary>
        Number,
        /// <summary>
        /// Pointer to a variable
        /// </summary>>
        Pointer,
    }
}
