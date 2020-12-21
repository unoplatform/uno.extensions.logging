// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Uno.Extensions.Logging.WebAssembly
{
	/// <summary>
	/// A provider of <see cref="WebAssemblyConsoleLogger"/> instances.
	/// </summary>
	public class WebAssemblyConsoleLoggerProvider : ILoggerProvider
	{
		private readonly ConcurrentDictionary<string, WebAssemblyConsoleLogger> _loggers;

		/// <summary>
		/// Creates an instance of <see cref="WebAssemblyConsoleLoggerProvider"/>.
		/// </summary>
		public WebAssemblyConsoleLoggerProvider()
		{
			_loggers = new ConcurrentDictionary<string, WebAssemblyConsoleLogger>();
		}

		/// <inheritdoc />
		public ILogger CreateLogger(string name)
		{
			return _loggers.GetOrAdd(name, loggerName => new WebAssemblyConsoleLogger(name));
		}

		/// <inheritdoc />
		public void Dispose()
		{
		}
	}
}