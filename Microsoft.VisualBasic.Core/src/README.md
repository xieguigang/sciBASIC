# Visual Basic Runtime Core Library For Scientific Computing

Runtime core of sciBASIC#: the shared application services, LINQ and data extensions, reflection/native interop, text handling and command line framework that every other sciBASIC# package builds on.

## Overview
- Application services: `App` runtime/process context, logging, terminal IO with Markdown rendering, console tables, progress bars and Tqdm-style progress.
- Command line framework: attribute-driven CLI parsing, POSIX style arguments, interop service and manual pages.
- Data extensions: LINQ and collection helpers, named-value / data-source models, tabular (TSV/DataFrame) readers and writers, XML and JSON serialization.
- Interop and reflection: dynamic native library loading, unmanaged memory helpers, emit/marshal and delegate factories.
- Math, text and network helpers: statistics and SIMD-aware math extensions, string similarity and text parsing, HTTP/TCP clients.

## Key Types
- `Microsoft.VisualBasic.ApplicationServices.App` — process/assembly/home directory context, stdout-stderr redirection and CPU/memory info.
- `Microsoft.VisualBasic.ApplicationServices.Debugging.Logging.LogFile` — buffered, leveled text log file with `WriteLine`, `log`, `LogException` and `Save`.
- `Microsoft.VisualBasic.CommandLine.CommandLine` — tokenized command line model plus the CLI parsing entry points.
- `Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue(Of T)` — the generic key/value data model used across the framework.
- `Microsoft.VisualBasic.ApplicationServices.DynamicInterop.UnmanagedDll` — proxy for loading and calling unmanaged dynamic libraries.
- `Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.ConsoleTableBuilder` — fluent builder for formatted console tables.
- `Microsoft.VisualBasic.Text.ASCII` — ASCII/encoding helpers for text and byte buffers.
- `Microsoft.VisualBasic.Math.RandomExtensions` — random number generators and sampling helpers.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging

Dim log As New LogFile("./scibasic.log", append:=True)

Call log.WriteLine($"app={App.AssemblyName}, home={App.HOME}, cores={App.CPUCoreNumbers}")
Call log.Save()
```

## Package
- Assembly: `Microsoft.VisualBasic.Runtime`
- TargetFramework: `net10.0`
- Tags: `scibasic;runtime;linq;reflection;application-services`

## License
GPL-3.0-or-later
