﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Based on https://github.com/dotnet/aspnetcore/commit/2ceca7fb89a4021166b32f18612bc490d3146fe2

using System;
using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using Uno.Foundation;

#if NET7_0_OR_GREATER
using System.Runtime.InteropServices.JavaScript;
#endif

namespace Uno.Extensions.Logging.WebAssembly
{
    internal partial class WebAssemblyConsoleLogger : ILogger<object>, ILogger
    {
        private static readonly string _loglevelPadding = ": ";
        private static readonly string _messagePadding;
        private static readonly string _newLineWithMessagePadding;
        private static readonly StringBuilder _logBuilder = new();

        private readonly string _name;

        static WebAssemblyConsoleLogger()
        {
            var logLevelString = GetLogLevelString(LogLevel.Information);
            _messagePadding = new string(' ', logLevelString.Length + _loglevelPadding.Length);
            _newLineWithMessagePadding = Environment.NewLine + _messagePadding;
        }

        public WebAssemblyConsoleLogger()
            : this(string.Empty)
        {
        }

        public WebAssemblyConsoleLogger(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoOpDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, _name, eventId.Id, message, exception);
            }
        }

        private void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            lock (_logBuilder)
            {
                try
                {
                    CreateDefaultLogMessage(_logBuilder, logLevel, logName, eventId, message, exception);
                    var formattedMessage = _logBuilder.ToString();

                    switch (logLevel)
                    {
                        case LogLevel.Trace:
                        case LogLevel.Debug:
                            // Although https://console.spec.whatwg.org/#loglevel-severity claims that
                            // "console.debug" and "console.log" are synonyms, that doesn't match the
                            // behavior of browsers in the real world. Chromium only displays "debug"
                            // messages if you enable "Verbose" in the filter dropdown (which is off
                            // by default). As such "console.debug" is the best choice for messages
                            // with a lower severity level than "Information".
                            NativeMethods.LogDebug(formattedMessage);
                            break;
                        case LogLevel.Information:
                            NativeMethods.LogInfo(formattedMessage);
                            break;
                        case LogLevel.Warning:
                            NativeMethods.LogWarning(formattedMessage);
                            break;
                        case LogLevel.Error:
                            NativeMethods.LogError(formattedMessage);
                            break;
                        case LogLevel.Critical:
                            // Writing to Console.Error is even more severe than calling console.error,
                            // because it also causes the error UI (gold bar) to appear.
                            Console.Error.WriteLine(formattedMessage);
                            break;
                        default: // LogLevel.None or invalid enum values
                            Console.WriteLine(formattedMessage);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to log \"{message}\": {ex}");
                }
                finally
                {
                    _logBuilder.Clear();
                }
            }
        }

        private void CreateDefaultLogMessage(StringBuilder logBuilder, LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            logBuilder.Append(GetLogLevelString(logLevel));
            logBuilder.Append(_loglevelPadding);
            logBuilder.Append(logName);
            logBuilder.Append("[");
            logBuilder.Append(eventId);
            logBuilder.Append("]");

            if (!string.IsNullOrEmpty(message))
            {
                // message
                logBuilder.AppendLine();
                logBuilder.Append(_messagePadding);

                var len = logBuilder.Length;
                logBuilder.Append(message);
                logBuilder.Replace(Environment.NewLine, _newLineWithMessagePadding, len, message.Length);
            }

            // Example:
            // System.InvalidOperationException
            //    at Namespace.Class.Function() in File:line X
            if (exception != null)
            {
                // exception message
                logBuilder.AppendLine();
                logBuilder.Append(exception.ToString());
            }
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "trce";
                case LogLevel.Debug:
                    return "dbug";
                case LogLevel.Information:
                    return "info";
                case LogLevel.Warning:
                    return "warn";
                case LogLevel.Error:
                    return "fail";
                case LogLevel.Critical:
                    return "crit";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        private class NoOpDisposable : IDisposable
        {
            public static NoOpDisposable Instance = new();

            public void Dispose() { }
        }

        private static partial class NativeMethods
        {
            private static void Invoke(string method, string message)
                => WebAssemblyRuntime.InvokeJS($"{method}(\"{WebAssemblyRuntime.EscapeJs(message)}\")");

#if NET7_0_OR_GREATER
            [JSImport("globalThis.console.debug")]
#endif
            public static partial void LogDebug(string message);

#if !NET7_0_OR_GREATER
            public static partial void LogDebug(string message)
                => Invoke("console.debug", message);
#endif

#if NET7_0_OR_GREATER
            [JSImport("globalThis.console.info")]
#endif
            public static partial void LogInfo(string message);

#if !NET7_0_OR_GREATER
            public static partial void LogInfo(string message)
                => Invoke("console.info", message);
#endif

#if NET7_0_OR_GREATER
            [JSImport("globalThis.console.warn")]
#endif
            public static partial void LogWarning(string message);

#if !NET7_0_OR_GREATER
            public static partial void LogWarning(string message)
                => Invoke("console.warn", message);
#endif

#if NET7_0_OR_GREATER
            [JSImport("globalThis.console.error")]
#endif
            public static partial void LogError(string message);

#if !NET7_0_OR_GREATER
            public static partial void LogError(string message)
                => Invoke("console.error", message);
#endif
        }
    }
}
