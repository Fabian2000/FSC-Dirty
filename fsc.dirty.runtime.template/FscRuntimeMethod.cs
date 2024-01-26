namespace FSC.Dirty.Runtime.Template
{
    public delegate object? ExternFunction(params object[] parameters);

    public class CallMethodDictionary : Dictionary<string, ExternFunction>
    {
        private new KeyCollection Keys = new KeyCollection(new Dictionary<string, ExternFunction>());

        public KeyCollection Names { get => base.Keys; }
    }
}
