Imports System.IO
Imports System.Xml.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj

''' <summary>
''' nuget_release|x64 编译配置的产物输出路径修正模块
''' </summary>
''' <remarks>
''' 框架下 RootNamespace 以 ``Microsoft.VisualBasic`` 起始的工程都是要发布到 nuget 的库，
''' 它们在 ``nuget_release|x64`` 配置下应当统一把产物输出到框架根的 ``.nuget`` 目录中，
''' 便于集中打包。
'''
''' 这个模块负责三件事：
'''
''' 1. 把所有 ``nuget_release|x64`` 条件组（含带 ``$(TargetFramework)`` 的变体）的
'''    ``&lt;OutputPath&gt;`` 改写为指向 ``.nuget`` 的正确相对路径；
''' 2. 完全没有该配置组的工程，补建一个只含 ``&lt;PlatformTarget&gt;`` 与 ``&lt;OutputPath&gt;``
'''    的条件属性组；
''' 3. 补齐 ``&lt;Configurations&gt;`` 中的 ``nuget_release`` 与 ``&lt;Platforms&gt;`` 中的 ``x64`` 声明，
'''    否则新加的条件组永远不会被 MSBuild 求值。
'''
''' 条件判定统一走 <see cref="MsBuildCondition.IsMatch"/>，
''' 模板中占位符的顺序由下标动态定位，因此纯形式
''' ``'$(Configuration)|$(Platform)'=='nuget_release|x64'``
''' 与变体形式
''' ``'$(Configuration)|$(TargetFramework)|$(Platform)'=='nuget_release|net10.0|x64'``
''' 会被同一套逻辑覆盖。
''' </remarks>
Module OutputPathFixer

    ''' <summary>单个工程的输出路径修正结果</summary>
    Public Class OutputPathResult

        ''' <summary>被改写或者补写了 OutputPath 的条件组数量</summary>
        Public Property Updated As Integer
        ''' <summary>新建的 nuget_release|x64 条件组数量（0 或 1）</summary>
        Public Property Created As Integer
        ''' <summary>补齐的 Configurations / Platforms 声明条数</summary>
        Public Property DeclarationsAdded As Integer
        ''' <summary>本次计算出的、指向 .nuget 的相对路径，用于日志展示</summary>
        Public Property OutputPath As String

        Public ReadOnly Property Changed As Boolean
            Get
                Return Updated > 0 OrElse Created > 0 OrElse DeclarationsAdded > 0
            End Get
        End Property

    End Class

    ''' <summary>目标工程的 RootNamespace 前缀</summary>
    Private Const RootNamespacePrefix As String = "Microsoft.VisualBasic"
    ''' <summary>需要修正的编译配置名</summary>
    Private Const ReleaseConfiguration As String = "nuget_release"
    ''' <summary>需要修正的目标平台名</summary>
    Private Const ReleasePlatform As String = "x64"

    ''' <summary>
    ''' 判定一个工程是否需要参与输出路径修正
    ''' </summary>
    ''' <param name="model">由 <see cref="VBProject.LoadProjectXml(String)"/> 加载得到的工程模型。</param>
    ''' <returns>
    ''' 需要同时满足：是 Microsoft.NET.Sdk 风格工程，且 RootNamespace 以
    ''' ``Microsoft.VisualBasic`` 起始（大小写不敏感）。
    ''' </returns>
    ''' <remarks>
    ''' legacy 工程被排除在外 —— 它们没有 &lt;Platforms&gt; 这个 SDK 专有属性，
    ''' 补声明没有意义，工具既有的版本号升级与配置清理也一视同仁地跳过了它们。
    ''' </remarks>
    Public Function IsTarget(model As VBProject) As Boolean
        If model Is Nothing OrElse Not model.IsDotNetCoreSDK Then
            Return False
        End If

        Return Not String.IsNullOrWhiteSpace(model.RootNamespace) AndAlso
               model.RootNamespace.StartsWith(RootNamespacePrefix, StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' 计算出从工程所在目录到 .nuget 目录的相对路径
    ''' </summary>
    ''' <param name="projectPath">vbproj 文件的路径。</param>
    ''' <param name="nugetDir">.nuget 目录的绝对路径，一般是框架根下的 .nuget。</param>
    ''' <returns>
    ''' MSBuild 风格的相对路径，分隔符统一为正斜杠并且以斜杠结尾，
    ''' 例如 ``../../.nuget/``、``../../../.nuget/``。
    ''' </returns>
    Public Function ComputeOutputPath(projectPath As String, nugetDir As String) As String
        Dim projectDir As String = Path.GetDirectoryName(Path.GetFullPath(projectPath))
        Dim relative As String = Path.GetRelativePath(projectDir, nugetDir)

        Return relative.Replace("\"c, "/"c).TrimEnd("/"c) & "/"
    End Function

    ''' <summary>
    ''' 判定一个 Condition 是否对应 nuget_release|x64 编译配置
    ''' </summary>
    ''' <param name="condition">Condition 属性的原始字符串。</param>
    ''' <returns>
    ''' Configuration 为 ``nuget_release`` 且 Platform 为 ``x64`` 时返回 True；
    ''' 条件中没有声明这两个占位符、或者形式无法识别时返回 False。
    ''' </returns>
    Public Function IsNugetReleaseX64(condition As String) As Boolean
        Return MsBuildCondition.IsMatch(condition, ReleaseConfiguration, ReleasePlatform)
    End Function

    ''' <summary>
    ''' 就地修正 nuget_release|x64 配置的产物输出路径
    ''' </summary>
    ''' <param name="doc">以 PreserveWhitespace 方式加载的原始文档。</param>
    ''' <param name="ns">文档根元素的命名空间。</param>
    ''' <param name="projectPath">vbproj 文件的绝对路径，用于推算相对路径。</param>
    ''' <param name="nugetDir">.nuget 目录的绝对路径。</param>
    ''' <returns>本次的修正统计。</returns>
    Public Function Apply(doc As XDocument,
                          ns As XNamespace,
                          projectPath As String,
                          nugetDir As String) As OutputPathResult

        Dim result As New OutputPathResult()

        If doc.Root Is Nothing Then
            Return result
        End If

        Dim outputPath As String = ComputeOutputPath(projectPath, nugetDir)
        Dim matched As Integer = 0

        result.OutputPath = outputPath

        ' 1. 逐个命中组写入 OutputPath。
        '    matched 与 Updated 必须分开统计：已经正确的组不会计入 Updated，
        '    但仍然要计入 matched，否则会被误判成"没有该配置组"而重复新建。
        For Each pg As XElement In doc.Root.Elements(ns + "PropertyGroup")
            Dim condition As String = If(pg.Attribute("Condition")?.Value, "")

            If String.IsNullOrWhiteSpace(condition) Then
                Continue For
            End If
            If Not IsNugetReleaseX64(condition) Then
                Continue For
            End If

            matched += 1

            If XmlEditor.SetOrCreateElement(pg, ns, "OutputPath", outputPath, True).Changed Then
                result.Updated += 1
            End If
        Next

        ' 2. 一个都没有命中，说明这个工程从来没有配过 nuget_release|x64，补建一组
        If matched = 0 Then
            Call CreateReleaseGroup(doc, ns, outputPath)
            result.Created = 1
        End If

        ' 3. 补齐 Configurations / Platforms 声明，确保上面的配置真的会被求值
        result.DeclarationsAdded += EnsureDeclared(doc, ns, "Configurations", ReleaseConfiguration)
        result.DeclarationsAdded += EnsureDeclared(doc, ns, "Platforms", ReleasePlatform)

        Return result
    End Function

    ''' <summary>
    ''' 新建一个只含 PlatformTarget 与 OutputPath 的 nuget_release|x64 条件属性组
    ''' </summary>
    ''' <remarks>
    ''' 刻意不写 DebugSymbols / DebugType / RemoveIntegerChecks 等属性，
    ''' 以免改变工程既有的调试符号构建行为。
    ''' </remarks>
    Private Sub CreateReleaseGroup(doc As XDocument, ns As XNamespace, outputPath As String)
        Dim group As New XElement(ns + "PropertyGroup")
        Dim innerIndent As String = XmlEditor.InferInnerIndent(doc.Root, ns, "PropertyGroup")
        Dim closingIndent As String = XmlEditor.InferChildIndent(doc.Root)

        group.SetAttributeValue("Condition", $"'$(Configuration)|$(Platform)'=='{ReleaseConfiguration}|{ReleasePlatform}'")
        group.Add(New XText(innerIndent))
        group.Add(New XElement(ns + "PlatformTarget", ReleasePlatform))
        group.Add(New XText(innerIndent))
        group.Add(New XElement(ns + "OutputPath", outputPath))
        group.Add(New XText(closingIndent))

        ' 跟随在文档中最后一个属性组之后，保持"主体属性在前、条件配置在后"的既有排布
        Dim last As XElement = doc.Root.Elements(ns + "PropertyGroup").LastOrDefault()

        If last Is Nothing Then
            doc.Root.AddFirst(group)
        Else
            Call XmlEditor.InsertAfter(last, group)
        End If
    End Sub

    ''' <summary>
    ''' 确保主属性组里的某个分号分隔的声明列表中包含指定取值
    ''' </summary>
    ''' <param name="doc">原始文档。</param>
    ''' <param name="name">声明元素名，例如 ``Configurations`` / ``Platforms``。</param>
    ''' <param name="value">必须出现的取值，例如 ``nuget_release`` / ``x64``。</param>
    ''' <returns>发生了改动返回 1，原本就已经包含返回 0。</returns>
    Private Function EnsureDeclared(doc As XDocument, ns As XNamespace, name As String, value As String) As Integer
        Dim group As XElement = XmlEditor.MainPropertyGroup(doc, ns)
        Dim el As XElement = group.Element(ns + name)

        If el Is Nothing Then
            Call XmlEditor.AddElement(group, ns, name, value)
            Return 1
        End If

        Dim items As New List(Of String)

        For Each part As String In If(el.Value, "").Split(";"c)
            Dim item As String = part.Trim()

            If item.Length > 0 Then
                items.Add(item)
            End If
        Next

        For Each item As String In items
            If item.Equals(value, StringComparison.OrdinalIgnoreCase) Then
                Return 0
            End If
        Next

        items.Add(value)
        el.Value = String.Join(";", items)

        Return 1
    End Function

End Module
