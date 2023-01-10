using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace DotNetPowerExtensions.Analyzers;

sealed class Logger
{
    static Logger()
    {
        if (!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);

        // https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2019
#if NET7_0_OR_GREATER
        fileName ??= Path.Combine(folderName, @$"Log_{Environment.ProcessId}_{Environment.CurrentManagedThreadId}.txt");
#else
        fileName ??= Path.Combine(folderName, @$"Log_{Process.GetCurrentProcess().Id}_{Thread.CurrentThread.ManagedThreadId}.txt");
#endif
        lockObject ??= new object();
    }

    // The file name needs to based on the process and the thread so not have an error on locking...
    private static readonly string folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $@"DotNetPowerExtensionsAnalyzer\AnalyzerLogs");
    [ThreadStatic] private static readonly string fileName;
    [ThreadStatic] private static object lockObject;
    
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

    private static DateTimeFormatInfo Format = CultureInfo.InvariantCulture.DateTimeFormat;
    public static void LogError(Exception ex)
      => Log($"{DateTime.Now.ToString("yy-MM-dd hh:mm:ss", Format)} :: Error:\nType: {ex.GetType().Name}\nMessage: {ex.Message}\nStack: {ex.StackTrace}\nHasInner: {ex.InnerException != null}");

    public static void LogLineNumber([CallerLineNumber] int lineNumber = 0)
        => Log($"{DateTime.Now.ToString("yy-MM-dd hh:mm:ss", Format)} :: Line: {lineNumber.ToString(CultureInfo.InvariantCulture.NumberFormat)} {Environment.NewLine}");

    public static void LogInfo(string info)
        => Log($"{DateTime.Now.ToString("yy-MM-dd hh:mm:ss", Format)} :: Info: {info} {Environment.NewLine}");

}
