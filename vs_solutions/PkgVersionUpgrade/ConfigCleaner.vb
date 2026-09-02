Imports System.Xml.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj

''' <summary>
''' vbproj 中过时编译配置的清理模块
''' </summary>
''' <remarks>
''' 当项目的 TargetFramework / TargetFrameworks 已经升级到新版本（例如 net10.0）之后，
''' 历史遗留下来的针对旧框架的条件属性组（例如 net6.0 / net6.0-windows / net48）就再也不会被求值了，
''' 这个模块负责把这些死配置从 vbproj 文件里剔除掉，精简项目文件。
'''
''' 典型的待清理条件组：
'''
''' ```xml
''' &lt;PropertyGroup Condition="'$(Configuration)|$(TargetFramework)|$(Platform)'=='Debug|net6.0|AnyCPU'">
'''     &lt;DebugType>full&lt;/DebugType>
''' &lt;/PropertyGroup>
''' ```
'''
''' 而下面这种不带 $(TargetFramework) 的条件组则不在清理范围内，必须原样保留：
'''
''' ```xml
''' &lt;PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Debug|AnyCPU'">
''' ```
''' </remarks>
Module ConfigCleaner

    ''' <summary>条件表达式中代表目标框架的属性占位符</summary>
    Private Const TargetFrameworkToken As String = "$(TargetFramework)"

    ''' <summary>
    ''' 从 VBProject 模型中收集出项目当前已经声明的目标框架集合
    ''' </summary>
    ''' <param name="model">由 <see cref="VBProject.LoadProjectXml(String)"/> 加载得到的工程模型。</param>
    ''' <returns>
    ''' ``TargetFramework`` 与 ``TargetFrameworks`` 按分号拆分后的集合（大小写不敏感）。
    ''' 两个属性都没有声明的时候返回空集合。
    ''' </returns>
    Public Function GetTargetFrameworkSet(model As VBProject) As HashSet(Of String)
        Dim tfSet As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        If model Is Nothing OrElse model.Metadata Is Nothing Then
            Return tfSet
        End If

        For Each declared As String In {model.Metadata.TargetFramework, model.Metadata.TargetFrameworks}
            If String.IsNullOrWhiteSpace(declared) Then
                Continue For
            End If

            For Each part As String In declared.Split(";"c)
                Dim tf As String = part.Trim()

                If tf.Length > 0 Then
                    tfSet.Add(tf)
                End If
            Next
        Next

        Return tfSet
    End Function

    ''' <summary>
    ''' 从 PropertyGroup 的 Condition 中提取出 $(TargetFramework) 所对应的具体值
    ''' </summary>
    ''' <param name="condition">Condition 属性的原始字符串。</param>
    ''' <returns>
    ''' 成功解析时返回目标框架（例如 ``net6.0``）；
    ''' 条件中没有引用 $(TargetFramework)、或者形式无法识别的时候返回 Nothing，
    ''' 调用方在拿到 Nothing 的时候应当保守地保留该属性组。
    ''' </returns>
    ''' <remarks>
    ''' 支持下面这两种实测出现过的形式：
    '''
    ''' 1. ``'$(Configuration)|$(TargetFramework)|$(Platform)'=='Debug|net6.0|AnyCPU'``
    ''' 2. ``'$(TargetFramework)'=='net6.0'``
    '''
    ''' 使用了 ``!=`` 这类否定比较的条件语义不明确，一律当作无法解析处理。
    ''' </remarks>
    Public Function TryExtractTargetFramework(condition As String) As String
        If String.IsNullOrWhiteSpace(condition) Then
            Return Nothing
        End If
        If condition.IndexOf(TargetFrameworkToken, StringComparison.OrdinalIgnoreCase) < 0 Then
            Return Nothing
        End If

        Dim eqIndex As Integer = condition.IndexOf("==", StringComparison.Ordinal)
        Dim neIndex As Integer = condition.IndexOf("!=", StringComparison.Ordinal)

        ' 否定比较，或者是根本找不到比较运算符，都无法安全地推断出目标框架
        If neIndex >= 0 AndAlso (eqIndex < 0 OrElse neIndex < eqIndex) Then
            Return Nothing
        End If
        If eqIndex < 0 Then
            Return Nothing
        End If

        Dim template As String = Unquote(condition.Substring(0, eqIndex).Trim())
        Dim value As String = Unquote(condition.Substring(eqIndex + 2).Trim())

        ' 条件模板按照 | 分段，取出 $(TargetFramework) 所处的分段下标，
        ' 然后到比较值里面取同样下标的那个分段，就是这一组配置所对应的目标框架
        Dim segments As String() = template.Split("|"c)
        Dim values As String() = value.Split("|"c)
        Dim index As Integer = -1

        For i As Integer = 0 To segments.Length - 1
            If segments(i).Trim().Equals(TargetFrameworkToken, StringComparison.OrdinalIgnoreCase) Then
                index = i
                Exit For
            End If
        Next

        If index < 0 OrElse index >= values.Length Then
            Return Nothing
        End If

        Dim tf As String = values(index).Trim()

        If tf.Length = 0 Then
            Return Nothing
        End If

        Return tf
    End Function

    ''' <summary>
    ''' 就地移除目标框架已经失效的条件属性组
    ''' </summary>
    ''' <param name="doc">以 PreserveWhitespace 方式加载的原始文档。</param>
    ''' <param name="ns">文档根元素的命名空间。</param>
    ''' <param name="tfSet">项目当前声明的有效目标框架集合。</param>
    ''' <returns>移除的属性组数量，以及因为无法解析而保守保留下来的属性组数量。</returns>
    Public Function Clean(doc As XDocument, ns As XNamespace, tfSet As HashSet(Of String)) As (Removed As Integer, Warnings As Integer)
        Dim removed As Integer = 0
        Dim warnings As Integer = 0

        If doc.Root Is Nothing OrElse tfSet Is Nothing Then
            Return (0, 0)
        End If

        Dim obsolete As New List(Of XElement)

        For Each pg As XElement In doc.Root.Elements(ns + "PropertyGroup")
            Dim condition As String = If(pg.Attribute("Condition")?.Value, "")

            If String.IsNullOrWhiteSpace(condition) Then
                Continue For
            End If
            If condition.IndexOf(TargetFrameworkToken, StringComparison.OrdinalIgnoreCase) < 0 Then
                ' 与目标框架无关的条件组（纯 Configuration|Platform）不在清理范围内
                Continue For
            End If

            Dim tf As String = TryExtractTargetFramework(condition)

            If tf Is Nothing Then
                warnings += 1
                Continue For
            End If

            If Not tfSet.Contains(tf) Then
                obsolete.Add(pg)
            End If
        Next

        For Each pg As XElement In obsolete
            RemoveWithTrailingWhitespace(pg)
            removed += 1
        Next

        Return (removed, warnings)
    End Function

    ''' <summary>
    ''' 移除属性组，并且顺带移除紧跟在它后面的空白文本节点，避免留下成片的空行
    ''' </summary>
    Private Sub RemoveWithTrailingWhitespace(element As XElement)
        Dim next1 As XNode = element.NextNode

        If TypeOf next1 Is XText AndAlso
            DirectCast(next1, XText).Value.Trim().Length = 0 AndAlso
            next1.NextNode IsNot Nothing Then

            next1.Remove()
        End If

        element.Remove()
    End Sub

    ''' <summary>
    ''' 去掉 Condition 两侧的单引号或者双引号
    ''' </summary>
    Private Function Unquote(text As String) As String
        If text Is Nothing OrElse text.Length < 2 Then
            Return text
        End If

        Dim first As Char = text(0)
        Dim last As Char = text(text.Length - 1)

        If (first = "'"c OrElse first = """"c) AndAlso last = first Then
            Return text.Substring(1, text.Length - 2)
        End If

        Return text
    End Function

End Module
