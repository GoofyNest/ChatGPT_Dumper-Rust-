using ChatGPT_Dumper;
using ChatGPT_Dumper.Classes;
using ChatGPT_Dumper.Engines;
using ChatGPT_Dumper.Utility;

partial class Program
{
    public static List<ClassNames> encryptedValues = [];
    public static string ItemContainer = "";

    static void Main()
    {
        Console.Title = "ChatGPT RustDumper";

        // Load Dump.cs
        var dump = LineScanner.LoadDumpLines();
        if (dump.Length == 0)
        {
            Utils.Out("Failed to load Dump.cs.");
            Console.ReadLine();
            return;
        }

        // Run engine
        var engine = new DumperEngine(dump);
        engine.Run();

        // Load script.json
        var script = LineScanner.LoadScriptLines();
        if (script.Length == 0)
        {
            Utils.Out("Failed to load script.json.");
            Console.ReadLine();
            return;
        }

        // Run engine
        var scriptengine = new ScriptEngine(script);
        scriptengine.Run();
        
        Console.WriteLine();

        Console.WriteLine("Program done");

        Console.ReadLine();
    }
}
