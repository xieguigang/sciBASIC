Imports System.ComponentModel
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.NuGet
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.ProjectXml
Imports Microsoft.VisualBasic.CommandLine
Imports VBScriptHost.Script

''' <summary>
''' <c>vbs make-project</c> 子命令: 把一个脚本<b>就地</b>转换为正式的 vbproj 工程。
''' </summary>
''' <remarks>
''' 转换的产出(全部位于脚本所在目录):
''' <list type="bullet">
''' <item><c>&lt;name&gt;.vbproj</c> —— 依据脚本的元数据指令生成的 SDK 风格工程文件;</item>
''' <item><c>src/Program.vb</c> —— 脚本的顶层语句/顶层函数/类型定义重构成的正式 VB.NET 代码;</item>
''' <item><c>src/VBScriptHostMagics.vb</c> —— 脚本魔法方法物化之后的普通源码文件;</item>
''' <item><c>src/&lt;被引入脚本&gt;.vb</c> —— 每一个 <c>#include</c> 引入的脚本。</item>
''' </list>
''' 原始脚本文件保持不动(不再参与编译)。生成之后默认执行 <c>dotnet build</c> 验证。
''' </remarks>
Module MakeProject

    ''' <summary>生成的工程所面向的目标框架(与脚本引擎保持一致)</summary>
    Public Const TargetFramework As String = "net10.0"

    ''' <summary>生成的源码所在的子目录名</summary>
    Public Const SourceFolder As String = "src"

    Public Function [Run](args As String()) As Integer
        If args.Length < 2 OrElse args(1).StringEmpty Then
            Call PrintUsage()

            Return 1
        End If

        Dim cmdl As CommandLine = CommandLine.BuildFromArguments(args.Skip(1).ToArray(), NoSubCommand:=False)
        Dim scriptFile As String = args(1)
        Dim verbose As Boolean = cmdl("--verbose")
        Dim noBuild As Boolean = cmdl("--no-build")
        Dim force As Boolean = cmdl("--force")
        Dim runtime As String = cmdl("--runtime")
        Dim vectorize As Boolean = Not cmdl("--no-vectorize")

        Try
            Call Console.WriteLine("---------- make-project ----------")

            ' ---- Step1: 解析脚本 ----
            Dim script As ScriptParseResult = VBScript.ParseScript(scriptFile, verbose, vectorize)

            Dim scriptPath As String = Path.GetFullPath(scriptFile)
            Dim projectDir As String = Path.GetDirectoryName(scriptPath)
            Dim name As String = Path.GetFileNameWithoutExtension(scriptPath)
            Dim projFile As String = Path.Combine(projectDir, name & ".vbproj")
            Dim srcDir As String = Path.Combine(projectDir, SourceFolder)

            If File.Exists(projFile) AndAlso Not force Then
                Call Console.WriteLine($"ERROR: 目标工程文件已经存在: {projFile}")
                Call Console.WriteLine("       如需覆盖请追加 --force")

                Return 1
            End If

            ' ---- Step2: 生成工程源码 ----
            Dim builder As New ProjectCodeBuilder(script)

            Call Console.WriteLine($"    脚本        : {scriptPath}")

            If builder.PromotedVariables.Length > 0 Then
                Call Console.WriteLine($"    提升字段    : {String.Join(", ", builder.PromotedVariables)}")
            End If

            Call Console.WriteLine($"    向量化改写  : {If(script.VectorizeEnabled, "已启用", "已关闭(SIMD)")}" &
                                   $" (SIMD {If(script.Vectorized, "已生效", "未生效")}, @ 投影 {script.Projections} 处)")

            If script.ScriptIncludes.Count > 0 Then
                Call Console.WriteLine($"    引入脚本    : {script.ScriptIncludes.Count} 个")
            End If

            If script.NuGetPackages.Count > 0 Then
                Call Console.WriteLine($"    nuget 包    : {script.NuGetPackages.Count} 个(含传递依赖)")
            End If

            For Each warning As String In script.IncludeWarnings
                Call Console.WriteLine("WARNING: " & warning)
            Next

            ' ---- Step3: 写出源码与工程文件 ----
            If force AndAlso Directory.Exists(srcDir) Then
                Call Directory.Delete(srcDir, recursive:=True)
            End If

            Call Directory.CreateDirectory(srcDir)

            Dim files As ProjectCodeBuilder.ProjectSourceFile() = builder.BuildSources().ToArray()

            For Each source As ProjectCodeBuilder.ProjectSourceFile In files
                Dim sourcePath As String = Path.Combine(srcDir, source.FileName)

                Call File.WriteAllText(sourcePath, source.Code)
                Call Console.WriteLine($"    写出源码    : {Relative(projectDir, sourcePath)}")
            Next

            Dim project As VBProject = BuildProjectModel(script, name, projectDir, files, runtime)

            Call SaveProject(project, projFile)
            Call Console.WriteLine($"    写出工程    : {Relative(projectDir, projFile)}")

            ' ---- Step4: 构建验证 ----
            If noBuild Then
                Call Console.WriteLine("    已跳过构建验证(--no-build)")

                Return 0
            End If

            Return Build(projFile)
        Catch ex As NuGetException
            Call Console.WriteLine("ERROR: nuget 包解析失败: " & ex.Message)

            Return 1
        Catch ex As IOException
            Call Console.WriteLine("ERROR: 写出工程文件失败: " & ex.Message)

            Return 1
        Catch ex As InvalidOperationException
            Call Console.WriteLine("ERROR: " & ex.Message)

            If verbose Then
                Call Console.WriteLine(ex.ToString())
            End If

            Return 1
        End Try
    End Function

    Private Sub PrintUsage()
        Call Console.WriteLine("vbs make-project </path/to/script.vb> [--verbose] [--no-build] [--force] [--runtime <sciBASIC runtime dll>] [--no-vectorize]")
        Call Console.WriteLine()
        Call Console.WriteLine("    把一个 vb 脚本就地转换为正式的 vbproj 工程(默认在生成之后执行 dotnet build 验证)")
        Call Console.WriteLine()
        Call Console.WriteLine("    --verbose        输出解析细节与异常堆栈")
        Call Console.WriteLine("    --no-build       只生成工程文件, 不执行构建验证")
        Call Console.WriteLine("    --force          覆盖已经存在的工程(会先清空 src/ 目录)")
        Call Console.WriteLine("    --runtime <dll>  指定 sciBASIC 运行时程序集(Microsoft.VisualBasic.Runtime.dll)")
        Call Console.WriteLine("    --no-vectorize   关闭数值向量的自动向量化改写")
    End Sub

    ' ==================================================================
    ' 工程模型
    ' ==================================================================

    Private Function BuildProjectModel(script As ScriptParseResult,
                                       name As String,
                                       projectDir As String,
                                       files As ProjectCodeBuilder.ProjectSourceFile(),
                                       runtime As String) As VBProject

        Dim metadata As ScriptMetadata = script.Metadata
        Dim assemblyName As String = name

        If metadata IsNot Nothing AndAlso Not String.IsNullOrEmpty(metadata.Package) Then
            assemblyName = metadata.Package
        End If

        Dim project As New VBProject With {
            .Sdk = "Microsoft.NET.Sdk",
            .OutputType = "Exe",
            .AssemblyName = assemblyName,
            .RootNamespace = "",
            .Metadata = New VBProjectMetadata With {
                .TargetFramework = TargetFramework,
                .LangVersion = "16",
                .EnableDefaultCompileItems = "false",
                .Other = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            },
            .NuGet = New VBNuGetMetadata With {
                .Other = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            },
            .CompileFiles = files.Select(
                Function(source) New VBDocument With {.FileName = SourceFolder & "/" & source.FileName}).ToArray()
        }

        ' 只编译 src/ 下由本命令生成的源码; 原始脚本与目录之中的其它 .vb 文件都不参与编译
        project.Metadata.Other("OptionExplicit") = "On"
        project.Metadata.Other("OptionStrict") = "Off"
        project.Metadata.Other("OptionInfer") = "On"

        If metadata IsNot Nothing Then
            Call ApplyMetadata(project, metadata)
        End If

        project.References = BuildReferences(script, projectDir, runtime)
        project.PackageReferences = BuildPackageReferences(script)

        Return project
    End Function

    ''' <summary>把脚本头部的元数据指令映射为工程属性</summary>
    Private Sub ApplyMetadata(project As VBProject, metadata As ScriptMetadata)
        If Not String.IsNullOrEmpty(metadata.Author) Then
            project.NuGet.Authors = metadata.Author
            project.NuGet.Company = metadata.Author
        End If

        If Not String.IsNullOrEmpty(metadata.Title) Then
            project.NuGet.Product = metadata.Title
            project.Metadata.Other("AssemblyTitle") = metadata.Title
        End If

        If Not String.IsNullOrEmpty(metadata.Version) Then
            project.NuGet.Version = metadata.Version
            project.Metadata.Other("AssemblyVersion") = metadata.Version
        End If
    End Sub

    ''' <summary>
    ''' 收集 <c>&lt;Reference&gt;</c> 项:
    ''' 1) sciBASIC 运行时(提供 <c>CommandLine</c> 类型与 VB 运行时能力);
    ''' 2) 脚本通过 <c>#include "xxx.dll"</c> 直接引用的本地程序集。
    ''' nuget 资产由 <c>&lt;PackageReference&gt;</c> 负责, 脚本引擎自身的程序集不再需要。
    ''' </summary>
    Private Function BuildReferences(script As ScriptParseResult, projectDir As String, runtime As String) As VBReference()
        Dim list As New List(Of VBReference)
        Dim runtimePath As String = If(runtime.StringEmpty, GetType(CommandLine).Assembly.Location, runtime)

        If runtimePath.FileExists Then
            Call list.Add(New VBReference With {
                .Include = Path.GetFileNameWithoutExtension(runtimePath),
                .HintPath = Relative(projectDir, runtimePath),
                .[Private] = "true"
            })
        Else
            Call Console.WriteLine($"WARNING: 找不到 sciBASIC 运行时程序集, 生成的工程可能无法编译: {runtimePath}")
        End If

        Dim nugetAssets As New HashSet(Of String)(
            script.NuGetPackages.SelectMany(Function(p) p.Assemblies), StringComparer.OrdinalIgnoreCase)

        ' 脚本引擎自身: 转换之后不再需要(魔法方法已经物化到源码之中)
        Dim engine As String = Path.GetFileNameWithoutExtension(GetType(DynamicDll).Assembly.Location)

        For Each dll As String In script.Imports
            If nugetAssets.Contains(dll) Then
                Continue For
            End If

            Dim simple As String = Path.GetFileNameWithoutExtension(dll)

            If String.Equals(simple, engine, StringComparison.OrdinalIgnoreCase) Then
                Call Console.WriteLine($"WARNING: #include ""{Path.GetFileName(dll)}"" 指向脚本引擎自身, 转换之后已自动跳过")

                Continue For
            End If

            Call list.Add(New VBReference With {
                .Include = simple,
                .HintPath = Relative(projectDir, dll),
                .[Private] = "true"
            })
        Next

        Return list.ToArray()
    End Function

    ''' <summary>
    ''' 收集 <c>&lt;PackageReference&gt;</c> 项: 只输出 <c>#include</c> 直接引用的根包,
    ''' 传递依赖交给 NuGet restore 依据根包的 nuspec 自动完成。
    ''' </summary>
    Private Function BuildPackageReferences(script As ScriptParseResult) As VBPackageReference()
        Return script.NuGetPackages _
            .Where(Function(package) package.IsRoot) _
            .Select(Function(package) New VBPackageReference With {
                .Id = package.Id,
                .Version = package.Version.NormalizedString()
            }) _
            .ToArray()
    End Function

    ' ==================================================================
    ' 写文件与构建
    ' ==================================================================

    ''' <summary>写工程文件(先写临时文件再原子替换, 避免留下半成品工程)</summary>
    Private Sub SaveProject(project As VBProject, projFile As String)
        Dim tmp As String = projFile & ".tmp"

        If File.Exists(tmp) Then
            Call File.Delete(tmp)
        End If

        Call project.Save(tmp)
        Call File.Move(tmp, projFile, overwrite:=True)
    End Sub

    ''' <summary>执行 <c>dotnet build</c> 验证生成的工程; 本机没有 dotnet 时降级为警告</summary>
    Private Function Build(projFile As String) As Integer
        Dim psi As New ProcessStartInfo("dotnet") With {
            .Arguments = $"build ""{projFile}"" --nologo",
            .UseShellExecute = False,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .CreateNoWindow = True,
            .WorkingDirectory = Path.GetDirectoryName(projFile)
        }

        Try
            Using process As Process = Process.Start(psi)
                AddHandler process.OutputDataReceived, AddressOf PrintLine
                AddHandler process.ErrorDataReceived, AddressOf PrintLine
                Call process.BeginOutputReadLine()
                Call process.BeginErrorReadLine()
                Call process.WaitForExit()

                If process.ExitCode = 0 Then
                    Call Console.WriteLine("    构建验证    : 成功")
                Else
                    Call Console.WriteLine($"    构建验证    : 失败 (exit code {process.ExitCode})")
                End If

                Return process.ExitCode
            End Using
        Catch ex As Win32Exception
            Call Console.WriteLine("WARNING: 未找到 dotnet CLI, 已跳过构建验证: " & ex.Message)

            Return 0
        End Try
    End Function

    Private Sub PrintLine(sender As Object, e As DataReceivedEventArgs)
        If Not e.Data Is Nothing Then
            Call Console.WriteLine("      | " & e.Data)
        End If
    End Sub

    ''' <summary>把绝对路径转换为相对于工程目录的路径(跨盘符时原样返回)</summary>
    Private Function Relative(baseDir As String, targetPath As String) As String
        Dim rel As String = Path.GetRelativePath(baseDir, targetPath)

        If Path.IsPathRooted(rel) Then
            Return targetPath
        End If

        Return rel.Replace("\"c, "/"c)
    End Function
End Module
