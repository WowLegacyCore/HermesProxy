using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using ThreadingState = System.Threading.ThreadState;

namespace Framework.Logging
{
    public enum LogType
    {
        Server,
        Network,
        Debug,
        Error,
        Warn,
        Storage
    }

    public static class Log
    {
        static Dictionary<LogType, (ConsoleColor Color, string Type)> LogToColorType = new()
        {
            { LogType.Debug,    (ConsoleColor.DarkBlue, " Debug   ") },
            { LogType.Server,   (ConsoleColor.Blue,     " Server  ") },
            { LogType.Network,  (ConsoleColor.Green,    " Network ") },
            { LogType.Error,    (ConsoleColor.Red,      " Error   ") },
            { LogType.Warn,     (ConsoleColor.Yellow,   " Warning ") },
            { LogType.Storage,  (ConsoleColor.Cyan,     " Storage ") },
        }; 

        static BlockingCollection<(LogType Type, string Message)> logQueue = new();
        private static Thread? _logOutputThread = null;
        public static bool IsLogging => _logOutputThread != null && !logQueue.IsCompleted;

        /// <summary>
        /// Start the logging Thread and take logs out of the <see cref="BlockingCollection{T}"/>
        /// </summary>
        public static void Start()
        {
            if (_logOutputThread == null)
            {
                _logOutputThread = new Thread(() =>
                {
                    foreach (var msg in logQueue.GetConsumingEnumerable())
                    {
                        if (msg.Type == LogType.Debug && !Framework.Settings.DebugOutput)
                            continue;

                        Console.Write($"{DateTime.Now:H:mm:ss} |");

                        Console.ForegroundColor = LogToColorType[msg.Type].Color;
                        Console.Write($"{LogToColorType[msg.Type].Type}");
                        Console.ResetColor();

                        Console.WriteLine($"| {msg.Message}");
                    }
                });

                _logOutputThread.IsBackground = true;
                _logOutputThread.Start();
            }
        }

        public static void Print(LogType type, object text, [CallerMemberName] string method = "", [CallerFilePath] string path = "")
        {
            logQueue.Add((type, $"{SetCaller(method, path)} | {text}"));
        }

        public static void outException(Exception err, [CallerMemberName] string method = "", [CallerFilePath] string path = "")
        {
            Print(LogType.Error, err.ToString(), method, path);
        }

        private static string SetCaller(string method, string path)
        {
            string location = path;

            if (location.Contains("\\"))
            {
                string[] temp = location.Split('\\');
                location = temp[temp.Length - 1].Replace(".cs", "");
            }

            return location;
        }

        private static string NameOfCallingClass()
        {
            Type declaringType;

            var fullName = string.Empty;
            var skipFrames = 2;

            do
            {
                var method = new StackFrame(skipFrames, false).GetMethod();

                declaringType = method.DeclaringType;
                if (declaringType == null)
                    return method.Name;

                skipFrames++;
                fullName = declaringType.Name;
            }
            while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return fullName;
        }
    }
}
