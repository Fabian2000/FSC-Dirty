# FSC.Beauty NuGet Package README

Welcome to the FSC.Beauty! This language is designed to help you easily integrate scripting into your .NET applications. Whether you are looking to add scripting capabilities to your existing project or create new, dynamic applications, FSC.Beauty offers a simple yet powerful solution.

## Features
- **Integrate Scripting into .NET Applications**: Use FSC.Beauty to add scripting functionalities to your .NET applications.
- **Custom Functionality**: Easily extend the language with your own functions.
- **Cross-Platform Support**: Compatible with various .NET-supported platforms.
- **Simple Syntax**: FSC.Beauty's easy-to-understand syntax makes scripting accessible for all levels of developers.

## Getting Started

### Usage
Here's a basic example to get you started with using FSC.Beauty in your .NET application. This example demonstrates how to set up a simple script and integrate custom functions.

#### Example Script
```csharp
using FSC.Beauty.Runtime;
using FSC.Dirty.Runtime.Template;
using System.Text;

namespace fsc.beauty
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string code = @"
extern Title(""Example Script"")

extern WriteLine(""This script shows how you can write plugins with FSC.Beauty in your .NET Application including the call of own functions"")
extern LongBeep(10)
extern Pause()

extern Exit(0)
";
            CustomFunctions customFunctions = new CustomFunctions();
            customFunctions.LoadFunctions();
            Runtime runtime = new Runtime(customFunctions);
            runtime.AddScript(code);
            runtime.Run();
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
```

In this example, we define a simple script that uses both built-in and custom functions. The `CustomFunctions` class demonstrates how you can extend the scripting functionalities by adding your own methods.

### Documentation
For more detailed documentation on using FSC.Beauty, including syntax guides and advanced features, please visit the Wiki on GitHub.

## License
FSC.Beauty is released under MIT License, allowing for wide-ranging use and modification.

---

Thank you for choosing FSC.Beauty for your .NET scripting needs. We look forward to seeing the innovative ways you use this package in your applications!
