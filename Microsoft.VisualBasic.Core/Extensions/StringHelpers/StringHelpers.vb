#Region "Microsoft.VisualBasic::7c29662f4454da7bf5f2c1a7925ba40b, Microsoft.VisualBasic.Core\Extensions\StringHelpers\StringHelpers.vb"

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

    ' Module StringHelpers
    ' 
    '     Properties: EmptyString, NonStrictCompares, StrictCompares
    ' 
    '     Function: __json, AllEquals, AsciiBytes, (+4 Overloads) ByteString, CharAtOrDefault
    '               CharString, (+3 Overloads) Count, CreateBuilder, DistinctIgnoreCase, EqualsAny
    '               First, FormatString, FormatZero, GetBetween, GetEMails
    '               GetStackValue, GetString, (+2 Overloads) GetTagValue, GetURLs, IgnoreCase
    '               InStrAny, (+2 Overloads) Intersection, IsEmptyStringVector, JoinBy, LineTokens
    '               Located, Lookup, (+2 Overloads) Match, Matches, MatchPattern
    '               (+2 Overloads) MaxLengthString, NotEmpty, PadEnd, Parts, RepeatString
    '               ReplaceChars, (+2 Overloads) Reverse, RNull, SaveTo, (+2 Overloads) Split
    '               SplitBy, StringEmpty, StringHashCode, StringReplace, StringSplit
    '               StripBlank, Strips, TextEquals, TextLast, TokenCount
    '               TokenCountIgnoreCase, TrimNewLine, TrimNull, WildcardsLocated
    ' 
    '     Sub: Parts, RemoveLast
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Math.Information
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Patterns
Imports r = System.Text.RegularExpressions.Regex

''' <summary>
''' The extensions module for facilities the string operations.
''' </summary>
<Package("StringHelpers", Publisher:="amethyst.asuka@gcmodeller.org", Url:="http://gcmodeller.org")>
Public Module StringHelpers

    Public Const CaseInsensitive As StringComparison = StringComparison.OrdinalIgnoreCase

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function PadEnd(str$, padLen%, Optional padString As Char = " "c) As String
        Return str.PadRight(padLen, padString)
    End Function

    ''' <summary>
    ''' 将字符串中的所有的<see cref="ASCII.NUL"/>给移除
    ''' </summary>
    ''' <param name="str"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function TrimNull(str As String) As String
        If str Is Nothing Then
            Return Nothing
        Else
            Return str.Trim(ASCII.NUL)
        End If
    End Function

    <Extension>
    Public Function CharAtOrDefault(s$, index%, Optional [default] As Char = ASCII.NUL) As Char
        If s.Length <= index Then
            Return [default]
        Else
            Return s(index)
        End If
    End Function

    ''' <summary>
    ''' Get the first char of the target <see cref="StringBuilder"/> 
    ''' </summary>
    ''' <param name="sb"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function First(sb As StringBuilder) As Char
        Return sb.Chars(Scan0)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CreateBuilder(s As String) As StringBuilder
        Return New StringBuilder(s)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function IgnoreCase(flag As Boolean) As CompareMethod
        Return If(flag, CompareMethod.Text, CompareMethod.Binary)
    End Function

    ''' <summary>
    ''' Using <see cref="[String].Empty"/> as default value
    ''' </summary>
    Public ReadOnly Property EmptyString As [Default](Of String) = String.Empty

    ''' <summary>
    ''' Replace the <see cref="vbCrLf"/> with the specific string.
    ''' </summary>
    ''' <param name="src"></param>
    ''' <param name="replacement"></param>
    ''' <returns></returns>
    <Extension> Public Function TrimNewLine(src$, <Parameter("vbCrLf.Replaced")> Optional replacement$ = " ") As String
        If src Is Nothing Then
            Return ""
        End If

        src = src.Replace(vbCrLf, replacement) _
                 .Replace(vbCr, replacement) _
                 .Replace(vbLf, replacement) _
                 .Replace("  ", " ")

        Return Strings.Trim(src)
    End Function

    <Extension>
    Public Function ReplaceChars(src$, chars As IEnumerable(Of Char), replaceAs As Char) As String
        Dim s As New StringBuilder(src)

        For Each c As Char In chars
            Call s.Replace(c, replaceAs)
        Next

        Return s.ToString
    End Function

    ''' <summary>
    ''' 判断这个字符串数组之中的所有的元素都是空字符串？
    ''' </summary>
    ''' <param name="s$">字符串数组</param>
    ''' <returns></returns>
    <Extension> Public Function IsEmptyStringVector(s$(), Optional RNull As Boolean = False) As Boolean
        If RNull Then
            Return s _
                .Where(AddressOf StringHelpers.RNull) _
                .Count = s.Length
        Else
            Return s.Where(Function(c) Not c.StringEmpty).Count = 0
        End If
    End Function

    ''' <summary>
    ''' Is text equals to the R nothing?
    ''' </summary>
    ''' <param name="c$"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function RNull(c$) As Boolean
        Return c.StringEmpty OrElse
               c.TextEquals("NULL") OrElse
               c.TextEquals("NA")
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsciiBytes(str As String) As Byte()
        Return Encoding.ASCII.GetBytes(str)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AllEquals(s As IEnumerable(Of String), str$) As Boolean
        Return s.All(Function(x) x = str)
    End Function

    ''' <summary>
    ''' https://github.com/darkskyapp/string-hash
    ''' </summary>
    ''' <param name="s$"></param>
    ''' <returns></returns>
    <Extension> Public Function StringHashCode(s As String) As Long
        Dim hash& = 5381
        Dim chars%() = s.Select(AddressOf Convert.ToInt32).ToArray

        For i As Integer = s.Length - 1 To 0 Step -1
            hash = (New BigInteger(hash) * 33 Xor chars(i)).ToTruncateInt64
        Next

        hash = hash >> 0

        Return hash
    End Function

    ''' <summary>
    ''' 常用于gdi+绘图操作，和<see cref="Graphics.MeasureString"/>共同使用
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <Extension>
    Public Function MaxLengthString(source As IEnumerable(Of String)) As String
        Return source.OrderByDescending(Function(s) Len(s)).First
    End Function

    ''' <summary>
    ''' 获取最大长度的字符串
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="getString"></param>
    ''' <returns></returns>
    <Extension>
    Public Function MaxLengthString(Of T)(source As IEnumerable(Of T), getString As Func(Of T, String)) As String
        Return source.Select(getString).MaxLengthString
    End Function

    ''' <summary>
    ''' 将<paramref name="replaces"/>列表之中的字符串都替换为空字符串
    ''' </summary>
    ''' <param name="s$"></param>
    ''' <param name="replaces"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Strips(s$, replaces As IEnumerable(Of String)) As String
        Dim sb As New StringBuilder(s)

        For Each r As String In replaces
            Call sb.Replace(r, "")
        Next

        Return sb.ToString
    End Function

    ''' <summary>
    ''' 将一个任意的目标字符集合转换为字符串对象
    ''' </summary>
    ''' <param name="chs"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CharString(chs As IEnumerable(Of Char)) As String
        Return New String(chs.ToArray)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ByteString(bytes As Byte()) As String
        Return bytes.ByteString(0, bytes.Length)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ByteString(bytes As Byte(), index As Integer, count As Integer) As String
        Return Encoding.UTF8.GetString(DirectCast(DirectCast(bytes, Object), Byte()), index, count)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ByteString(bytes As Byte(), encoding As String) As String
        Return bytes.ByteString(Scan0, bytes.Length, encoding)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ByteString(bytes As Byte(), index As Integer, count As Integer, encoding As String) As String
        Return System.Text.Encoding.GetEncoding(encoding).GetString(DirectCast(DirectCast(bytes, Object), Byte()), index, count)
    End Function

    '
    ' Summary:
    '     Replaces the format item in a specified string with the string representation
    '     of a corresponding object in a specified array.
    '
    ' Parameters:
    '   format:
    '     A composite format string.
    '
    '   args:
    '     An object array that contains zero or more objects to format.
    '
    ' Returns:
    '     A copy of format in which the format items have been replaced by the string representation
    '     of the corresponding objects in args.
    '
    ' Exceptions:
    '   T:System.ArgumentNullException:
    '     format or args is null.
    '
    '   T:System.FormatException:
    '     format is invalid.-or- The index of a format item is less than zero, or greater
    '     than or equal to the length of the args array.

    ''' <summary>
    ''' Replaces the format item in a specified string with the string representation
    ''' of a corresponding object in a specified array.
    ''' </summary>
    ''' <param name="s">A composite format string.</param>
    ''' <param name="args">An object array that contains zero or more objects to format.</param>
    ''' <returns>A copy of format in which the format items have been replaced by the string representation
    ''' of the corresponding objects in args.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <DebuggerStepThrough>
    <Extension>
    Public Function FormatString(s$, ParamArray args As Object()) As String
        Return String.Format(s, args)
    End Function

    ''' <summary>
    ''' this is to emulate what's evailable in PHP
    ''' </summary>
    <Extension>
    Public Function RepeatString(text$, count%) As String
        Dim sb = New StringBuilder(text.Length * count)
        For i As Integer = 0 To count - 1
            Call sb.Append(text)
        Next
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Join and contact the text tokens with a specific <paramref name="delimiter"/> string.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="data"></param>
    ''' <param name="delimiter"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function JoinBy(Of T)(data As IEnumerable(Of T), delimiter$, Optional toString As Func(Of T, String) = Nothing) As String
        If toString Is Nothing Then
            toString = Function(o) Scripting.ToString(o)
        End If

        Return data _
            .SafeQuery _
            .Select(toString) _
            .DoCall(Function(strs)
                        Return String.Join(delimiter, strs.ToArray)
                    End Function)
    End Function

    ''' <summary>
    ''' Text parser for the format: ``tagName{<paramref name="delimiter"/>}value``
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="delimiter"></param>
    ''' <param name="trim">Needs Removes all leading and trailing white-space characters from 
    ''' the current <see cref="System.String"/> object.</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GetTagValue(s$, Optional delimiter$ = " ", Optional trim As Boolean = False, Optional failureNoName As Boolean = True) As NamedValue(Of String)
        Return s.GetTagValue(delimiter, trim:=If(trim, " ", Nothing), failureNoName:=failureNoName)
    End Function

    ''' <summary>
    ''' Text parser for the format: ``tagName{<paramref name="delimiter"/>}value``
    ''' </summary>
    ''' <param name="s$"></param>
    ''' <param name="delimiter$"></param>
    ''' <param name="trim">Chars collection for <see cref="String.Trim"/> function</param>
    ''' <param name="failureNoName"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetTagValue(s$, delimiter$, trim$, Optional failureNoName As Boolean = True) As NamedValue(Of String)
        If s.StringEmpty Then
            Return Nothing
        End If

        Dim p% = InStr(s, delimiter, CompareMethod.Text)

        If p = 0 Then
            If failureNoName Then
                Return New NamedValue(Of String)("", s, s)
            Else
                Return New NamedValue(Of String)(s, "", s)
            End If
        Else
            Dim key$ = Mid(s, 1, p - 1)
            Dim value$ = Mid(s, p + delimiter.Length)

            If Not trim.StringEmpty(whitespaceAsEmpty:=False) Then
                With trim.ToArray
                    value = value.Trim(.ByRef)
                    key = key.Trim(.ByRef)
                End With
            End If

            Return New NamedValue(Of String)(key, value, s)
        End If
    End Function

    <Extension>
    Public Function StripBlank(s$, Optional includeNewline As Boolean = True) As String
        If includeNewline Then
            Return s.Trim(" "c, ASCII.TAB, ASCII.LF, ASCII.CR)
        Else
            Return s.Trim(" "c, ASCII.TAB)
        End If
    End Function

    ''' <summary>
    ''' Shortcuts for method <see cref="System.String.Equals"/>(s1, s2, <see cref="StringComparison.OrdinalIgnoreCase"/>)
    ''' </summary>
    ''' <param name="s1"></param>
    ''' <param name="s2"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function TextEquals(s1$, s2$) As Boolean
        'If {s1, s2}.All(Function(s) s Is Nothing) Then
        '    Return True ' null = null ??
        'End If
        Return String.Equals(s1, s2, StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' <see cref="RegexOptions.IgnoreCase"/> + <see cref="RegexOptions.Singleline"/> 
    ''' </summary>
    Public Const RegexICSng As RegexOptions = RegexOptions.IgnoreCase Or RegexOptions.Singleline

    ''' <summary>
    ''' <see cref="RegexOptions.IgnoreCase"/> + <see cref="RegexOptions.Multiline"/> 
    ''' </summary>
    Public Const RegexICMul As RegexOptions = RegexOptions.IgnoreCase Or RegexOptions.Multiline

    ''' <summary>
    ''' <paramref name="s"/> Is Nothing, <see cref="System.String.IsNullOrEmpty"/>, <see cref="System.String.IsNullOrWhiteSpace"/>
    ''' </summary>
    ''' <param name="s">The input test string</param>
    ''' <returns></returns>
    <Extension> Public Function StringEmpty(s$, Optional whitespaceAsEmpty As Boolean = True) As Boolean
        If s Is Nothing OrElse String.IsNullOrEmpty(s) Then
            Return True
        Else
            If String.IsNullOrWhiteSpace(s) Then
                Return whitespaceAsEmpty
            Else
                Return False
            End If
        End If
    End Function

    ''' <summary>
    ''' Not <see cref="StringEmpty(String, Boolean)"/>
    ''' </summary>
    ''' <param name="s$"></param>
    ''' <param name="whitespaceAsEmpty"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function NotEmpty(s$, Optional whitespaceAsEmpty As Boolean = True) As Boolean
        Return Not s.StringEmpty(whitespaceAsEmpty)
    End Function

    ''' <summary>
    ''' Call <see cref="StringBuilder.Remove"/>(<see cref="StringBuilder.Length"/> - 1, 1) for removes the last character in the string sequence.
    ''' </summary>
    ''' <param name="s"></param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Sub RemoveLast(s As StringBuilder)
        Call s.Remove(s.Length - 1, 1)
    End Sub

    ''' <summary>
    ''' Returns a reversed version of String s.
    ''' </summary>
    ''' <param name="sb"></param>
    ''' <returns></returns>
    <Extension> Public Function Reverse(ByRef sb As StringBuilder) As StringBuilder
        Dim s As String = New String(sb.ToString.Reverse.ToArray)
        sb = New StringBuilder(s)
        Return sb
    End Function

    ''' <summary>
    ''' Returns a reversed version of String s.
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Reverse(s As String) As String
        Return New String(s.Reverse.ToArray)
    End Function

    Public ReadOnly Property StrictCompares As StringComparison = StringComparison.Ordinal
    ''' <summary>
    ''' String compares with ignored chars' case.(忽略大小写为非严格的比较)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property NonStrictCompares As StringComparison = StringComparison.OrdinalIgnoreCase

    ''' <summary>
    ''' Split long text data into seperate lines by the specific <paramref name="len"/> value.
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="len"></param>
    ''' <returns></returns>
    ''' <remarks>Using for the Fasta sequence writer.</remarks>
    <ExportAPI("s.Parts")>
    <Extension> Public Function Parts(s$, len%) As String
        Dim sbr As New StringBuilder
        Call Parts(s, len, sbr)
        Return sbr.ToString
    End Function

    Public Sub Parts(s As String, len As String, ByRef sbr As StringBuilder)
        Do While Not String.IsNullOrEmpty(s)
            Call sbr.Append(Mid(s, 1, len))
            s = Mid(s, len + 1)
            If String.IsNullOrEmpty(s) Then
                Return
            End If
            Dim fs As Integer = InStr(s, " ")

            If fs = 0 Then
                Call sbr.AppendLine(s)
                Return
            End If

            Dim Firts As String = Mid(s, 1, fs - 1)
            s = Mid(s, fs + 1)
            Call sbr.AppendLine(Firts)
        Loop
    End Sub

    ''' <summary>
    ''' Regex expression for parsing E-Mail URL
    ''' </summary>
    Const REGEX_EMAIL As String = "[a-z0-9\._-]+@[a-z0-9\._-]+"
    ''' <summary>
    ''' Regex exprression for parsing the http/ftp URL
    ''' </summary>
    Const REGEX_URL As String = "(ftp|http(s)?)[:]//[a-z0-9\.-_]+\.[a-z]+/*[^""]*"

    <ExportAPI("Parsing.E-Mails")>
    Public Function GetEMails(s As String) As String()
        Dim values$() = Regex _
            .Matches(s, REGEX_EMAIL, RegexICSng) _
            .ToArray
        Return values
    End Function

    <ExportAPI("Parsing.URLs")>
    Public Function GetURLs(s As String) As String()
        Dim values$() = Regex _
            .Matches(s, REGEX_URL, RegexICSng) _
            .ToArray
        Return values
    End Function

    ''' <summary>
    ''' Counts the specific char that appeared in the input string.
    ''' (计数在字符串之中所出现的指定的字符的出现的次数)
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="ch"></param>
    ''' <returns></returns>
    '''
    <Extension> Public Function Count(str$, ch As Char) As Integer
        If String.IsNullOrEmpty(str) Then
            Return 0
        Else
            Return str.Count(Function(c) c = ch)
        End If
    End Function

    ''' <summary>
    ''' 计算目标字符串在序列之中出现的次数
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="target$"></param>
    ''' <param name="method"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Count(source As IEnumerable(Of String), target$, Optional method As StringComparison = StringComparison.Ordinal) As Integer
        Return source _
            .Where(Function(s) String.Equals(s, target, method)) _
            .Count
    End Function

    ''' <summary>
    ''' Count the phrase in <paramref name="text"/>
    ''' </summary>
    ''' <param name="text$"></param>
    ''' <param name="phrase$"></param>
    ''' <param name="method"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Count(text$, phrase$, Optional method As CompareMethod = CompareMethod.Binary) As Integer
        Dim n%
        Dim pos% = InStr(text, phrase, method)

        Do While pos > 0
            n += 1
            pos = InStr(pos + 1, text, phrase, method)
        Loop

        Return n
    End Function

    ''' <summary>
    ''' 获取""或者其他字符所包围的字符串的值，请注意，假若只有一个<paramref name="wrapper"/>的话，字符串将不会进行任何处理
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="wrapper"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Wrapper.Trim")>
    <Extension> Public Function GetString(s$, Optional wrapper As Char = ASCII.Quot) As String
        If String.IsNullOrEmpty(s) OrElse Len(s) = 1 Then
            Return s
        End If
        If s.First = wrapper AndAlso s.Last = wrapper Then
            Return Mid(s, 2, Len(s) - 2)
        Else
            Return s
        End If
    End Function

    ''' <summary>
    ''' Get sub string value from the region that between the <paramref name="left"/> and <paramref name="right"/>.
    ''' (这个函数是直接分别查找左右两边的定位字符串来进行切割的) 
    ''' </summary>
    ''' <param name="str$"></param>
    ''' <param name="left$"></param>
    ''' <param name="right$"></param>
    ''' <returns></returns>
    <ExportAPI("Get.Stackvalue")>
    <Extension>
    Public Function GetStackValue(str$, left$, right$) As String
        If Len(str) < 2 Then
            Return ""
        End If

        Dim p As Integer = InStr(str, left) + left.Length
        Dim q As Integer = InStrRev(str, right)

        If p = 0 Or q = 0 Then
            Return str
        ElseIf p >= q Then
            Return ""
        Else
            str = Mid(str, p, q - p)
            Return str
        End If
    End Function

    ''' <summary>
    ''' 和<see cref="GetStackValue(String, String, String)"/>相似，这个函数也是查找起始和终止字符串之间的字符串，
    ''' 但是这个函数是查找相邻的两个标记，而非像<see cref="GetStackValue(String, String, String)"/>函数一样
    ''' 直接查找字符串的两端的定位结果
    ''' </summary>
    ''' <param name="str$"></param>
    ''' <param name="strStart$"></param>
    ''' <param name="strEnd$"></param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function GetBetween(str$, strStart$, strEnd$) As String
        Dim start%, end%

        If str.StringEmpty Then
            Return Nothing
        End If

        If str.Contains(strStart) AndAlso str.Contains(strEnd) Then
            start = str.IndexOf(strStart, 0) + strStart.Length
            [end] = str.IndexOf(strEnd, start)

            Return str.Substring(start, [end] - start)
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' 在字符串前面填充指定长度的0字符序列，假若输入的字符串长度大于fill的长度，则不再进行填充
    ''' </summary>
    ''' <typeparam name="T">限定类型为字符串或者数值基础类型</typeparam>
    ''' <param name="n"></param>
    ''' <param name="fill"></param>
    ''' <returns></returns>
    <ExportAPI("FormatZero")>
    <Extension> Public Function FormatZero(Of T As {IComparable(Of T)})(n As T, Optional fill$ = "00") As String
        Dim s As String = n.ToString
        Dim d As Integer = Len(fill) - Len(s)

        If d < 0 Then
            Return s
        Else
            Return Mid(fill, 1, d) & s
        End If
    End Function

    ''' <summary>
    ''' 求交集
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Intersection")>
    <Extension> Public Function Intersection(source As IEnumerable(Of IEnumerable(Of String))) As String()
        Dim stringMatrix = source.Select(Function(seq) seq.ToArray).ToArray
        ' 获取并集，接下来需要从并集之中去除在两个集合之中都不存在的
        Dim union As List(Of String) = stringMatrix.IteratesALL.Distinct.ToList

        For Each line As Index(Of String) In stringMatrix.Select(Function(seq) seq.Indexing)
            For Each row In source
                ' 遍历每一个集合
                Dim LQuery As IEnumerable(Of String) = From s As String
                                                       In row
                                                       Where line(s) = -1
                                                       Select s
                ' 假若line之中存在不存在的元素，则从并集之中移除
                For Each s As String In LQuery
                    Call union.Remove(s)
                Next
            Next
        Next

        ' 剩下的元素都是在所有的序列之中都存在的，既交集元素
        Return union.ToArray
    End Function

    ''' <summary>
    ''' 求交集
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Intersection")>
    Public Function Intersection(ParamArray values$()()) As String()
        Return values.Intersection
    End Function

    ''' <summary>
    ''' Does this input string is matched by the specific regex expression?
    ''' (判断所输入的整个字符串是否为进行判断的<paramref name="regex"/>模式，
    ''' 即使用正则表达式所匹配的结果字符串和所输入的字符串一致)
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="regex"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("Matched?")>
    <Extension> Public Function MatchPattern(str$, regex$, Optional opt As RegexOptions = RegexICSng) As Boolean
        If str.StringEmpty Then
            Return False
        Else
            Return r.Match(str, regex, opt).Success
        End If
    End Function

    ''' <summary>
    ''' Searches the specified input string for the first occurrence of the specified regular expression.
    ''' </summary>
    ''' <param name="input">The string to search for a match.</param>
    ''' <param name="pattern">The regular expression pattern to match.</param>
    ''' <param name="options"></param>
    ''' <returns></returns>
    <Extension> Public Function Match(<Parameter("input", "The string to search for a match.")> input$,
                                      <Parameter("Pattern", "The regular expression pattern to match.")> pattern$,
                                      Optional options As RegexOptions = RegexOptions.Multiline) As String
        If input.StringEmpty Then
            Return ""
        Else
            Return r.Match(input, pattern, options).Value
        End If
    End Function

    ''' <summary>
    ''' Get regex match value from the target input string.
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="pattern"></param>
    ''' <param name="options"></param>
    ''' <returns></returns>
    <ExportAPI("Match")>
    <Extension> Public Function Match(input As Match, pattern$, Optional options As RegexOptions = RegexOptions.Multiline) As String
        Return r.Match(input.Value, pattern, options).Value
    End Function

    ''' <summary>
    ''' Regular expression pattern text token matches
    ''' </summary>
    ''' <param name="input$"></param>
    ''' <param name="pattern$"></param>
    ''' <param name="options"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Matches(input$, pattern$, Optional options As RegexOptions = RegexICSng) As IEnumerable(Of String)
        If input Is Nothing OrElse input.Length = 0 Then
            Return {}
        Else
            Return r.Matches(input, pattern, options).EachValue
        End If
    End Function

    ''' <summary>
    ''' Save this string dictionary object as json file.
    ''' </summary>
    ''' <param name="dict"></param>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 其实，对于字典类型是可以直接使用JSON序列化得到json字符串的，但是在这里是需要
    ''' 保存接口类型的对象，但是在这里不能够将接口类型进行json序列化，所以进行字符串
    ''' 的序列化然后拼接出json数据
    ''' </remarks>
    <Extension>
    <ExportAPI("Write.Dictionary")>
    Public Function SaveTo(dict As IDictionary(Of String, String), path$) As Boolean
        Dim lines$() = dict.Select(AddressOf __json).ToArray
        Dim json$ = "{" &
            vbTab & String.Join("," & vbCrLf & vbTab, lines) &
        "}"

        Return json.SaveTo(path, TextEncodings.UTF8WithoutBOM)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function __json(x As KeyValuePair(Of String, String)) As String
        Return x.Key.GetJson & ": " & x.Value.GetJson
    End Function

    ''' <summary>
    ''' Count the string value numbers.(请注意，这个函数是倒序排序的)
    ''' </summary>
    ''' <param name="tokens"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function TokenCount(tokens As IEnumerable(Of String), Optional ignoreCase As Boolean = False) As Dictionary(Of String, Integer)
        If Not ignoreCase Then
            ' 大小写敏感
            With From s As String
                 In tokens
                 Select s
                 Group s By s Into Count

                Return .ToDictionary(Function(x) x.s,
                                     Function(x)
                                         Return x.Count
                                     End Function)
            End With
        Else
            Return tokens.TokenCountIgnoreCase
        End If
    End Function

    <Extension>
    Public Function TokenCountIgnoreCase(tokens As IEnumerable(Of String)) As Dictionary(Of String, Integer)
        With tokens.ToArray
            Dim uniques = From s As String
                          In .Distinct
                          Let data As String = s
                          Select UNIQUE_KEY = s.ToLower, data
                          Group By UNIQUE_KEY Into Group

            Dim LQuery = From ustr
                         In uniques
                         Let s As String = ustr.UNIQUE_KEY
                         Let count As Integer = .Count(s, StringComparison.OrdinalIgnoreCase)
                         Select key = ustr.Group.First.data, count
                         Order By count Descending

            Dim result = LQuery.ToDictionary(
                Function(x) x.key,
                Function(x) x.count)

            Return result
        End With
    End Function

    ''' <summary>
    ''' This method is used to replace most calls to the Java <see cref="[String].Split"/> method.
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="pattern"><see cref="Regex"/> patterns</param>
    ''' <param name="trimTrailingEmptyStrings"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function StringSplit(source$, pattern$,
                                Optional TrimTrailingEmptyStrings As Boolean = False,
                                Optional opt As RegexOptions = RegexICSng) As String()

        If source.StringEmpty Then
            Return {}
        End If

        Dim splitArray$() = Regex.Split(source, pattern, options:=opt)

        If Not TrimTrailingEmptyStrings OrElse splitArray.Length <= 1 Then
            Return splitArray
        Else
            Return splitArray _
                .Where(Function(s)
                           Return Not String.IsNullOrEmpty(s)
                       End Function) _
                .ToArray
        End If
    End Function

    ''' <summary>
    ''' Alias for <see cref="Strings.Split(String, String, Integer, CompareMethod)"/>
    ''' </summary>
    ''' <param name="str$"></param>
    ''' <param name="deli$"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function SplitBy(str$, deli$, Optional compares As CompareMethod = CompareMethod.Text) As String()
        Return Strings.Split(str, deli, Compare:=compares)
    End Function

    ''' <summary>
    ''' 将正则匹配成功的字符串替换为指定的目标字符串：<paramref name="replaceAs"/>
    ''' </summary>
    ''' <param name="s$"></param>
    ''' <param name="pattern$"></param>
    ''' <param name="replaceAs$"></param>
    ''' <param name="opt"></param>
    ''' <returns></returns>
    <Extension>
    Public Function StringReplace(s$, pattern$, replaceAs$, Optional opt As RegexOptions = RegexICSng) As String
        If Not s Is Nothing Then
            Dim targets$() = r.Matches(s, pattern, opt).ToArray
            Dim sb As New StringBuilder(s)

            For Each t As String In targets
                Call sb.Replace(t, replaceAs)
            Next

            Return sb.ToString
        Else
            Return ""
        End If
    End Function

    ''' <summary>
    ''' String collection tokenized by a certain delimiter string element.
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="delimiter">
    ''' Using ``String.Equals`` or Regular expression function to determined this delimiter 
    ''' </param>
    ''' <returns></returns>
    <Extension> Public Function Split(source As IEnumerable(Of String),
                                      delimiter$,
                                      Optional regex As Boolean = False,
                                      Optional opt As RegexOptions = RegexOptions.Singleline) As IEnumerable(Of String())

        Dim delimiterTest As Predicate(Of String)

        If regex Then
            With New Regex(delimiter, opt)
                delimiterTest = Function(line)
                                    Return .Match(line).Value = line
                                End Function
            End With
        Else
            delimiterTest = Function(line)
                                Return String.Equals(delimiter, line, StringComparison.Ordinal)
                            End Function
        End If

        Return source.Split(delimiterTest, includes:=False)
    End Function

    ''' <summary>
    ''' 这个函数适合将一个很大的数组进行分割
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="delimiterPredicate">分隔符断言，判断当前的对象是不是分隔符</param>
    ''' <param name="includes"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function Split(source As IEnumerable(Of String),
                                   delimiterPredicate As Predicate(Of String),
                                   Optional includes As Boolean = True) As IEnumerable(Of String())

        Dim list As New List(Of String)
        ' first line
        Dim isFirst As Boolean = True

        For Each line As String In source
            If True = delimiterPredicate(line) Then
                If isFirst Then
                    isFirst = False
                Else
                    Yield list.ToArray

                    list.Clear()
                End If

                If includes Then
                    list.Add(line)
                End If
            Else
                Call list.Add(line)
            End If

            isFirst = False
        Next

        If list.Count > 0 Then
            Yield list.ToArray
        End If
    End Function

    ''' <summary>
    ''' String compares using <see cref="String.Equals"/>, if the target value could not be located, 
    ''' then -1 will be return from this function.
    ''' </summary>
    ''' <param name="collection"></param>
    ''' <param name="text"></param>
    ''' <param name="caseSensitive"></param>
    ''' <param name="fuzzy">
    ''' If fuzzy, then <see cref="InStr"/> will be used if ``String.Equals`` method have no result.
    ''' </param>
    ''' <returns></returns>
    <Extension> Public Function Located(collection As IEnumerable(Of String), text$,
                                        Optional caseSensitive As Boolean = True,
                                        Optional fuzzy As Boolean = False) As Integer

        Dim method As StringComparison = StringComparison.OrdinalIgnoreCase Or StringComparison.Ordinal.When(caseSensitive)
        Dim method2 As CompareMethod = CompareMethod.Text Or CompareMethod.Binary.When(caseSensitive)

        For Each str As SeqValue(Of String) In collection.SeqIterator
            If String.Equals(str.value, text, method) Then
                Return str.i
            ElseIf fuzzy Then
                If InStr(str.value, text, method2) > 0 Then
                    Return str.i
                End If
            End If
        Next

        Return -1
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="collection"></param>
    ''' <param name="text">可以使用通配符</param>
    ''' <param name="caseSensitive"></param>
    ''' <returns></returns>
    <Extension>
    Public Function WildcardsLocated(collection As IEnumerable(Of String), text$, Optional caseSensitive As Boolean = True) As Integer
        For Each s As SeqValue(Of String) In collection.SeqIterator
            If text.WildcardMatch(s.value, Not caseSensitive) Then
                Return s.i
            End If
        Next

        Return -1
    End Function

    ''' <summary>
    ''' Search the string by keyword in a string collection. Unlike search function <see cref="StringHelpers.Located(IEnumerable(Of String), String, Boolean, Boolean)"/>
    ''' using function <see cref="String.Equals"/> function to search string, this function using <see cref="Strings.InStr(String, String, CompareMethod)"/>
    ''' to search the keyword.
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="keyword"></param>
    ''' <param name="caseSensitive"></param>
    ''' <returns>返回第一个找到关键词的行数，没有找到则返回-1</returns>
    <Extension>
    Public Function Lookup(source As IEnumerable(Of String), keyword As String, Optional caseSensitive As Boolean = True) As Integer
        Dim method As CompareMethod = If(caseSensitive, CompareMethod.Binary, CompareMethod.Text)
        Dim i As Integer

        For Each line As String In source
            If InStr(line, keyword, method) > 0 Then
                Return i
            Else
                i += 1
            End If
        Next

        Return -1
    End Function

    ''' <summary>
    ''' 判断目标字符串是否与字符串参数数组之中的任意一个字符串相等，大小写不敏感，假若没有相等的字符串，则会返回空字符串，假若找到了相等的字符串，则会返回该字符串
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="compareTo"></param>
    ''' <returns></returns>
    <Extension>
    <ExportAPI("Equals.Any")>
    Public Function EqualsAny(source$, ParamArray compareTo As String()) As String
        Dim index As Integer = compareTo.Located(source, False)

        If index = -1 Then
            Return ""
        Else
            Return compareTo(index)
        End If
    End Function

    ''' <summary>
    ''' 查找到任意一个既返回位置，大小写不敏感，假若查找不到，则返回-1值，判断是否查找成功，可以使用 &lt;0 来完成，
    ''' 因为是通过InStr来完成的，所以查找成功的时候，最小的值是1，即字符串序列的第一个位置，也是元素0位置
    ''' </summary>
    ''' <param name="text"></param>
    ''' <param name="find"></param>
    ''' <returns></returns>
    <ExportAPI("InStr.Any")>
    <Extension>
    Public Function InStrAny(text$, ParamArray find$()) As Integer
        For Each token As String In find
            Dim idx% = Strings.InStr(text, token, CompareMethod.Text)

            If idx > 0 Then
                Return idx
            End If
        Next

        Return -1
    End Function

    ''' <summary>
    ''' Removes the duplicated string from the source <paramref name="strings"/> collection 
    ''' with string compare ignore case.
    ''' </summary>
    ''' <param name="strings"></param>
    ''' <returns></returns>
    <Extension>
    Public Function DistinctIgnoreCase(strings As IEnumerable(Of String)) As IEnumerable(Of String)
        Dim list = strings.Distinct.ToArray
        Dim lowerBuffers As New Dictionary(Of String, List(Of String))

        For Each s As String In list
            With s.ToLower
                If Not lowerBuffers.ContainsKey(.ByRef) Then
                    lowerBuffers(.ByRef) = New List(Of String)
                End If
                lowerBuffers(.ByRef).Add(s)
            End With
        Next

        Dim distinct = lowerBuffers _
            .Select(Function(pack)
                        Dim n$() = pack _
                            .Value _
                            .Where(Function(s) s <> pack.Key) _
                            .ToArray

                        ' 尽量不返回全部都是小写的字符串
                        If n.Length > 0 Then
                            Return n.First
                        Else
                            Return pack.Key
                        End If
                    End Function) _
            .ToArray

        Return distinct
    End Function

    ''' <summary>
    ''' Line tokens. **=> Parsing the text into lines by using <see cref="vbCr"/>, <see cref="vbLf"/>**.
    ''' (函数对文本进行分行操作，由于在Windows(<see cref="VbCrLf"/>)和
    ''' Linux(<see cref="vbCr"/>, <see cref="vbLf"/>)平台上面所生成的文本文件的换行符有差异，
    ''' 所以可以使用这个函数来进行统一的分行操作)
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    ''' <param name="trim">
    ''' Set <see cref="Boolean.FalseString"/> to avoid a reader bug in the csv data reader 
    ''' </param>
    ''' <param name="escape">
    ''' 是否需要将字符串之中的``\n``转义为换行之后再进行分割？默认不进行转义
    ''' </param>
    <ExportAPI("LineTokens")>
    <Extension> Public Function LineTokens(s$, Optional trim As Boolean = True, Optional escape As Boolean = False) As String()
        If String.IsNullOrEmpty(s) Then
            Return {}
        ElseIf escape Then
            s = s.Replace("\n", ASCII.LF)
        End If

        Dim lf As Boolean = InStr(s, vbLf) > 0
        Dim cr As Boolean = InStr(s, vbCr) > 0

        If Not (cr OrElse lf) Then  ' 没有分行符，则只有一行
            Return {s}
        End If

        If lf AndAlso cr Then
            If trim Then  ' 假若将这个换行替换掉，在Csv文件读取模块会出现bug。。。。。不清楚是怎么回事
                s = s.Replace(vbCr, "")
            End If

            Return Splitter.Split(s, vbLf, True)
        End If

        If lf Then
            Return Splitter.Split(s, vbLf, True)
        Else
            Return Splitter.Split(s, vbCr, True)
        End If
    End Function

    ''' <summary>
    ''' Does the <paramref name="token"/> is located at the last of <paramref name="s"/> text string.
    ''' </summary>
    ''' <param name="s$"></param>
    ''' <param name="token$"></param>
    ''' <returns></returns>
    <Extension> Public Function TextLast(s$, token$) As Boolean
        Dim lastIndex% = s.Length - token.Length
        ' 因为token子字符串可能会在s字符串之中出现多次，所以直接使用正向的InStr函数
        ' 可能会导致匹配到第一个字符串而无法正确的匹配上最后一个token，所以在这里使用
        ' InstrRev来避免这个问题
        Dim val% = InStrRev(s, token)

        Return lastIndex = val
    End Function
End Module
