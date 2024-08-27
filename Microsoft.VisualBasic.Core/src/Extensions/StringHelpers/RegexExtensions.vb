#Region "Microsoft.VisualBasic::b00492b428789cc93948e9162ba6b6ac, Microsoft.VisualBasic.Core\src\Extensions\StringHelpers\RegexExtensions.vb"

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

    '   Total Lines: 286
    '    Code Lines: 121 (42.31%)
    ' Comment Lines: 127 (44.41%)
    '    - Xml Docs: 96.85%
    ' 
    '   Blank Lines: 38 (13.29%)
    '     File Size: 11.54 KB


    ' Module RegexExtensions
    ' 
    '     Properties: RegexpTimeout
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: (+2 Overloads) EachValue, EndsWith, (+2 Overloads) IsPattern, Locates, PythonRawRegexp
    '               StartsWith, (+2 Overloads) ToArray
    '     Structure [NameOf]
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language

<HideModuleName>
Public Module RegexExtensions

    ''' <summary>
    ''' A helper function for make sub-string replacement
    ''' </summary>
    ''' <param name="r"></param>
    ''' <param name="str"></param>
    ''' <param name="replacement"></param>
    ''' <param name="stringCopy"></param>
    <Extension>
    Public Sub Replace(r As Regex, ByRef str As StringBuilder, replacement As String, Optional stringCopy As String = Nothing)
        If stringCopy Is Nothing Then
            stringCopy = str.ToString
        End If

        Dim matches = r.Matches(stringCopy).ToArray

        For Each m As String In matches
            Call str.Replace(m, replacement)
        Next
    End Sub

    ''' <summary>
    ''' Determines whether the beginning of this string instance matches the specified
    ''' string.
    ''' </summary>
    ''' <param name="str">The string to compare.</param>
    ''' <param name="pattern"></param>
    ''' <param name="opt"></param>
    ''' <returns>true if value matches the beginning of this string; otherwise, false.</returns>
    <Extension>
    Public Function StartsWith(str$, pattern$, opt As RegexOptions) As Boolean
        Dim match$ = str.Match(pattern, opt)

        If match.StringEmpty Then
            Return False
        Else
            Return str.StartsWith(match)
        End If
    End Function

    ''' <summary>
    ''' Determines whether the end of this string instance matches the specified string pattern.
    ''' </summary>
    ''' <param name="str$"></param>
    ''' <param name="pattern$"></param>
    ''' <param name="opt"></param>
    ''' <returns></returns>
    <Extension>
    Public Function EndsWith(str$, pattern$, opt As RegexOptions) As Boolean
        Dim match$ = str.Match(pattern, opt)

        If match.StringEmpty Then
            Return False
        Else
            Return str.EndsWith(match)
        End If
    End Function

    ''' <summary>
    ''' 模拟python的raw字符串的正则表达式，多行的
    ''' </summary>
    Public Const RegexPythonRawString As RegexOptions =
        RegexOptions.Multiline Or
        RegexOptions.IgnorePatternWhitespace Or
        RegexOptions.Compiled

    Const TimeoutConfig$ = "REGEX_DEFAULT_MATCH_TIMEOUT"

    Public Property RegexpTimeout As Integer
        Get
            Dim domain As AppDomain = AppDomain.CurrentDomain
            Dim timeout = domain.GetData(TimeoutConfig)

            Return timeout
        End Get
        Set(value As Integer)
            Dim domain As AppDomain = AppDomain.CurrentDomain
            Dim timeout = TimeSpan.FromSeconds(value)

            ' Set a timeout interval of 2 seconds.
            Call domain.SetData(TimeoutConfig, timeout)
        End Set
    End Property

    Sub New()
        RegexpTimeout = 5
    End Sub

    ''' <summary>
    ''' Name of <see cref="RegexOptions"/>
    ''' </summary>
    Public Structure [NameOf]

        ''' <summary>
        ''' Specifies that no options are set. For more information about the default behavior
        ''' of the regular expression engine, see the "Default Options" section in the Regular
        ''' Expression Options topic.
        ''' </summary>
        Public Const None As String = NameOf(None)

        ''' <summary>
        ''' Specifies case-insensitive matching. For more information, see the "Case-Insensitive
        ''' Matching " section in the Regular Expression Options topic.
        ''' </summary>
        Public Const IgnoreCase As String = NameOf(IgnoreCase)

        ''' <summary>
        ''' Multiline mode. Changes the meaning of ^ and $ so they match at the beginning
        ''' and end, respectively, of any line, and not just the beginning and end of the
        ''' entire string. For more information, see the "Multiline Mode" section in the
        ''' Regular Expression Options topic.
        ''' </summary>
        Public Const Multiline As String = NameOf(Multiline)

        ''' <summary>
        ''' Specifies that the only valid captures are explicitly named or numbered groups
        ''' of the form (?&lt;name>…). This allows unnamed parentheses to act as noncapturing
        ''' groups without the syntactic clumsiness of the expression (?:…). For more information,
        ''' see the "Explicit Captures Only" section in the Regular Expression Options topic.
        ''' </summary>
        Public Const ExplicitCapture As String = NameOf(ExplicitCapture)

        ''' <summary>
        ''' Specifies that the regular expression is compiled to an assembly. This yields
        ''' faster execution but increases startup time. This value should not be assigned
        ''' to the System.Text.RegularExpressions.RegexCompilationInfo.Options property when
        ''' calling the System.Text.RegularExpressions.Regex.CompileToAssembly(System.Text.RegularExpressions.RegexCompilationInfo[],System.Reflection.AssemblyName)
        ''' method. For more information, see the "Compiled Regular Expressions" section
        ''' in the Regular Expression Options topic.
        ''' </summary>
        Public Const Compiled As String = NameOf(Compiled)

        ''' <summary>
        ''' Specifies single-line mode. Changes the meaning of the dot (.) so it matches
        ''' every character (instead of every character except \n). For more information,
        ''' see the "Single-line Mode" section in the Regular Expression Options topic.
        ''' </summary>
        Public Const Singleline As String = NameOf(Singleline)

        ''' <summary>
        ''' Eliminates unescaped white space from the pattern and enables comments marked
        ''' with #. However, this value does not affect or eliminate white space in character
        ''' classes, numeric quantifiers, or tokens that mark the beginning of individual
        ''' regular expression language elements. For more information, see the "Ignore White
        ''' Space" section of the Regular Expression Options topic.
        ''' </summary>
        Public Const IgnorePatternWhitespace As String = NameOf(IgnorePatternWhitespace)

        ''' <summary>
        ''' Specifies that the search will be from right to left instead of from left to
        ''' right. For more information, see the "Right-to-Left Mode" section in the Regular
        ''' Expression Options topic.
        ''' </summary>
        Public Const RightToLeft As String = NameOf(RightToLeft)

        ''' <summary>
        ''' Enables ECMAScript-compliant behavior for the expression. This value can be used
        ''' only in conjunction with the System.Text.RegularExpressions.RegexOptions.IgnoreCase,
        ''' System.Text.RegularExpressions.RegexOptions.Multiline, and System.Text.RegularExpressions.RegexOptions.Compiled
        ''' values. The use of this value with any other values results in an exception.For
        ''' more information on the System.Text.RegularExpressions.RegexOptions.ECMAScript
        ''' option, see the "ECMAScript Matching Behavior" section in the Regular Expression
        ''' Options topic.
        ''' </summary>
        Public Const ECMAScript As String = NameOf(ECMAScript)

        ''' <summary>
        ''' Specifies that cultural differences in language is ignored. For more information,
        ''' see the "Comparison Using the Invariant Culture" section in the Regular Expression
        ''' Options topic.
        ''' </summary>
        Public Const CultureInvariant As String = NameOf(CultureInvariant)
    End Structure

    <Extension>
    Public Function EachValue(Of T)(m As MatchCollection, parser As Func(Of String, T)) As IEnumerable(Of T)
        Return From s As Match In m Select parser(s.Value)
    End Function

    ''' <summary>
    ''' Each match its value in the source match collection.
    ''' </summary>
    ''' <param name="m"></param>
    ''' <returns></returns>
    <Extension>
    Public Function EachValue(m As MatchCollection) As IEnumerable(Of String)
        Return From s As Match In m Select s.Value
    End Function

    ''' <summary>
    ''' Gets the matched strings from the regex match result as source
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <ExportAPI("As.Array")>
    <Extension> Public Function ToArray(source As MatchCollection) As String()
        Dim LQuery$() = LinqAPI.Exec(Of String) _
 _
            () <= From m As Match
                  In source
                  Select m.Value

        Return LQuery
    End Function

    ''' <summary>
    ''' Converts the <see cref="Regex"/> string pattern match results to the objects.
    ''' （这个函数是非并行化的，所以不需要担心会打乱顺序）
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="[ctype]">The object parser</param>
    ''' <returns></returns>
    <Extension>
    Public Function ToArray(Of T)(source As MatchCollection, [ctype] As Func(Of String, T)) As T()
        Dim LQuery As T() = LinqAPI.Exec(Of T) _
 _
            () <= From m As Match
                  In source
                  Let s As String = m.Value
                  Select [ctype](s)

        Return LQuery
    End Function

    ''' <summary>
    ''' The enitre string input equals to the pattern's matched.
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="pattern"></param>
    ''' <returns></returns>
    <Extension>
    Public Function IsPattern(s$, pattern$, Optional opt As RegexOptions = RegexICSng) As Boolean
        ' 2018-6-1 因为空字符串肯定无法匹配上目标模式
        ' 所以match函数总回返回空字符串
        ' 由于s参数本身就是空字符串，所以会造成空字符串可以被任意模式完全匹配的bug
        If s.StringEmpty AndAlso pattern <> StringEmptyPattern Then
            Return False
        End If

        Static patternCache As New Dictionary(Of String, Regex)

        Dim match$ = patternCache _
            .ComputeIfAbsent(pattern, lazyValue:=Function(r) New Regex(r, opt)) _
            .Match(s) _
            .Value

        If match = s Then
            Return True
        Else
            Return False
        End If
    End Function

    Const StringEmptyPattern As String = "\s*"

    ''' <summary>
    ''' The enitre string input equals to the pattern's matched.
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="pattern"></param>
    ''' <returns></returns>
    <Extension>
    Public Function IsPattern(s As String, pattern As Regex) As Boolean
        If s.StringEmpty AndAlso pattern.ToString <> StringEmptyPattern Then
            Return False
        End If

        Return pattern.Match(s).Value = s
    End Function

    ''' <summary>
    ''' 模拟python语言之中的从raw string构建正则表达式
    ''' </summary>
    ''' <param name="raw$"></param>
    ''' <returns></returns>
    <Extension> Public Function PythonRawRegexp(raw As String) As Regex
        Return New Regex(raw, RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace)
    End Function

    ''' <summary>
    ''' 函数返回以1为底的位置，当找不到的时候会返回零
    ''' </summary>
    ''' <param name="str$"></param>
    ''' <param name="pattern$"></param>
    ''' <param name="opt"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Locates(str$, pattern$, Optional opt As RegexOptions = RegexICSng) As Integer
        Dim sub$ = Regex.Match(str, pattern, opt).Value

        If String.IsNullOrEmpty([sub]) Then
            Return 0
        Else
            Return InStr(str, [sub], CompareMethod.Binary)
        End If
    End Function
End Module
