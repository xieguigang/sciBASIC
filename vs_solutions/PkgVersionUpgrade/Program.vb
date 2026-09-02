Imports System.IO
Imports System.Xml.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.ProjectXml

''' <summary>
''' sciBASIC# 框架 vbproj 批量升级工具
''' </summary>
''' <remarks>
''' 扫描框架目录下所有的 Microsoft.NET.Sdk 风格工程，统一做两件事情：
'''
''' 1. 刷新版本号
'''    - ``&lt;Version&gt;``（nuget 程序包版本号）：命令行显式指定时用指定值，
'''      否则在现有值的 major.minor 基础上用当前时间戳推算出剩余数字；
'''    - ``&lt;AssemblyVersion&gt;`` / ``&lt;FileVersion&gt;``：恒由当前时间戳推算，
'''      不受命令行参数影响。
'''
''' 2. 精简编译配置
'''    移除 Condition 中引用了 $(TargetFramework) 但该框架已经不在
'''    TargetFramework / TargetFrameworks 声明集合中的 PropertyGroup。
'''
''' 出于数据安全考虑，写回的时候不使用 <see cref="VBProject.Generate"/> 重建文档，
''' 而是对原始 XML 做原地外科手术式修改，完整保留 EmbeddedResource / None / Content
''' 等节点以及原有的 XML 注释。
''' </remarks>
Module Program

    ''' <summary>命令行参数解析结果</summary>
    Private Class CliOptions
        ''' <summary>用户显式指定的 nuget 程序包版本号，未指定时为空</summary>
        Public Property Version As String
        ''' <summary>框架根目录，未指定时自动向上回溯查找</summary>
        Public Property Root As String
        ''' <summary>只打印将要发生的改动，不写盘</summary>
        Public Property DryRun As Boolean
        ''' <summary>只更新版本号，跳过过时编译配置的清理</summary>
        Public Property NoClean As Boolean
        ''' <summary>是否请求打印用法说明</summary>
        Public Property ShowHelp As Boolean
        ''' <summary>解析过程中出现的错误描述</summary>
        Public Property [Error] As String
    End Class

    ''' <summary>单个工程的处理结果</summary>
    Private Class ProjectResult
        Public Property FilePath As String
        Public Property Changes As VersionUpgrader.VersionChange()
        Public Property RemovedConditions As Integer
        Public Property Warnings As Integer
        Public Property [Error] As String
        Public Property Skipped As Boolean

        Public ReadOnly Property Changed As Boolean
            Get
                If Changes Is Nothing Then
                    Return False
                End If

                For Each change In Changes
                    If change.Changed Then
                        Return True
                    End If
                Next

                Return False
            End Get
        End Property
    End Class

    ''' <summary>扫描与遍历时需要跳过的目录名</summary>
    Private ReadOnly ExcludedDirectories As String() = {"obj", "bin", ".git", ".vs", "node_modules", "packages"}

    Public Sub Main(args As String())
        Dim opts As CliOptions = ParseCommandLine(args)

        If opts.ShowHelp Then
            Call PrintUsage()
            Return
        End If
        If Not String.IsNullOrEmpty(opts.Error) Then
            Console.WriteLine($"[error] {opts.Error}")
            Call PrintUsage()
            Environment.ExitCode = 1
            Return
        End If

        Dim root As String = FindFrameworkRoot(opts.Root)

        If Not Directory.Exists(root) Then
            Console.WriteLine($"[error] 框架根目录不存在: {root}")
            Environment.ExitCode = 1
            Return
        End If

        ' 整批处理共用同一个时间戳，保证这一批里面所有工程的 build/revision 段完全一致
        Dim timestamp As Date = Now

        Console.WriteLine("sciBASIC# framework vbproj upgrade tool")
        Console.WriteLine($"framework root : {root}")
        Console.WriteLine($"timestamp      : {timestamp:yyyy-MM-dd HH:mm:ss}")
        Console.WriteLine($"nuget version  : {If(String.IsNullOrWhiteSpace(opts.Version), "<auto> (major.minor + timestamp)", opts.Version)}")
        Console.WriteLine($"assembly ver   : <auto> (major.minor + timestamp)")
        Console.WriteLine($"clean configs  : {If(opts.NoClean, "disabled", "enabled")}")
        Console.WriteLine($"mode           : {If(opts.DryRun, "dry-run (no write)", "write")}")
        Console.WriteLine(New String("-"c, 96))

        Dim projects As List(Of String) = ScanProjects(root)
        Dim results As New List(Of ProjectResult)
        Dim watch As Stopwatch = Stopwatch.StartNew()

        For Each path As String In projects
            results.Add(ProcessProject(path, root, opts, timestamp))
        Next

        watch.Stop()

        Call PrintSummary(results, watch.Elapsed, opts.DryRun)
    End Sub

    ''' <summary>
    ''' 处理单个 vbproj 文件
    ''' </summary>
    Private Function ProcessProject(path As String, root As String, opts As CliOptions, timestamp As Date) As ProjectResult
        Dim result As New ProjectResult With {
            .FilePath = path,
            .Changes = New VersionUpgrader.VersionChange() {}
        }
        Dim display As String = RelativePath(path, root)

        Try
            ' 只用模型来读取元数据，写回一律走原始 XML
            Dim model As VBProject = VBProject.LoadProjectXml(path)

            If Not model.IsDotNetCoreSDK Then
                result.Skipped = True
                Return result
            End If

            Dim doc As XDocument = XDocument.Load(path, LoadOptions.PreserveWhitespace)
            Dim ns As XNamespace = If(doc.Root Is Nothing, XNamespace.None, doc.Root.Name.Namespace)
            Dim meta As VBProjectMetadata = model.Metadata

            Dim nuGetVersion As String = VersionUpgrader.ResolveNuGetVersion(opts.Version, If(model.NuGet?.Version, ""), timestamp)
            Dim assemblyVersion As String = VersionUpgrader.ResolveAssemblyVersion(OtherValue(meta, "AssemblyVersion"), timestamp)
            Dim fileVersion As String = VersionUpgrader.ResolveAssemblyVersion(OtherValue(meta, "FileVersion"), timestamp)

            result.Changes = VersionUpgrader.Apply(doc, ns, nuGetVersion, assemblyVersion, fileVersion, False)

            If Not opts.NoClean Then
                Dim cleaned = ConfigCleaner.Clean(doc, ns, ConfigCleaner.GetTargetFrameworkSet(model))

                result.RemovedConditions = cleaned.Removed
                result.Warnings = cleaned.Warnings
            End If

            If Not opts.DryRun AndAlso (result.Changed OrElse result.RemovedConditions > 0) Then
                doc.Save(path, SaveOptions.DisableFormatting)
            End If
        Catch ex As Exception
            result.Error = ex.Message
        End Try

        Call ReportProject(result, display)

        Return result
    End Function

    ''' <summary>打印单个工程的处理明细</summary>
    Private Sub ReportProject(result As ProjectResult, display As String)
        If Not String.IsNullOrEmpty(result.Error) Then
            Console.WriteLine($"  [error] {display}")
            Console.WriteLine($"          {result.Error}")
            Return
        End If
        If result.Skipped Then
            Return
        End If

        Dim lines As New List(Of String)

        For Each change In result.Changes
            If change.Changed Then
                lines.Add(change.ToString())
            End If
        Next
        If result.RemovedConditions > 0 Then
            lines.Add($"removed obsolete config groups: {result.RemovedConditions}")
        End If
        If result.Warnings > 0 Then
            lines.Add($"unresolved conditions kept: {result.Warnings}")
        End If

        If lines.Count = 0 Then
            Return
        End If

        Console.WriteLine($"  {display}")

        For Each line As String In lines
            Console.WriteLine($"      {line}")
        Next
    End Sub

    ''' <summary>打印汇总统计</summary>
    Private Sub PrintSummary(results As List(Of ProjectResult), elapsed As TimeSpan, dryRun As Boolean)
        Dim scanned As Integer = results.Count
        Dim skipped As Integer = 0
        Dim failed As Integer = 0
        Dim changed As Integer = 0
        Dim removed As Integer = 0
        Dim warnings As Integer = 0

        For Each r As ProjectResult In results
            If Not String.IsNullOrEmpty(r.Error) Then
                failed += 1
            ElseIf r.Skipped Then
                skipped += 1
            End If
            If r.Changed Then changed += 1
            removed += r.RemovedConditions
            warnings += r.Warnings
        Next

        Console.WriteLine(New String("-"c, 96))
        Console.WriteLine($"scanned   : {scanned}")
        Console.WriteLine($"upgraded  : {changed}")
        Console.WriteLine($"cleaned   : {removed} obsolete config group(s) removed")
        Console.WriteLine($"skipped   : {skipped} (non Microsoft.NET.Sdk project)")
        Console.WriteLine($"failed    : {failed}")
        Console.WriteLine($"warnings  : {warnings} unresolved condition group(s) kept")
        Console.WriteLine($"elapsed   : {elapsed.TotalSeconds:F2}s")

        If dryRun Then
            Console.WriteLine()
            Console.WriteLine("dry-run: no file was modified.")
        End If
    End Sub

    ''' <summary>
    ''' 递归扫描框架目录下的所有 vbproj 文件
    ''' </summary>
    Private Function ScanProjects(root As String) As List(Of String)
        Dim list As New List(Of String)

        Call CollectProjects(New DirectoryInfo(root), list)

        list.Sort(StringComparer.OrdinalIgnoreCase)

        Return list
    End Function

    Private Sub CollectProjects(dir As DirectoryInfo, list As List(Of String))
        If ExcludedDirectories.Contains(dir.Name, StringComparer.OrdinalIgnoreCase) Then
            Return
        End If

        Try
            For Each file As FileInfo In dir.EnumerateFiles("*.vbproj")
                list.Add(file.FullName)
            Next

            For Each subDir As DirectoryInfo In dir.EnumerateDirectories()
                Call CollectProjects(subDir, list)
            Next
        Catch ex As Exception
            Console.WriteLine($"  [warn] 无法访问目录 {dir.FullName}: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' 定位框架根目录：默认从程序集所在位置逐级向上回溯，
    ''' 命中第一个包含 Microsoft.VisualBasic.Core 子目录的目录即为框架根。
    ''' </summary>
    Private Function FindFrameworkRoot(explicit As String) As String
        If Not String.IsNullOrWhiteSpace(explicit) Then
            Return Path.GetFullPath(explicit)
        End If

        Dim dir As String = AppContext.BaseDirectory

        If String.IsNullOrEmpty(dir) Then
            dir = Directory.GetCurrentDirectory()
        End If

        Do While Not String.IsNullOrEmpty(dir)
            If Directory.Exists(Path.Combine(dir, "Microsoft.VisualBasic.Core")) Then
                Return dir
            End If

            dir = Path.GetDirectoryName(dir)
        Loop

        Return Directory.GetCurrentDirectory()
    End Function

    ''' <summary>
    ''' 从 VBProject 模型的 Other 字典中安全地取出一个属性值
    ''' </summary>
    Private Function OtherValue(meta As VBProjectMetadata, key As String) As String
        If meta Is Nothing OrElse meta.Other Is Nothing Then
            Return ""
        End If

        Dim value As String = Nothing

        If meta.Other.TryGetValue(key, value) Then
            Return If(value, "")
        End If

        Return ""
    End Function

    Private Function RelativePath(path As String, root As String) As String
        If path.StartsWith(root, StringComparison.OrdinalIgnoreCase) Then
            Return path.Substring(root.Length).TrimStart("\"c, "/"c)
        End If

        Return path
    End Function

    ''' <summary>
    ''' 解析命令行参数
    ''' </summary>
    Private Function ParseCommandLine(args As String()) As CliOptions
        Dim opts As New CliOptions()

        If args Is Nothing Then
            Return opts
        End If

        Dim i As Integer = 0

        While i < args.Length
            Dim arg As String = args(i)
            Dim value As String = Nothing

            ' 同时支持 --key=value 与 --key value 两种写法
            Dim eq As Integer = arg.IndexOf("="c)

            If eq > 0 Then
                value = arg.Substring(eq + 1)
                arg = arg.Substring(0, eq)
            End If

            Select Case arg.ToLower()
                Case "-h", "--help", "/?", "-?"
                    opts.ShowHelp = True
                    Return opts
                Case "-n", "--dry-run"
                    opts.DryRun = True
                Case "--no-clean"
                    opts.NoClean = True
                Case "-v", "--version", "-r", "--root"
                    If value Is Nothing Then
                        i += 1

                        If i >= args.Length Then
                            opts.Error = $"参数 {arg} 缺少取值"
                            Return opts
                        End If

                        value = args(i)
                    End If

                    If arg.ToLower() = "-v" OrElse arg.ToLower() = "--version" Then
                        opts.Version = value
                    Else
                        opts.Root = value
                    End If
                Case Else
                    opts.Error = $"无法识别的命令行参数: {args(i)}"
                    Return opts
            End Select

            i += 1
        End While

        Return opts
    End Function

    Private Sub PrintUsage()
        Console.WriteLine("Usage:")
        Console.WriteLine("  PkgVersionUpgrade [options]")
        Console.WriteLine()
        Console.WriteLine("Options:")
        Console.WriteLine("  -v, --version <ver>   nuget 程序包版本号。指定时直接写入 <Version>；")
        Console.WriteLine("                        未指定时在每个工程现有 <Version> 的 major.minor 基础上")
        Console.WriteLine("                        用当前时间戳推算出剩余数字（CalculateVersion）。")
        Console.WriteLine("  -r, --root <dir>      框架根目录。默认从程序所在目录向上回溯查找")
        Console.WriteLine("                        包含 Microsoft.VisualBasic.Core 的目录。")
        Console.WriteLine("  -n, --dry-run         只打印将要发生的改动，不写入文件。")
        Console.WriteLine("      --no-clean        只更新版本号，不清理过时的 TargetFramework 条件配置组。")
        Console.WriteLine("  -h, --help            显示本帮助信息。")
        Console.WriteLine()
        Console.WriteLine("Notes:")
        Console.WriteLine("  * <AssemblyVersion> 与 <FileVersion> 恒由当前时间戳推算，不受 --version 影响；")
        Console.WriteLine("    nuget 版本号与 assembly version 在所有 SDK 工程中确保存在，file version 只更新已有值。")
        Console.WriteLine("  * 仅处理 Microsoft.NET.Sdk 风格工程，legacy 工程自动跳过；obj/bin 目录不参与扫描。")
        Console.WriteLine()
        Console.WriteLine("Examples:")
        Console.WriteLine("  PkgVersionUpgrade --dry-run")
        Console.WriteLine("  PkgVersionUpgrade -v 10.5.0.0")
        Console.WriteLine("  PkgVersionUpgrade --root G:\pixelArtist\src\framework -n")
    End Sub

End Module
