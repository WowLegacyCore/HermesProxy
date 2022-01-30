using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using ThreadingState = System.Threading.ThreadState;

namespace HermesProxy.Framework.Logging
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

        public static bool IsLogging;

        /// <summary>
        /// Start the logging Thread and take logs out of the <see cref="BlockingCollection{T}"/>
        /// </summary>
        public static void Start()
        {
            var logThread = new Thread(() =>
            {
                IsLogging = true;

                while (IsLogging)
                {
                    Thread.Sleep(1);

                    PrintMessageFromQueue();
                }
            });

            IsLogging = logThread.ThreadState == ThreadingState.Background;
            logThread.IsBackground = true;
            logThread.Start();
        }

        public static void PrintMessageFromQueue()
        {
            if (!logQueue.TryTake(out var msg))
                return;

            Console.Write($"{DateTime.Now:H:mm:ss} |");

            Console.ForegroundColor = LogToColorType[msg.Type].Color;
            Console.Write($"{LogToColorType[msg.Type].Type}");
            Console.ResetColor();

            Console.WriteLine($"| {msg.Message}");
        }

        public static void Print(LogType type, object text, [CallerMemberName] string method = "", [CallerFilePath] string path = "")
        {
            logQueue.Add((type, $"{SetCaller(method, path)} | {text}"));
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
    }
}
