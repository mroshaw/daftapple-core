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

        public readonly UnityEvent<EditorLog> LogChangedEvent = new UnityEvent<EditorLog>();

        public EditorLog(bool logToConsole=true)
        {
            _logEntries = new List<string>();
            _logToConsole = logToConsole;
        }

        public void Log(string logEntry)
        {
            Log(LogLevel.Info, logEntry);
        }

        public void Log(LogLevel logLevel, string logEntry)
        {
            string fullLogText = "";

            switch (logLevel)
            {
                case LogLevel.Info:
                    fullLogText = $"Info: {logEntry}";
                    if (_logToConsole)
                    {
                        Debug.Log(logEntry);
                    }
                    break;
                case LogLevel.Warning:
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