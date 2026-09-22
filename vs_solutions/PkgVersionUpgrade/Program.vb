#Region "Microsoft.VisualBasic::d9c4ad3463a1b85a78aea75f9206ef84, vs_solutions\PkgVersionUpgrade\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 463
    '    Code Lines: 330 (71.27%)
    ' Comment Lines: 63 (13.61%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 70 (15.12%)
    '     File Size: 20.55 KB


    ' Module Program
    ' 
    '     Function: EnumerateProjects, HasUtf8Bom, OtherValue, ParseCommandLine, ProcessProject
    '               RelativePath, Validate
    ' 
    '     Sub: Main, PrintSummary, PrintUsage, ReportProject, SaveDocument
    '     Class ProjectResult
    ' 
    '         Properties: [Error], Changed, Changes, FilePath, OutputPath
    '                     OutputPathChanged, RemovedConditions, Skipped, Warnings
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Xml
Imports System.Xml.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.ProjectXml
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.sln
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.sln.File
Imports Microsoft.VisualBasic.CommandLine

''' <summary>
''' sciBASIC# 框架 vbproj 批量升级工具
''' </summary>
''' <remarks>
''' 工具以一个 slnx 解决方案文件为输入：解析该解决方案并枚举其中声明的 vbproj，
''' 再按命令行给定的命名空间前缀（对 RootNamespace 做大小写不敏感的前缀匹配）过滤，
''' 仅对过滤后命中的目标工程统一做两件事：
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
''' 3. 修正 nuget_release|x64 的产物输出路径（需显式加 --fix-output-path，并配合 --output）
'''    把命中工程的 nuget_release|x64 编译配置的 &lt;OutputPath&gt; 统一指向 --output
'''    所指定的输出文件夹（化为相对于该 vbproj 的相对路径），缺失该配置组的工程自动补建，
'''    并补齐 Configurations / Platforms 声明。
'''
''' 出于数据安全考虑，写回的时候不使用 <see cref="VBProject.Generate"/> 重建文档，
''' 而是对原始 XML 做原地外科手术式修改，完整保留 EmbeddedResource / None / Content
''' 等节点以及原有的 XML 注释。
''' </remarks>
Module Program

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

        Dim sln As Solution = Solution.Load(opts.Slnx)

        If sln Is Nothing Then
            Console.WriteLine($"[error] 无法解析 slnx 解决方案文件: {opts.Slnx}")
            Environment.ExitCode = 1
            Return
        End If

        ' 整批处理共用同一个时间戳，保证这一批里面所有工程的 build/revision 段完全一致
        Dim timestamp As Date = Now
        Dim outputDir As String = If(opts.FixOutputPath, Path.GetFullPath(opts.OutputDir), Nothing)
        Dim solutionDir As String = Path.GetDirectoryName(Path.GetFullPath(opts.Slnx))

        Console.WriteLine("sciBASIC# framework vbproj upgrade tool")
        Console.WriteLine($"solution      : {opts.Slnx}")
        Console.WriteLine($"namespace     : {opts.NamespacePrefix}")
        Console.WriteLine($"timestamp      : {timestamp:yyyy-MM-dd HH:mm:ss}")
        Console.WriteLine($"nuget version  : {If(String.IsNullOrWhiteSpace(opts.Version), "<auto> (major.minor + timestamp)", opts.Version)}")
        Console.WriteLine($"assembly ver   : <auto> (major.minor + timestamp)")
        Console.WriteLine($"clean configs  : {If(Not opts.MakeClean, "disabled", "enabled")}")
        Console.WriteLine($"output path    : {If(opts.FixOutputPath, outputDir, "disabled")}")
        Console.WriteLine($"mode           : {If(opts.DryRun, "dry-run (no write)", "write")}")
        Console.WriteLine(New String("-"c, 96))

        Dim projects As List(Of String) = EnumerateProjects(sln, opts)
        Dim results As New List(Of ProjectResult)
        Dim watch As Stopwatch = Stopwatch.StartNew()

        For Each path As String In projects
            results.Add(ProcessProject(path, RelativePath(path, solutionDir), outputDir, opts, timestamp))
        Next

        watch.Stop()

        Call PrintSummary(results, watch.Elapsed, opts.DryRun)
    End Sub

    ''' <summary>
    ''' 处理单个 vbproj 文件
    ''' </summary>
    Private Function ProcessProject(path As String,
                                     display As String,
                                     outputDir As String,
                                     opts As CliOptions,
                                     timestamp As Date) As ProjectResult
        Dim result As New ProjectResult With {
            .FilePath = path,
            .Changes = New VersionUpgrader.VersionChange() {}
        }

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

            If opts.MakeClean Then
                Dim cleaned = ConfigCleaner.Clean(doc, ns, ConfigCleaner.GetTargetFrameworkSet(model))

                result.RemovedConditions = cleaned.Removed
                result.Warnings = cleaned.Warnings
            End If

            If opts.FixOutputPath AndAlso outputDir IsNot Nothing Then
                result.OutputPath = OutputPathFixer.Apply(doc, ns, path, outputDir)
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
    ''' 解析 slnx 解决方案并枚举其中声明的 vbproj，按命名空间前缀过滤出目标工程。
    ''' </summary>
    ''' <remarks>
    ''' 1. 用 <see cref="Solution.Load"/> 解析 slnx，过滤掉解决方案文件夹与非 .vbproj 工程；
    ''' 2. 逐个加载 vbproj 模型，按 RootNamespace 是否以 --namespace 前缀起始（大小写不敏感）过滤；
    ''' 3. 非 Microsoft.NET.Sdk 工程以及文件不存在的工程直接跳过。
    ''' 返回经过排序的绝对路径目标 vbproj 集合。
    ''' </remarks>
    Private Function EnumerateProjects(solution As Solution, opts As CliOptions) As List(Of String)
        Dim list As New List(Of String)

        For Each p As Project In solution.Projects
            If p.IsFolder Then
                Continue For
            End If
            If String.IsNullOrEmpty(p.FullPath) OrElse
               Not p.FullPath.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If
            If Not File.Exists(p.FullPath) Then
                Console.WriteLine($"  [warn] slnx 引用的工程不存在，已跳过: {p.FullPath}")
                Continue For
            End If

            ' slnx 不携带 RootNamespace，必须先加载模型才能做前缀过滤
            Dim model As VBProject = Nothing

            Try
                model = VBProject.LoadProjectXml(p.FullPath)
            Catch ex As Exception
                Console.WriteLine($"  [warn] 无法加载工程，已跳过: {p.FullPath} ({ex.Message})")
                Continue For
            End Try

            If model Is Nothing OrElse Not model.IsDotNetCoreSDK Then
                Continue For
            End If

            If String.IsNullOrWhiteSpace(model.RootNamespace) OrElse
               Not model.RootNamespace.StartsWith(opts.NamespacePrefix, StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            list.Add(p.FullPath)
        Next

        list.Sort(StringComparer.OrdinalIgnoreCase)

        Return list
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
    ''' 解析命令行参数，并做必填项校验
    ''' </summary>
    Private Function ParseCommandLine(args As String()) As CliOptions
        Dim opts As CliOptions = CommandLine.BuildFromArguments(args, NoSubCommand:=True).CreateOpts(Of CliOptions)

        opts.Error = Validate(opts)

        Return opts
    End Function

    ''' <summary>
    ''' 校验必填的命令行参数，返回错误描述（无错误时为空串）
    ''' </summary>
    Private Function Validate(opts As CliOptions) As String
        If String.IsNullOrWhiteSpace(opts.Slnx) Then
            Return "缺少必填参数 --slnx（slnx 解决方案文件路径）。"
        End If
        If Not File.Exists(opts.Slnx) Then
            Return $"slnx 解决方案文件不存在: {opts.Slnx}"
        End If
        If String.IsNullOrWhiteSpace(opts.NamespacePrefix) Then
            Return "缺少必填参数 --namespace（命名空间前缀）。"
        End If
        If opts.FixOutputPath AndAlso String.IsNullOrWhiteSpace(opts.OutputDir) Then
            Return "启用 --fix-output-path 时必须同时提供 --output（输出文件夹）。"
        End If

        Return ""
    End Function

    Private Sub PrintUsage()
        Console.WriteLine("Usage:")
        Console.WriteLine("  PkgVersionUpgrade --slnx <solution.slnx> --namespace <prefix> [options]")
        Console.WriteLine()
        Console.WriteLine("Required:")
        Console.WriteLine("  -s, --slnx <file>     要处理的 slnx 解决方案文件路径。")
        Console.WriteLine("  -p, --namespace <p>  命名空间前缀，用于按 RootNamespace 过滤 slnx 中的 vbproj。")
        Console.WriteLine()
        Console.WriteLine("Options:")
        Console.WriteLine("  -v, --version <ver>   nuget 程序包版本号。指定时直接写入 <Version>；")
        Console.WriteLine("                        未指定时在每个工程现有 <Version> 的 major.minor 基础上")
        Console.WriteLine("                        用当前时间戳推算出剩余数字（CalculateVersion）。")
        Console.WriteLine("  -n, --dry-run         只打印将要发生的改动，不写入文件。")
        Console.WriteLine("      --clean           不仅仅只更新版本号，清理过时的 TargetFramework 条件配置组。")
        Console.WriteLine("  -o, --output <dir>    编译产物输出文件夹。仅当 --fix-output-path 开启时必填；")
        Console.WriteLine("                        会被设为所操作目标 vbproj 的 nuget_release|x64 配置的")
        Console.WriteLine("                        <OutputPath>，并化为相对于该 vbproj 的相对路径。")
        Console.WriteLine("      --fix-output-path 修正 nuget_release|x64 的产物输出路径（需配合 --output）。")
        Console.WriteLine("                        缺配置组的工程自动补建，并补齐 <Configurations> 中的")
        Console.WriteLine("                        nuget_release 与 <Platforms> 中的 x64。")
        Console.WriteLine("  -h, --help            显示本帮助信息。")
        Console.WriteLine()
        Console.WriteLine("Notes:")
        Console.WriteLine("  * <AssemblyVersion> 与 <FileVersion> 恒由当前时间戳推算，不受 --version 影响；")
        Console.WriteLine("    nuget 版本号与 assembly version 在所有 SDK 工程中确保存在，file version 只更新已有值。")
        Console.WriteLine("  * --fix-output-path 默认关闭，需要显式指定才执行；带 $(TargetFramework) 的")
        Console.WriteLine("    nuget_release|net10.0|x64 变体配置组同样会被修正。")
        Console.WriteLine("  * 仅处理 Microsoft.NET.Sdk 风格工程，legacy 工程自动跳过；")
        Console.WriteLine("    命名空间前缀不匹配的工程同样不参与任何更新。")
        Console.WriteLine()
        Console.WriteLine("Examples:")
        Console.WriteLine("  PkgVersionUpgrade --slnx VBS.slnx --namespace Microsoft.VisualBasic --dry-run")
        Console.WriteLine("  PkgVersionUpgrade --slnx VBS.slnx --namespace Microsoft.VisualBasic -v 10.5.0.0")
        Console.WriteLine("  PkgVersionUpgrade --slnx VBS.slnx --namespace Microsoft.VisualBasic --fix-output-path --output G:\out -n")
    End Sub

End Module
