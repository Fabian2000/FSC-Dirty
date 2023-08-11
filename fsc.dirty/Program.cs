using FSC.Dirty.Runtime;

namespace FSC.Dirty
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            string code = @"
var result void
var input text
var isIn number
var variable text
array testArray text 1
set input ""Please enter anything:""
extern result from ""WriteLine"", input
extern input from ""ReadLine""
target here
extern result from ""WriteLine"", input
set testArray at 0 ""stop""
set variable input
in isIn input testArray
is isIn jumpOut
jump here
target jumpOut
equals isIn variable input
var number number
var number1 number
var number2 number
set number1 200
set number2 2
greater number number1 number2
";

            string code2 = @"
var result void
var info text
var language text
var compare number
var english text
var german text
var french text

target start
set info ""Please enter one of these languages [english|german|french]:""
extern result from ""WriteLine"", info

extern language from ""ReadLine""

set english ""english""
set german ""german""
set french ""french""

equals compare language english
is compare jumpEnglish

equals compare language german
is compare jumpGerman

equals compare language french
is compare jumpFrench

jump noMatch

target jumpEnglish
set info ""Hello World""
extern result from ""WriteLine"", info
jump end

target jumpGerman
set info ""Hallo Welt""
extern result from ""WriteLine"", info
jump end

target jumpFrench
set info ""Bonjour le monde""
extern result from ""WriteLine"", info
jump end

target noMatch
set info ""No language match found - retry""
extern result from ""WriteLine"", info
jump start

target end
";
            
            DirtyRuntime dirtyRuntime = new DirtyRuntime();
            dirtyRuntime.AddScript(code2);
            dirtyRuntime.Run();
            Console.ReadKey();
        }
    }
}

