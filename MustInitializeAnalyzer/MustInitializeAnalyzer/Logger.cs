using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace MustInitializeAnalyzer
{
    class Logger
    {
        // The file name needs to based on the process and the thread so not have an error on locking...
        private static string FileName => $@"C:\Users\Desk\Desktop\AnalyzerLogs\Log_{Process.GetCurrentProcess().Id}_{Thread.CurrentThread.ManagedThreadId}.txt";
        public static void LogError(Exception ex) =>
            File.AppendAllText(FileName,
                $"{DateTime.Now.ToString("yy-MM-dd hh:mm:ss")} :: Error:\nType: {ex.GetType().Name}\nMessage: {ex.Message}\nStack: {ex.StackTrace}\nHasInner: {ex.InnerException != null}");
        public static void LogLineNumber([CallerLineNumber] int lineNumber = 0) =>
    File.AppendAllText(FileName, $"{DateTime.Now.ToString("yy-MM-dd hh:mm:ss")} :: Line: {lineNumber.ToString()} {Environment.NewLine}");

        public static void LogInfo(string info) =>
    File.AppendAllText(FileName, $"{DateTime.Now.ToString("yy-MM-dd hh:mm:ss")} :: Info: {info} {Environment.NewLine}");

    }
}
