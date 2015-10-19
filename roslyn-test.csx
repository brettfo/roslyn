using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

Dictionary<string, string> Options { get; } = new Dictionary<string, string>();

Main();

//

void Main()
{
    SetDefaultOptions();
    ParseOptions(Args);
    var testBinaries = GetAllTestBinaries(Options["bindir"]);
    var arguments = string.Join(" ", testBinaries.Select(b => "\"" + b + "\""));
    arguments += $@" -xml ""results.xml"" -noshadow -parallel all";
    
    Console.WriteLine("Starting xunit");
    var xunit = Process.Start(XUnitPath, arguments);
    xunit.WaitForExit();
}

bool IsX64 => bool.Parse(Options["x64"]);
string PackagesRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
string XUnitPath => Path.Combine(PackagesRoot, "xunit.runner.console", "2.1.0", "tools", IsX64 ? "xunit.console.exe" : "xunit.console.x86.exe");

void SetDefaultOptions()
{
    Options["x64"] = true.ToString();
}

void ParseOptions(IEnumerable<string> args)
{
    foreach (var arg in args)
    {
        if (arg.StartsWith("-"))
        {
            var parts = arg.Split(":".ToCharArray(), 2);
            if (parts.Length == 2)
            {
                Options[parts[0].Substring(1).ToLower()] = parts[1];
            }
            else
            {
                throw new Exception("Unexpected argument: " + arg);
            }
        }
    }
}

IEnumerable<string> GetAllTestBinaries(string binaryPath)
{
    var files = Directory.EnumerateFiles(binaryPath, "*UnitTests*.dll", SearchOption.AllDirectories);
    return files;
}
