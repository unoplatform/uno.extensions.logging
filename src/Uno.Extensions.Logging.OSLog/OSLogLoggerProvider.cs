﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Uno.Extensions.Logging
{
	/// <summary>
	/// A provider of <see cref="WebAssemblyConsoleLogger{T}"/> instances.
	/// </summary>
	public class OSLogLoggerProvider : ILoggerProvider
	{
		private readonly ConcurrentDictionary<string, OSLogLogger> _loggers;

		/// <summary>
		/// Creates an instance of <see cref="WebAssemblyConsoleLoggerProvider"/>.
		/// </summary>
		public OSLogLoggerProvider()
		{
			_loggers = new ConcurrentDictionary<string, OSLogLogger>();
		}

		/// <inheritdoc />
		public ILogger CreateLogger(string name)
		{
			return _loggers.GetOrAdd(name, loggerName => new OSLogLogger(name));
		}

		/// <inheritdoc />
		public void Dispose()
		{
		}
	}
}