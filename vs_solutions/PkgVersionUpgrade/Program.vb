Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Xml
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
''' 3. 修正 nuget_release|x64 的产物输出路径（需显式加 --fix-output-path）
'''    把 RootNamespace 以 Microsoft.VisualBasic 起始的工程的
'''    nuget_release|x64 编译配置的 &lt;OutputPath&gt; 统一指向框架根下的 .nuget 目录，
'''    缺失该配置组的工程自动补建，并补齐 Configurations / Platforms 声明。
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
        ''' <summary>是否修正 nuget_release|x64 配置的产物输出路径</summary>
        Public Property FixOutputPath As Boolean
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
        Public Property OutputPath As OutputPathFixer.OutputPathResult
        Public Property [Error] As String
        Public Property Skipped As Boolean

        ''' <summary>输出路径修正是否产生了改动</summary>
        Public ReadOnly Property OutputPathChanged As Boolean
            Get
                Return OutputPath IsNot Nothing AndAlso OutputPath.Changed
            End Get
        End Property

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
        Dim nugetDir As String = Path.Combine(root, ".nuget")

        Console.WriteLine("sciBASIC# framework vbproj upgrade tool")
        Console.WriteLine($"framework root : {root}")
        Console.WriteLine($"timestamp      : {timestamp:yyyy-MM-dd HH:mm:ss}")
        Console.WriteLine($"nuget version  : {If(String.IsNullOrWhiteSpace(opts.Version), "<auto> (major.minor + timestamp)", opts.Version)}")
        Console.WriteLine($"assembly ver   : <auto> (major.minor + timestamp)")
        Console.WriteLine($"clean configs  : {If(opts.NoClean, "disabled", "enabled")}")
        Console.WriteLine($"output path    : {If(opts.FixOutputPath, nugetDir, "disabled")}")
        Console.WriteLine($"mode           : {If(opts.DryRun, "dry-run (no write)", "write")}")
        Console.WriteLine(New String("-"c, 96))

        Dim projects As List(Of String) = ScanProjects(root)
        Dim results As New List(Of ProjectResult)
        Dim watch As Stopwatch = Stopwatch.StartNew()

        For Each path As String In projects
            results.Add(ProcessProject(path, root, nugetDir, opts, timestamp))
        Next

        watch.Stop()

        Call PrintSummary(results, watch.Elapsed, opts.DryRun)
    End Sub

    ''' <summary>
    ''' 处理单个 vbproj 文件
    ''' </summary>
    Private Function ProcessProject(path As String,
                                     root As String,
                                     nugetDir As String,
                                     opts As CliOptions,
                                     timestamp As Date) As ProjectResult
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

            If opts.FixOutputPath AndAlso OutputPathFixer.IsTarget(model) Then
                result.OutputPath = OutputPathFixer.Apply(doc, ns, path, nugetDir)
            End If

            If Not opts.DryRun AndAlso (result.Changed OrElse result.RemovedConditions > 0 OrElse result.OutputPathChanged) Then
                Call SaveDocument(doc, path)
            End If
        Catch ex As Exception
            result.Error = ex.Message
        End Try

        Call ReportProject(result, display)

        Return result
    End Function

    ''' <summary>
    ''' 将修改后的就地写回 vbproj
    ''' </summary>
    ''' <remarks>
    ''' 框架内的 vbproj 统一是带 BOM 的 UTF-8 且没有 XML 声明，
    ''' 这里沿用原文件的 BOM 设定并且禁用自动缩进，保证 diff 只落在实际改动的那几行上。
    ''' </remarks>
    Private Sub SaveDocument(doc As XDocument, path As String)
        Dim settings As New XmlWriterSettings With {
            .Encoding = New UTF8Encoding(HasUtf8Bom(path)),
            .Indent = False,
            .OmitXmlDeclaration = doc.Declaration Is Nothing
        }

        Using writer As XmlWriter = XmlWriter.Create(path, settings)
            doc.Save(writer)
        End Using
    End Sub

    ''' <summary>判断文件开头是否存在 UTF-8 BOM</summary>
    Private Function HasUtf8Bom(path As String) As Boolean
        Try
            Using stream As New FileStream(path, FileMode.Open, FileAccess.Read)
                If stream.Length < 3 Then
                    Return False
                End If

                Dim head(2) As Byte

                stream.ReadExactly(head, 0, 3)

                Return head(0) = &HEF AndAlso head(1) = &HBB AndAlso head(2) = &HBF
            End Using
        Catch ex As Exception
            Return False
        End Try
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
        If result.OutputPathChanged Then
            Dim op = result.OutputPath

            If op.Created > 0 Then
                lines.Add($"added nuget_release|x64 config group, OutputPath -> {op.OutputPath}")
            End If
            If op.Updated > 0 Then
                lines.Add($"OutputPath -> {op.OutputPath} ({op.Updated} group(s))")
            End If
            If op.DeclarationsAdded > 0 Then
                lines.Add($"Configurations/Platforms declarations added: {op.DeclarationsAdded}")
            End If
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
        Dim outputUpdated As Integer = 0
        Dim outputCreated As Integer = 0
        Dim declarations As Integer = 0

        For Each r As ProjectResult In results
            If Not String.IsNullOrEmpty(r.Error) Then
                failed += 1
            ElseIf r.Skipped Then
                skipped += 1
            End If
            If r.Changed Then changed += 1
            removed += r.RemovedConditions
            warnings += r.Warnings

            If r.OutputPath IsNot Nothing Then
                outputUpdated += r.OutputPath.Updated
                outputCreated += r.OutputPath.Created
                declarations += r.OutputPath.DeclarationsAdded
            End If
        Next

        Console.WriteLine(New String("-"c, 96))
        Console.WriteLine($"scanned   : {scanned}")
        Console.WriteLine($"upgraded  : {changed}")
        Console.WriteLine($"cleaned   : {removed} obsolete config group(s) removed")

        If outputCreated + outputUpdated + declarations > 0 Then
            Console.WriteLine($"outputpath: {outputUpdated} group(s) updated, " &
                              $"{outputCreated} group(s) created, " &
                              $"{declarations} declaration(s) added")
        End If

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
                Case "--fix-output-path"
                    opts.FixOutputPath = True
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
        Console.WriteLine("      --fix-output-path 修正 nuget_release|x64 的产物输出路径。将 RootNamespace")
        Console.WriteLine("                        以 Microsoft.VisualBasic 起始的工程的该配置 <OutputPath>")
        Console.WriteLine("                        统一设为指向框架根下 .nuget 目录的相对路径；缺配置组的补建，")
        Console.WriteLine("                        并补齐 <Configurations> 中的 nuget_release 与 <Platforms> 中的 x64。")
        Console.WriteLine("  -h, --help            显示本帮助信息。")
        Console.WriteLine()
        Console.WriteLine("Notes:")
        Console.WriteLine("  * <AssemblyVersion> 与 <FileVersion> 恒由当前时间戳推算，不受 --version 影响；")
        Console.WriteLine("    nuget 版本号与 assembly version 在所有 SDK 工程中确保存在，file version 只更新已有值。")
        Console.WriteLine("  * --fix-output-path 默认关闭，需要显式指定才执行；带 $(TargetFramework) 的")
        Console.WriteLine("    nuget_release|net10.0|x64 变体配置组同样会被修正。")
        Console.WriteLine("  * 仅处理 Microsoft.NET.Sdk 风格工程，legacy 工程自动跳过；obj/bin 目录不参与扫描。")
        Console.WriteLine()
        Console.WriteLine("Examples:")
        Console.WriteLine("  PkgVersionUpgrade --dry-run")
        Console.WriteLine("  PkgVersionUpgrade -v 10.5.0.0")
        Console.WriteLine("  PkgVersionUpgrade --root G:\pixelArtist\src\framework -n")
        Console.WriteLine("  PkgVersionUpgrade --fix-output-path --dry-run")
    End Sub

End Module
