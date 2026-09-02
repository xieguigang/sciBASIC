''' <summary>
''' MSBuild Condition 表达式的解析工具
''' </summary>
''' <remarks>
''' vbproj 里条件属性组的 Condition 形如：
'''
''' ```xml
''' &lt;PropertyGroup Condition="'$(Configuration)|$(TargetFramework)|$(Platform)'=='Debug|net6.0|AnyCPU'">
''' ```
'''
''' 左侧是条件模板，右侧是比较值，两边都按 ``|`` 分段一一对应。
'''
''' **模板中各个占位符的顺序并不固定**，实测存在
''' ``'$(Configuration)|$(Platform)|$(TargetFramework)'=='mzkit_win32|x64|net6.0'``
''' 这类顺序颠倒的写法。因此这里一律先在模板里定位目标占位符的下标，
''' 再取比较值中同下标的分段，绝不固定取第 0/1/2 段。
''' </remarks>
Module MsBuildCondition

    ''' <summary>条件表达式中代表编译配置的占位符</summary>
    Public Const ConfigurationToken As String = "$(Configuration)"
    ''' <summary>条件表达式中代表目标平台的占位符</summary>
    Public Const PlatformToken As String = "$(Platform)"
    ''' <summary>条件表达式中代表目标框架的占位符</summary>
    Public Const TargetFrameworkToken As String = "$(TargetFramework)"

    ''' <summary>
    ''' 把一个 Condition 表达式拆分成条件模板分段与比较值分段
    ''' </summary>
    ''' <param name="condition">Condition 属性的原始字符串。</param>
    ''' <returns>
    ''' 拆分成功时返回两侧的分段数组；
    ''' 使用了 ``!=`` 这类否定比较、或者根本找不到比较运算符时返回 (Nothing, Nothing)。
    ''' </returns>
    Public Function TrySplit(condition As String) As (Template As String(), Values As String())
        If String.IsNullOrWhiteSpace(condition) Then
            Return (Nothing, Nothing)
        End If

        Dim eqIndex As Integer = condition.IndexOf("==", StringComparison.Ordinal)
        Dim neIndex As Integer = condition.IndexOf("!=", StringComparison.Ordinal)

        ' 否定比较的语义不明确，无法安全地推断出任何取值
        If neIndex >= 0 AndAlso (eqIndex < 0 OrElse neIndex < eqIndex) Then
            Return (Nothing, Nothing)
        End If
        If eqIndex < 0 Then
            Return (Nothing, Nothing)
        End If

        Dim template As String = Unquote(condition.Substring(0, eqIndex).Trim())
        Dim value As String = Unquote(condition.Substring(eqIndex + 2).Trim())

        Return (template.Split("|"c), value.Split("|"c))
    End Function

    ''' <summary>
    ''' 取出 Condition 中某个 MSBuild 属性占位符所对应的具体取值
    ''' </summary>
    ''' <param name="condition">Condition 属性的原始字符串。</param>
    ''' <param name="token">占位符，例如 ``$(TargetFramework)``。</param>
    ''' <returns>取值；条件中没有引用该占位符、或者形式无法识别时返回 Nothing。</returns>
    Public Function TryGetValue(condition As String, token As String) As String
        If String.IsNullOrWhiteSpace(condition) Then
            Return Nothing
        End If
        If condition.IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0 Then
            Return Nothing
        End If

        Dim parts = TrySplit(condition)

        If parts.Template Is Nothing Then
            Return Nothing
        End If

        Dim index As Integer = IndexOfToken(parts.Template, token)

        If index < 0 OrElse index >= parts.Values.Length Then
            Return Nothing
        End If

        Dim value As String = parts.Values(index).Trim()

        If value.Length = 0 Then
            Return Nothing
        End If

        Return value
    End Function

    ''' <summary>
    ''' 判定 Condition 是否同时匹配给定的编译配置与目标平台
    ''' </summary>
    ''' <param name="condition">Condition 属性的原始字符串。</param>
    ''' <param name="configuration">期望的编译配置，例如 ``nuget_release``。</param>
    ''' <param name="platform">期望的目标平台，例如 ``x64``。</param>
    ''' <returns>
    ''' 模板中是否同时声明了 ``$(Configuration)`` 与 ``$(Platform)`` 且取值一致。
    ''' 模板里是否额外带有 ``$(TargetFramework)`` 不影响判定，
    ''' 所以纯形式与带目标框架的变体形式会被同一套逻辑覆盖。
    ''' </returns>
    Public Function IsMatch(condition As String, configuration As String, platform As String) As Boolean
        Dim actualConfiguration As String = TryGetValue(condition, ConfigurationToken)
        Dim actualPlatform As String = TryGetValue(condition, PlatformToken)

        If actualConfiguration Is Nothing OrElse actualPlatform Is Nothing Then
            Return False
        End If

        Return actualConfiguration.Equals(configuration, StringComparison.OrdinalIgnoreCase) AndAlso
               actualPlatform.Equals(platform, StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>在条件模板分段中定位某个占位符所处的下标</summary>
    Private Function IndexOfToken(template As String(), token As String) As Integer
        For i As Integer = 0 To template.Length - 1
            If template(i).Trim().Equals(token, StringComparison.OrdinalIgnoreCase) Then
                Return i
            End If
        Next

        Return -1
    End Function

    ''' <summary>
    ''' 去掉条件两侧的单引号或者双引号
    ''' </summary>
    Public Function Unquote(text As String) As String
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
