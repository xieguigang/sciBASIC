Imports System.Xml.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Development

''' <summary>
''' vbproj 版本号升级模块
''' </summary>
''' <remarks>
''' 版本号的计算统一复用 <see cref="ApplicationInfoUtils.CalculateVersion(Date, Integer, Integer)"/>，
''' 本模块只负责决定 major.minor 基准值，以及将结果写回到 vbproj 的 XML 文档中。
''' </remarks>
Module VersionUpgrader

    ''' <summary>
    ''' 单个版本元素的变更记录，用于 dry-run 预览以及最终的统计汇总
    ''' </summary>
    Public Class VersionChange

        ''' <summary>版本元素的名称，例如 Version / AssemblyVersion / FileVersion</summary>
        Public Property Name As String
        ''' <summary>写入之前的值，元素原本不存在时为空字符串</summary>
        Public Property OldValue As String
        ''' <summary>本次计算出的新值</summary>
        Public Property NewValue As String
        ''' <summary>该元素是否是本次新建出来的</summary>
        Public Property Inserted As Boolean

        Public ReadOnly Property Changed As Boolean
            Get
                Return Inserted OrElse Not String.Equals(OldValue, NewValue, StringComparison.Ordinal)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim from As String = If(Inserted, "<none>", If(String.IsNullOrEmpty(OldValue), "<empty>", OldValue))
            Return $"{Name}: {from} -> {NewValue}"
        End Function

    End Class

    ''' <summary>
    ''' 容错解析版本号中的 major.minor 片段
    ''' </summary>
    ''' <param name="version">
    ''' 允许的形式：``10.5.3.8911``、``1.1.25.0``、``2.33.*``（通配符）、空串、以及任意非数字文本。
    ''' </param>
    ''' <returns>解析失败或者值缺失的时候回退到 (1, 0)</returns>
    Public Function ParseMajorMinor(version As String) As (Major As Integer, Minor As Integer)
        Dim major As Integer = 1
        Dim minor As Integer = 0

        If String.IsNullOrWhiteSpace(version) Then
            Return (major, minor)
        End If

        Dim parts As String() = version.Trim().Split("."c)

        ' Integer.TryParse 在失败的时候会把 ByRef 的结果清零，
        ' 所以这里需要在失败分支上显式还原为缺省值
        If parts.Length > 0 AndAlso Not Integer.TryParse(parts(0).Trim(), major) Then
            major = 1
        End If
        If parts.Length > 1 AndAlso Not Integer.TryParse(parts(1).Trim(), minor) Then
            minor = 0
        End If

        If major < 0 Then major = 1
        If minor < 0 Then minor = 0

        Return (major, minor)
    End Function

    ''' <summary>
    ''' nuget 程序包版本号：命令行显式指定了版本号的时候直接采用，
    ''' 否则在现有版本号的 major.minor 基础上用当前时间戳推算出剩余的数字。
    ''' </summary>
    ''' <param name="cliVersion">用户通过 ``--version`` 传入的版本号，没有传入时为空。</param>
    ''' <param name="currentVersion">vbproj 中现有的 ``&lt;Version&gt;`` 值。</param>
    ''' <param name="timestamp">统一的时间戳（整批处理中共用同一个值）。</param>
    Public Function ResolveNuGetVersion(cliVersion As String, currentVersion As String, timestamp As Date) As String
        If Not String.IsNullOrWhiteSpace(cliVersion) Then
            Return cliVersion.Trim()
        End If

        Return StampVersion(currentVersion, timestamp)
    End Function

    ''' <summary>
    ''' assembly version / assembly file version：
    ''' 恒由当前时间戳推算得出，完全不受命令行 ``--version`` 参数的影响。
    ''' </summary>
    ''' <param name="currentVersion">该元素自身当前的值，用于提取 major.minor 基准。</param>
    ''' <param name="timestamp">统一的时间戳。</param>
    Public Function ResolveAssemblyVersion(currentVersion As String, timestamp As Date) As String
        Return StampVersion(currentVersion, timestamp)
    End Function

    ''' <summary>
    ''' 在已有版本号的 major.minor 基础上，用时间戳计算出 version 中剩余的数字
    ''' </summary>
    Private Function StampVersion(currentVersion As String, timestamp As Date) As String
        Dim baseline = ParseMajorMinor(currentVersion)
        Dim ver As Version = timestamp.CalculateVersion(baseline.Major, baseline.Minor)

        If ver Is Nothing Then
            Return currentVersion
        End If

        Return ver.ToString()
    End Function

    ''' <summary>
    ''' 将计算好的版本号写回到 vbproj 文档当中
    ''' </summary>
    ''' <param name="doc">以 PreserveWhitespace 方式加载的原始文档，会就地进行修改。</param>
    ''' <param name="ns">文档根元素的命名空间（SDK 风格工程一般为 <see cref="XNamespace.None"/>）。</param>
    ''' <param name="nuGetVersion">``&lt;Version&gt;`` 的新值。</param>
    ''' <param name="assemblyVersion">``&lt;AssemblyVersion&gt;`` 的新值。</param>
    ''' <param name="fileVersion">``&lt;FileVersion&gt;`` 的新值。</param>
    ''' <param name="insertFileVersion">
    ''' 当 ``&lt;FileVersion&gt;`` 元素不存在的时候是否新建。
    ''' nuget 版本号与 assembly version 总是确保存在，而 file version 默认只更新已有的。
    ''' </param>
    ''' <returns>三个版本元素各自的变更记录</returns>
    Public Function Apply(doc As XDocument,
                          ns As XNamespace,
                          nuGetVersion As String,
                          assemblyVersion As String,
                          fileVersion As String,
                          insertFileVersion As Boolean) As VersionChange()

        Dim result As New List(Of VersionChange)

        If doc.Root Is Nothing Then
            Return result.ToArray()
        End If

        Dim group As XElement = XmlEditor.MainPropertyGroup(doc, ns)

        result.Add(SetProperty(group, ns, "Version", nuGetVersion, True))
        result.Add(SetProperty(group, ns, "AssemblyVersion", assemblyVersion, True))
        result.Add(SetProperty(group, ns, "FileVersion", fileVersion, insertFileVersion))

        Return result.ToArray()
    End Function

    ''' <summary>
    ''' 更新或者新增一个版本属性元素
    ''' </summary>
    Private Function SetProperty(group As XElement,
                                 ns As XNamespace,
                                 name As String,
                                 value As String,
                                 allowInsert As Boolean) As VersionChange

        Dim change As New VersionChange With {
            .Name = name,
            .NewValue = value,
            .OldValue = "",
            .Inserted = False
        }

        If Not allowInsert AndAlso group.Element(ns + name) Is Nothing Then
            ' 元素不存在并且不允许新建，这种情况下实际上什么都没有改动，
            ' 把新值清空以免向外误报一次变更
            change.NewValue = ""
            Return change
        End If

        Dim applied = XmlEditor.SetOrCreateElement(group, ns, name, value, allowInsert)

        change.Inserted = applied.Inserted
        change.OldValue = applied.OldValue

        Return change
    End Function

End Module
