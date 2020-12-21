# Uno.Extensions.Logging

A set of logging extensions for `Microsoft.Extensions.Logging`.

## OSLogLoggerProvider
This logger is using the `OSLog` iOS API to log to the system log.

Usage example:
```csharp
.AddProvider(new Uno.Extensions.Logging.OSLogLoggerProvider());
```

## WebAssemblyConsoleLoggerProvider
This logger is using the browser's console javascript API

Usage example:
```csharp
.AddProvider(new Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
```