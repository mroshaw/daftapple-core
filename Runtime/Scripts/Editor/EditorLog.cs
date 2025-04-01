using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DaftAppleGames.Editor
{
    public enum LogLevel { Info, Warning, Error }

    public class EditorLog
    {
        private readonly List<string> _logEntries;
        private readonly bool _logToConsole;

        public bool DetailedLogging { set; private get; }

        public readonly UnityEvent<EditorLog> LogChangedEvent = new UnityEvent<EditorLog>();

        public EditorLog(bool logToConsole, bool detailedLogging)
        {
            _logEntries = new List<string>();
            _logToConsole = logToConsole;
        }

        public void Log(string logEntry)
        {
            Log(LogLevel.Info, logEntry);
        }

        public void Log(LogLevel logLevel, string logEntry, bool forceLog = false)
        {
            string fullLogText = "";

            switch (logLevel)
            {
                case LogLevel.Info:
                    if (!DetailedLogging && !forceLog)
                    {
                        return;
                    }
                    fullLogText = $"Info: {logEntry}";
                    if (_logToConsole)
                    {
                        Debug.Log(logEntry);
                    }
                    break;
                case LogLevel.Warning:
                    if (!DetailedLogging && !forceLog)
                    {
                        return;
                    }
                    fullLogText = $"Warning: {logEntry}";
                    if (_logToConsole)
                    {
                        Debug.LogWarning(logEntry);
                    }
                    break;
                case LogLevel.Error:
                    fullLogText = $"Error: {logEntry}";
                    if (_logToConsole)
                    {
                        Debug.LogError(logEntry);
                    }
                    break;
            }

            _logEntries.Add(fullLogText);

            LogChangedEvent?.Invoke(this);
        }

        public void Clear()
        {
            _logEntries.Clear();
            LogChangedEvent?.Invoke(this);
        }

        public string GetLogAsString()
        {
            string logString = "";
            foreach (string logEntry in _logEntries)
            {
                logString += logEntry + "\n";
            }
            return logString.TrimEnd('\n');
        }
    }
}