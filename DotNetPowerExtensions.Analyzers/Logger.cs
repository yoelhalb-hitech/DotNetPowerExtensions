using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace DotNetPowerExtensions.Analyzers;

class Logger
{
    static Logger()
    {
        if (!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);
    }

    // The file name needs to based on the process and the thread so not have an error on locking...
    private static readonly string folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $@"DotNetPowerExtensionsAnalyzer\AnalyzerLogs");
    [ThreadStatic] private static readonly string fileName = Path.Combine(folderName, @$"Log_{Process.GetCurrentProcess().Id}_{Thread.CurrentThread.ManagedThreadId}.txt");
    [ThreadStatic] private static object lockObject = new object();
    private static void Log(string str)
    {
        try
        {
            lock (lockObject)
            {
                File.AppendAllText(fileName, str);
            }
        }
        catch (IOException)
        {
            // TODO...
        }
        catch { } // TODO...
    }
    public static void LogError(Exception ex)
      => Log($"{DateTime.Now.ToString("yy-MM-dd hh:mm:ss")} :: Error:\nType: {ex.GetType().Name}\nMessage: {ex.Message}\nStack: {ex.StackTrace}\nHasInner: {ex.InnerException != null}");

    public static void LogLineNumber([CallerLineNumber] int lineNumber = 0)
        => Log($"{DateTime.Now.ToString("yy-MM-dd hh:mm:ss")} :: Line: {lineNumber.ToString()} {Environment.NewLine}");

    public static void LogInfo(string info)
        => Log($"{DateTime.Now.ToString("yy-MM-dd hh:mm:ss")} :: Info: {info} {Environment.NewLine}");

}
