using FSC.Dirty.Runtime.Template;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FSC.Dirty.Runtime
{
    internal class DefaultTemplate : IFscRuntime
    {
        public bool UseDefaultTemplate => false;

        public CallMethodDictionary ExternCallMethods { get; set; } = new CallMethodDictionary();

        internal void LoadMethods()
        {
            ExternCallMethods.Add("WriteLine", (object[] args) =>
            {
                object arg = args[0];
                Console.WriteLine((string)arg);
                return null;
            });

            ExternCallMethods.Add("Write", (object[] args) =>
            {
                object arg = args[0];
                Console.Write((string)arg);
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

            ExternCallMethods.Add("GetTitle", (object[] args) =>
            {
                return Console.Title;
            });

            ExternCallMethods.Add("ConsoleFgColor", (object[] args) =>
            {
                object arg = args[0];
                Console.ForegroundColor = (ConsoleColor)Convert.ToInt32(arg);
                return null;
            });

            ExternCallMethods.Add("ConsoleBgColor", (object[] args) =>
            {
                object arg = args[0];
                Console.BackgroundColor = (ConsoleColor)Convert.ToInt32(arg);
                return null;
            });

            ExternCallMethods.Add("ConsoleResetColor", (object[] args) =>
            {
                Console.ResetColor();
                return null;
            });

            ExternCallMethods.Add("Random", (object[] args) =>
            {
                Random random = new Random();
                return random.Next(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]));
            });

            ExternCallMethods.Add("RandomSeed", (object[] args) =>
            {
                Random random = new Random(Convert.ToInt32(args[0]));
                return Convert.ToDouble(random.Next(Convert.ToInt32(args[1]), Convert.ToInt32(args[2])));
            });

            ExternCallMethods.Add("SecureRandom", (object[] args) =>
            {
                int min = Convert.ToInt32(args[0]);
                int max = Convert.ToInt32(args[1]);

                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    byte[] randomNumber = new byte[4];
                    rng.GetBytes(randomNumber);
                    int value = BitConverter.ToInt32(randomNumber, 0);

                    return Math.Abs(value % (max - min)) + min;
                }
            });

            ExternCallMethods.Add("TextToNumber", (object[] args) =>
            {
                return double.Parse((string)args[0]);
            });

            ExternCallMethods.Add("NumberToText", (object[] args) =>
            {
                double number = Convert.ToDouble(args[0]);
                return number.ToString();
            });

            ExternCallMethods.Add("NumberToAscii", (object[] args) =>
            {
                double number = Convert.ToDouble(args[0]);
                return Convert.ToChar(number).ToString();
            });

            ExternCallMethods.Add("AsciiToNumber", (object[] args) =>
            {
                string text = (string)args[0];
                return Convert.ToDouble(Convert.ToChar(text));
            });

            ExternCallMethods.Add("CharFromText", (object[] args) =>
            {
                string text = (string)args[0];
                return text[Convert.ToInt32(args[1])].ToString();
            });

            ExternCallMethods.Add("StrConcat", (object[] args) =>
            {
                StringBuilder builder = new StringBuilder();
                foreach (object arg in args)
                {
                    builder.Append(arg.ToString());
                }
                return builder.ToString();
            });

            ExternCallMethods.Add("StrLength", (object[] args) =>
            {
                return ((string)args[0]).Length;
            });

            ExternCallMethods.Add("StrSubstring", (object[] args) =>
            {
                string str = (string)args[0];
                int start = Convert.ToInt32(args[1]);
                int length = Convert.ToInt32(args[2]);
                return str.Substring(start, length);
            });

            ExternCallMethods.Add("StrIndexOf", (object[] args) =>
            {
                string str = (string)args[0];
                string search = (string)args[1];
                return str.IndexOf(search);
            });

            ExternCallMethods.Add("StrLastIndexOf", (object[] args) =>
            {
                string str = (string)args[0];
                string search = (string)args[1];
                return str.LastIndexOf(search);
            });

            ExternCallMethods.Add("StrReplace", (object[] args) =>
            {
                string str = (string)args[0];
                string search = (string)args[1];
                string replace = (string)args[2];
                return str.Replace(search, replace);
            });

            ExternCallMethods.Add("StrToLower", (object[] args) =>
            {
                string str = (string)args[0];
                return str.ToLower();
            });

            ExternCallMethods.Add("StrToUpper", (object[] args) =>
            {
                string str = (string)args[0];
                return str.ToUpper();
            });

            ExternCallMethods.Add("StrTrim", (object[] args) =>
            {
                string str = (string)args[0];
                return str.Trim();
            });

            ExternCallMethods.Add("StrTrimStart", (object[] args) =>
            {
                string str = (string)args[0];
                return str.TrimStart();
            });

            ExternCallMethods.Add("StrTrimEnd", (object[] args) =>
            {
                string str = (string)args[0];
                return str.TrimEnd();
            });

            ExternCallMethods.Add("StrPadLeft", (object[] args) =>
            {
                string str = (string)args[0];
                int totalWidth = Convert.ToInt32(args[1]);
                char paddingChar = (char)Convert.ToInt32(args[2]);
                return str.PadLeft(totalWidth, paddingChar);
            });

            ExternCallMethods.Add("StrPadRight", (object[] args) =>
            {
                string str = (string)args[0];
                int totalWidth = Convert.ToInt32(args[1]);
                char paddingChar = (char)Convert.ToInt32(args[2]);
                return str.PadRight(totalWidth, paddingChar);
            });

            ExternCallMethods.Add("StrStartsWith", (object[] args) =>
            {
                string str = (string)args[0];
                string search = (string)args[1];
                return str.StartsWith(search) ? 1 : 0;
            });

            ExternCallMethods.Add("StrEndsWith", (object[] args) =>
            {
                string str = (string)args[0];
                string search = (string)args[1];
                return str.EndsWith(search) ? 1 : 0;
            });

            ExternCallMethods.Add("StrContains", (object[] args) =>
            {
                string str = (string)args[0];
                string search = (string)args[1];
                return str.Contains(search) ? 1 : 0;
            });

            ExternCallMethods.Add("StrSplit", (object[] args) =>
            {
                string str = (string)args[0];
                string separator = (string)args[1];
                return str.Split(separator)[Convert.ToInt32(args[2])];
            });

            ExternCallMethods.Add("StrJoin", (object[] args) =>
            {
                string separator = (string)args[0];
                string[] values = (string[])args[1];
                return string.Join(separator, values);
            });

            ExternCallMethods.Add("MathAdd", (object[] args) =>
            {
                double result = 0;
                foreach (object arg in args)
                {
                    result += Convert.ToDouble(arg);
                }
                return result;
            });

            ExternCallMethods.Add("MathSub", (object[] args) =>
            {
                double result = Convert.ToDouble(args[0]);
                for (int i = 1; i < args.Length; i++)
                {
                    result -= Convert.ToDouble(args[i]);
                }
                return result;
            });

            ExternCallMethods.Add("MathMul", (object[] args) =>
            {
                double result = 1;
                foreach (object arg in args)
                {
                    result *= Convert.ToDouble(arg);
                }
                return result;
            });

            ExternCallMethods.Add("MathDiv", (object[] args) =>
            {
                double result = Convert.ToDouble(args[0]);
                for (int i = 1; i < args.Length; i++)
                {
                    result /= Convert.ToDouble(args[i]);
                }
                return result;
            });

            ExternCallMethods.Add("MathMod", (object[] args) =>
            {
                double result = Convert.ToDouble(args[0]);
                for (int i = 1; i < args.Length; i++)
                {
                    result %= Convert.ToDouble(args[i]);
                }
                return result;
            });

            ExternCallMethods.Add("MathPow", (object[] args) =>
            {
                double result = Convert.ToDouble(args[0]);
                for (int i = 1; i < args.Length; i++)
                {
                    result = Math.Pow(result, Convert.ToDouble(args[i]));
                }
                return result;
            });

            ExternCallMethods.Add("MathSqrt", (object[] args) =>
            {
                return Math.Sqrt(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathAbs", (object[] args) =>
            {
                return Math.Abs(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathRound", (object[] args) =>
            {
                return Math.Round(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathCeil", (object[] args) =>
            {
                return Math.Ceiling(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathFloor", (object[] args) =>
            {
                return Math.Floor(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathMin", (object[] args) =>
            {
                double result = Convert.ToDouble(args[0]);
                for (int i = 1; i < args.Length; i++)
                {
                    result = Math.Min(result, Convert.ToDouble(args[i]));
                }
                return result;
            });

            ExternCallMethods.Add("MathMax", (object[] args) =>
            {
                double result = Convert.ToDouble(args[0]);
                for (int i = 1; i < args.Length; i++)
                {
                    result = Math.Max(result, Convert.ToDouble(args[i]));
                }
                return result;
            });

            ExternCallMethods.Add("MathSin", (object[] args) =>
            {
                return Math.Sin(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathCos", (object[] args) =>
            {
                return Math.Cos(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathTan", (object[] args) =>
            {
                return Math.Tan(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathAsin", (object[] args) =>
            {
                return Math.Asin(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathAcos", (object[] args) =>
            {
                return Math.Acos(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathAtan", (object[] args) =>
            {
                return Math.Atan(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathAtan2", (object[] args) =>
            {
                return Math.Atan2(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]));
            });

            ExternCallMethods.Add("MathLog", (object[] args) =>
            {
                return Math.Log(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathLog10", (object[] args) =>
            {
                return Math.Log10(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathLog2", (object[] args) =>
            {
                return Math.Log2(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathExp", (object[] args) =>
            {
                return Math.Exp(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathSign", (object[] args) =>
            {
                return Math.Sign(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathTruncate", (object[] args) =>
            {
                return Math.Truncate(Convert.ToDouble(args[0]));
            });

            ExternCallMethods.Add("MathE", (object[] args) =>
            {
                return Math.E;
            });

            ExternCallMethods.Add("MathPI", (object[] args) =>
            {
                return Math.PI;
            });

            ExternCallMethods.Add("MathTau", (object[] args) =>
            {
                return Math.Tau;
            });

            ExternCallMethods.Add("ReadFile", (object[] args) =>
            {
                string path = (string)args[0];
                return File.ReadAllText(path);
            });

            ExternCallMethods.Add("ReadFileLine", (object[] args) =>
            {
                string path = (string)args[0];
                return File.ReadAllLines(path)[Convert.ToInt32(args[1])];
            });

            ExternCallMethods.Add("WriteFile", (object[] args) =>
            {
                string path = (string)args[0];
                string content = (string)args[1];
                File.WriteAllText(path, content);
                return null;
            });

            ExternCallMethods.Add("AppendFile", (object[] args) =>
            {
                string path = (string)args[0];
                string content = (string)args[1];
                File.AppendAllText(path, content);
                return null;
            });

            ExternCallMethods.Add("DeleteFile", (object[] args) =>
            {
                string path = (string)args[0];
                File.Delete(path);
                return null;
            });

            ExternCallMethods.Add("FileExists", (object[] args) =>
            {
                string path = (string)args[0];
                return File.Exists(path) ? 1 : 0;
            });

            ExternCallMethods.Add("GetFileLength", (object[] args) =>
            {
                string path = (string)args[0];
                return Convert.ToDouble(new FileInfo(path).Length);
            });

            ExternCallMethods.Add("GetFileCreationTime", (object[] args) =>
            {
                string path = (string)args[0];
                return File.GetCreationTime(path).ToString();
            });

            ExternCallMethods.Add("GetFileLastAccessTime", (object[] args) =>
            {
                string path = (string)args[0];
                return File.GetLastAccessTime(path).ToString();
            });

            ExternCallMethods.Add("GetFileLastWriteTime", (object[] args) =>
            {
                string path = (string)args[0];
                return File.GetLastWriteTime(path).ToString();
            });

            ExternCallMethods.Add("FileCopy", (object[] args) =>
            {
                string source = (string)args[0];
                string destination = (string)args[1];
                File.Copy(source, destination);
                return null;
            });

            ExternCallMethods.Add("FileMove", (object[] args) =>
            {
                string source = (string)args[0];
                string destination = (string)args[1];
                File.Move(source, destination);
                return null;
            });

            ExternCallMethods.Add("CreateDirectory", (object[] args) =>
            {
                string path = (string)args[0];
                Directory.CreateDirectory(path);
                return null;
            });

            ExternCallMethods.Add("DeleteDirectory", (object[] args) =>
            {
                string path = (string)args[0];
                Directory.Delete(path);
                return null;
            });

            ExternCallMethods.Add("DirectoryExists", (object[] args) =>
            {
                string path = (string)args[0];
                return Directory.Exists(path) ? 1 : 0;
            });

            ExternCallMethods.Add("GetDirectoryCreationTime", (object[] args) =>
            {
                string path = (string)args[0];
                return Directory.GetCreationTime(path).ToString();
            });

            ExternCallMethods.Add("GetDirectoryLastAccessTime", (object[] args) =>
            {
                string path = (string)args[0];
                return Directory.GetLastAccessTime(path).ToString();
            });

            ExternCallMethods.Add("GetDirectoryLastWriteTime", (object[] args) =>
            {
                string path = (string)args[0];
                return Directory.GetLastWriteTime(path).ToString();
            });

            ExternCallMethods.Add("DirectoryCopy", (object[] args) =>
            {
                string source = (string)args[0];
                string destination = (string)args[1];
                DirectoryCopy(source, destination);
                return null;
            });

            ExternCallMethods.Add("DirectoryMove", (object[] args) =>
            {
                string source = (string)args[0];
                string destination = (string)args[1];
                Directory.Move(source, destination);
                return null;
            });

            ExternCallMethods.Add("GetFiles", (object[] args) =>
            {
                string path = (string)args[0];
                return Directory.GetFiles(path)[Convert.ToInt32(args[1])];
            });

            ExternCallMethods.Add("GetDirectories", (object[] args) =>
            {
                string path = (string)args[0];
                return Directory.GetDirectories(path)[Convert.ToInt32(args[1])];
            });

            ExternCallMethods.Add("GetFileName", (object[] args) =>
            {
                string path = (string)args[0];
                return Path.GetFileName(path);
            });

            ExternCallMethods.Add("GetFileNameWithoutExtension", (object[] args) =>
            {
                string path = (string)args[0];
                return Path.GetFileNameWithoutExtension(path);
            });

            ExternCallMethods.Add("GetExtension", (object[] args) =>
            {
                string path = (string)args[0];
                return Path.GetExtension(path);
            });

            ExternCallMethods.Add("GetDirectoryName", (object[] args) =>
            {
                string path = (string)args[0];
                return Path.GetDirectoryName(path);
            });

            ExternCallMethods.Add("GetFullPath", (object[] args) =>
            {
                string path = (string)args[0];
                return Path.GetFullPath(path);
            });

            ExternCallMethods.Add("GetRandomFileName", (object[] args) =>
            {
                return Path.GetRandomFileName();
            });

            ExternCallMethods.Add("GetTempPath", (object[] args) =>
            {
                return Path.GetTempPath();
            });

            ExternCallMethods.Add("GetTempFileName", (object[] args) =>
            {
                return Path.GetTempFileName();
            });

            ExternCallMethods.Add("GetPathRoot", (object[] args) =>
            {
                string path = (string)args[0];
                return Path.GetPathRoot(path);
            });

            ExternCallMethods.Add("ExtractZip", (object[] args) =>
            {
                string zipPath = (string)args[0];
                string extractPath = (string)args[1];
                ZipFile.ExtractToDirectory(zipPath, extractPath);
                return null;
            });

            ExternCallMethods.Add("CreateZip", (object[] args) =>
            {
                string zipPath = (string)args[0];
                string sourcePath = (string)args[1];
                ZipFile.CreateFromDirectory(sourcePath, zipPath);
                return null;
            });

            ExternCallMethods.Add("AddFileToZip", (object[] args) =>
            {
                string zipPath = (string)args[0];
                string sourcePath = (string)args[1];
                ZipArchive zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Update);
                zipArchive.CreateEntryFromFile(sourcePath, Path.GetFileName(sourcePath));
                zipArchive.Dispose();
                return null;
            });

            ExternCallMethods.Add("DateTimeNow", (object[] args) =>
            {
                return DateTime.Now.ToString();
            });

            ExternCallMethods.Add("DateTimeUtcNow", (object[] args) =>
            {
                return DateTime.UtcNow.ToString();
            });

            ExternCallMethods.Add("DateTimeFormat", (object[] args) =>
            {
                string format = (string)args[1];
                return DateTime.Parse((string)args[0]).ToString(format);
            });

            ExternCallMethods.Add("DateTimeAdd", (object[] args) =>
            {
                string format = (string)args[1];
                return DateTime.Parse((string)args[0]).Add(TimeSpan.Parse((string)args[1])).ToString(format);
            });

            ExternCallMethods.Add("DateTimeSub", (object[] args) =>
            {
                string format = (string)args[1];
                return DateTime.Parse((string)args[0]).Subtract(TimeSpan.Parse((string)args[1])).ToString(format);
            });

            ExternCallMethods.Add("DateTimeDiff", (object[] args) =>
            {
                string format = (string)args[2];
                return DateTime.Parse((string)args[0]).Subtract(DateTime.Parse((string)args[1])).ToString(format);
            });

            ExternCallMethods.Add("DateTimeTicks", (object[] args) =>
            {
                return Convert.ToDouble(DateTime.Parse((string)args[0]).Ticks);
            });

            ExternCallMethods.Add("Exit", (object[] args) =>
            {
                Environment.Exit(Convert.ToInt32(args[0]));
                return null;
            });

            ExternCallMethods.Add("DownloadFile", (object[] args) =>
            {
                string url = (string)args[0];
                string path = (string)args[1];

                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(url).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();
                    byte[] fileBytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    File.WriteAllBytes(path, fileBytes);
                    return "Download successful";
                }
            });

            ExternCallMethods.Add("DownloadString", (object[] args) =>
            {
                string url = (string)args[0];

                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(url).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();
                    return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            });

            ExternCallMethods.Add("UploadFile", (object[] args) =>
            {
                string url = (string)args[0];
                string path = (string)args[1];

                using (var client = new HttpClient())
                {
                    var fileBytes = File.ReadAllBytes(path);
                    var fileContent = new ByteArrayContent(fileBytes);
                    var response = client.PostAsync(url, fileContent).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();
                    return null;
                }
            });

            ExternCallMethods.Add("UploadString", (object[] args) =>
            {
                string url = (string)args[0];
                string content = (string)args[1];

                using (var client = new HttpClient())
                {
                    var stringContent = new StringContent(content);
                    var response = client.PostAsync(url, stringContent).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();
                    return null;
                }
            });

            ExternCallMethods.Add("RegMatch", (object[] args) =>
            {
                string input = (string)args[0];
                string pattern = (string)args[1];
                return Regex.IsMatch(input, pattern) ? 1 : 0;
            });

            ExternCallMethods.Add("RegReplace", (object[] args) =>
            {
                string input = (string)args[0];
                string pattern = (string)args[1];
                string replacement = (string)args[2];
                return Regex.Replace(input, pattern, replacement);
            });

            ExternCallMethods.Add("RegSplit", (object[] args) =>
            {
                string input = (string)args[0];
                string pattern = (string)args[1];
                return Regex.Split(input, pattern)[Convert.ToInt32(args[2])];
            });

            ExternCallMethods.Add("EnvGet", (object[] args) =>
            {
                string variable = (string)args[0];
                return Environment.GetEnvironmentVariable(variable);
            });

            ExternCallMethods.Add("EnvSet", (object[] args) =>
            {
                string variable = (string)args[0];
                string value = (string)args[1];
                Environment.SetEnvironmentVariable(variable, value);
                return null;
            });

            ExternCallMethods.Add("EnvRemove", (object[] args) =>
            {
                string variable = (string)args[0];
                Environment.SetEnvironmentVariable(variable, null);
                return null;
            });

            ExternCallMethods.Add("EnvGetAll", (object[] args) =>
            {
                var output = Environment.GetEnvironmentVariables();
                var result = output.Keys.Cast<string>().Select(key => $"{key}={output[key]}").ToArray();
                return string.Join(", ", result);
            });

            ExternCallMethods.Add("EnvGetAllUser", (object[] args) =>
            {
                var output = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);
                var result = output.Keys.Cast<string>().Select(key => $"{key}={output[key]}").ToArray();
                return string.Join(", ", result);
            });

            ExternCallMethods.Add("EnvGetAllMachine", (object[] args) =>
            {
                var output = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
                var result = output.Keys.Cast<string>().Select(key => $"{key}={output[key]}").ToArray();
                return string.Join(", ", result);
            });

            ExternCallMethods.Add("EnvGetAllProcess", (object[] args) =>
            {
                var output = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
                var result = output.Keys.Cast<string>().Select(key => $"{key}={output[key]}").ToArray();
                return string.Join(", ", result);
            });

            ExternCallMethods.Add("Sleep", (object[] args) =>
            {
                Thread.Sleep(Convert.ToInt32(args[0]));
                return null;
            });

            ExternCallMethods.Add("GetCurrentProcessId", (object[] args) =>
            {
                return Convert.ToDouble(Process.GetCurrentProcess().Id);
            });

            ExternCallMethods.Add("GetProcessByName", (object[] args) =>
            {
                string name = (string)args[0];
                return Process.GetProcessesByName(name)[Convert.ToInt32(args[1])].Id;
            });

            ExternCallMethods.Add("GetProcessName", (object[] args) =>
            {
                return Process.GetProcessById(Convert.ToInt32(args[0])).ProcessName;
            });

            ExternCallMethods.Add("GetProcessPath", (object[] args) =>
            {
                return Process.GetProcessById(Convert.ToInt32(args[0])).MainModule.FileName;
            });

            ExternCallMethods.Add("GetProcessPriority", (object[] args) =>
            {
                return Convert.ToDouble(Process.GetProcessById(Convert.ToInt32(args[0])).PriorityClass);
            });

            ExternCallMethods.Add("SetProcessPriority", (object[] args) =>
            {
                Process.GetProcessById(Convert.ToInt32(args[0])).PriorityClass = (ProcessPriorityClass)Convert.ToInt32(args[1]);
                return null;
            });

            ExternCallMethods.Add("GetProcessPriorityBoost", (object[] args) =>
            {
                return Process.GetProcessById(Convert.ToInt32(args[0])).PriorityBoostEnabled ? 1 : 0;
            });

            ExternCallMethods.Add("SetProcessPriorityBoost", (object[] args) =>
            {
                Process.GetProcessById(Convert.ToInt32(args[0])).PriorityBoostEnabled = Convert.ToInt32(args[1]) == 1;
                return null;
            });

            ExternCallMethods.Add("GetProcessSessionId", (object[] args) =>
            {
                return Convert.ToDouble(Process.GetProcessById(Convert.ToInt32(args[0])).SessionId);
            });

            ExternCallMethods.Add("GetProcessStartTime", (object[] args) =>
            {
                return Process.GetProcessById(Convert.ToInt32(args[0])).StartTime.ToString();
            });

            ExternCallMethods.Add("GetProcessTotalProcessorTime", (object[] args) =>
            {
                return Process.GetProcessById(Convert.ToInt32(args[0])).TotalProcessorTime.ToString();
            });

            ExternCallMethods.Add("GetProcessUserProcessorTime", (object[] args) =>
            {
                return Process.GetProcessById(Convert.ToInt32(args[0])).UserProcessorTime.ToString();
            });

            ExternCallMethods.Add("GetProcessPrivilegedProcessorTime", (object[] args) =>
            {
                return Process.GetProcessById(Convert.ToInt32(args[0])).PrivilegedProcessorTime.ToString();
            });

            ExternCallMethods.Add("GetProcessWorkingSet", (object[] args) =>
            {
                return Convert.ToDouble(Process.GetProcessById(Convert.ToInt32(args[0])).WorkingSet64);
            });

            ExternCallMethods.Add("GetProcessVirtualMemorySize", (object[] args) =>
            {
                return Convert.ToDouble(Process.GetProcessById(Convert.ToInt32(args[0])).VirtualMemorySize64);
            });

            ExternCallMethods.Add("GetProcessPrivateMemorySize", (object[] args) =>
            {
                return Convert.ToDouble(Process.GetProcessById(Convert.ToInt32(args[0])).PrivateMemorySize64);
            });

            ExternCallMethods.Add("GetProcessPeakWorkingSet", (object[] args) =>
            {
                return Convert.ToDouble(Process.GetProcessById(Convert.ToInt32(args[0])).PeakWorkingSet64);
            });

            ExternCallMethods.Add("GetProcessPeakVirtualMemorySize", (object[] args) =>
            {
                return Convert.ToDouble(Process.GetProcessById(Convert.ToInt32(args[0])).PeakVirtualMemorySize64);
            });

            ExternCallMethods.Add("GetProcessPeakPrivateMemorySize", (object[] args) =>
            {
                return Convert.ToDouble(Process.GetProcessById(Convert.ToInt32(args[0])).PeakPagedMemorySize64);
            });

            ExternCallMethods.Add("GetProcessThreads", (object[] args) =>
            {
                return Process.GetProcessById(Convert.ToInt32(args[0])).Threads[Convert.ToInt32(args[1])].Id;
            });

            ExternCallMethods.Add("GetProcessThreadsCount", (object[] args) =>
            {
                return Convert.ToDouble(Process.GetProcessById(Convert.ToInt32(args[0])).Threads.Count);
            });

            ExternCallMethods.Add("GetProcessModules", (object[] args) =>
            {
                return Process.GetProcessById(Convert.ToInt32(args[0])).Modules[Convert.ToInt32(args[1])].FileName;
            });

            ExternCallMethods.Add("GetProcessModulesCount", (object[] args) =>
            {
                return Convert.ToDouble(Process.GetProcessById(Convert.ToInt32(args[0])).Modules.Count);
            });

            ExternCallMethods.Add("GetProcessMainWindowTitle", (object[] args) =>
            {
                return Process.GetProcessById(Convert.ToInt32(args[0])).MainWindowTitle;
            });

            ExternCallMethods.Add("ProcessCloseMainWindow", (object[] args) =>
            {
                Process.GetProcessById(Convert.ToInt32(args[0])).CloseMainWindow();
                return null;
            });

            ExternCallMethods.Add("ProcessClose", (object[] args) =>
            {
                Process.GetProcessById(Convert.ToInt32(args[0])).Close();
                return null;
            });

            ExternCallMethods.Add("ProcessKill", (object[] args) =>
            {
                Process.GetProcessById(Convert.ToInt32(args[0])).Kill();
                return null;
            });

            ExternCallMethods.Add("ProcessRefresh", (object[] args) =>
            {
                Process.GetProcessById(Convert.ToInt32(args[0])).Refresh();
                return null;
            });

            ExternCallMethods.Add("ProcessStart", (object[] args) =>
            {
                string path = (string)args[0];
                Process.Start(path);
                return null;
            });

            ExternCallMethods.Add("ProcessStartWithArguments", (object[] args) =>
            {
                string path = (string)args[0];
                string arguments = (string)args[1];
                Process.Start(path, arguments);
                return null;
            });

            ExternCallMethods.Add("ProcessStartWithInfo", (object[] args) =>
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = (string)args[0];
                info.Arguments = (string)args[1];
                info.WorkingDirectory = (string)args[2];
                info.UseShellExecute = Convert.ToInt32(args[3]) == 1;
                info.CreateNoWindow = Convert.ToInt32(args[4]) == 1;
                info.RedirectStandardInput = Convert.ToInt32(args[5]) == 1;
                info.RedirectStandardOutput = Convert.ToInt32(args[6]) == 1;
                info.RedirectStandardError = Convert.ToInt32(args[7]) == 1;
                Process.Start(info);
                return null;
            });

            ExternCallMethods.Add("ProcessStartAsAdmin", (object[] args) =>
            {
                string path = (string)args[0];
                ProcessStartInfo processStartInfo = new ProcessStartInfo(path);
                processStartInfo.Verb = "runas";
                Process.Start(processStartInfo);
                return null;
            });

            ExternCallMethods.Add("InteropCall", (object[] args) =>
            {
                string assemblyName = (string)args[0];
                string typeName = (string)args[1];
                string methodName = (string)args[2];
                object[] methodArgs = args.Skip(3).ToArray();

                Assembly assembly = Assembly.Load(assemblyName);
                Type type = assembly.GetType(typeName);
                MethodInfo method = type.GetMethod(methodName);

                return method.Invoke(null, methodArgs);
            });
        }

        private static void DirectoryCopy(string source, string destination)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(source);
            DirectoryInfo destinationDirectory = new DirectoryInfo(destination);

            if (!destinationDirectory.Exists)
            {
                destinationDirectory.Create();
            }

            foreach (FileInfo file in sourceDirectory.GetFiles())
            {
                file.CopyTo(Path.Combine(destinationDirectory.FullName, file.Name), true);
            }

            foreach (DirectoryInfo directory in sourceDirectory.GetDirectories())
            {
                DirectoryCopy(directory.FullName, Path.Combine(destinationDirectory.FullName, directory.Name));
            }
        }
    }
}
