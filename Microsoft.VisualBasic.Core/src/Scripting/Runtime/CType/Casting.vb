#Region "Microsoft.VisualBasic::727db2dae867793c3c7500cd85bb4f44, Microsoft.VisualBasic.Core\src\Scripting\Runtime\CType\Casting.vb"

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

    '     Module Casting
    ' 
    '         Function: (+3 Overloads) [As], AsBaseType, CastChar, CastCharArray, CastCommandLine
    '                   CastDate, CastFileInfo, CastFont, CastGDIPlusDeviceHandle, CastImage
    '                   CastInteger, CastIPEndPoint, CastLogFile, CastLong, CastProcess
    '                   CastRegexOptions, CastSingle, CastStringBuilder, (+2 Overloads) Expression, FloatPointParser
    '                   FloatSizeParser, NumericRangeParser, ParseNumeric, PointParser, RegexParseDouble
    '                   ScriptValue, SizeParser, TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.ValueTypes

Namespace Scripting.Runtime

    ''' <summary>
    ''' Methods for convert the <see cref="String"/> to some .NET data types.
    ''' </summary>
    Public Module Casting

        ''' <summary>
        ''' Try parse of the enum value.
        ''' </summary>
        ''' <typeparam name="T">This generic type should be an <see cref="System.Enum"/> type!</typeparam>
        ''' <param name="expression"></param>
        ''' <param name="[default]"></param>
        ''' <returns></returns>
        <Extension>
        Public Function TryParse(Of T As Structure)(expression As Match, Optional [default] As T = Nothing) As T
            Dim result As T = Nothing

            If [Enum].TryParse(Of T)(expression.Value, result) Then
                Return result
            Else
                Return [default]
            End If
        End Function

        ''' <summary>
        ''' <see cref="Size"/> object to string expression
        ''' </summary>
        ''' <param name="size"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ScriptValue(size As Size) As String
            Return $"{size.Width},{size.Height}"
        End Function

        <Extension>
        Public Iterator Function [As](Of T)(source As IEnumerable) As IEnumerable(Of T)
            Dim l As New List(Of Object)

            For Each x In source
                l.Add(x)

                If l.Count > 1 Then
                    Exit For
                End If
            Next

            If l.Count = 1 AndAlso Not l.First.GetType Is GetType(T) Then

                Dim x = l.First

                ' If x.GetType() Is GetType(IEnumerator) Then
                With DirectCast(x, IEnumerator)
                    Do While .MoveNext
                        Yield DirectCast(.Current, T)
                    Loop
                End With

                'Return
                'Else
                '    source = x
                'End If
            Else
                For Each o As Object In source
                    Yield DirectCast(o, T)
                Next
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function NumericRangeParser(exp As String) As DoubleRange
            Return DoubleRange.TryParse(exp)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [As](Of T As {IComparable(Of T), Structure})(x As Double) As T
            Return CType(CObj(x), T)
        End Function

        ''' <summary>
        ''' width,height
        ''' </summary>
        ''' <param name="size"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Expression(size As Size) As String
            With size
                Return $"{ .Width},{ .Height}"
            End With
        End Function

        ''' <summary>
        ''' width,height
        ''' </summary>
        ''' <param name="size"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Expression(size As SizeF) As String
            With size
                Return $"{ .Width},{ .Height}"
            End With
        End Function

        ''' <summary>
        ''' Parse <see cref="Point"/> from a given string expression
        ''' </summary>
        ''' <param name="pt$"></param>
        ''' <returns></returns>
        Public Function PointParser(pt$) As Point
            Dim x, y As Double
            Call Ranges.Parser(pt, x, y)
            Return New Point(x, y)
        End Function

        ''' <summary>
        ''' Parse <see cref="PointF"/> from a given string expression
        ''' </summary>
        ''' <param name="pt$"></param>
        ''' <returns></returns>
        Public Function FloatPointParser(pt$) As PointF
            Dim x, y As Double
            Call Ranges.Parser(pt, x, y)
            Return New PointF(x, y)
        End Function

        ''' <summary>
        ''' Parse <see cref="Size"/> from a given string expression
        ''' </summary>
        ''' <param name="pt$"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        <Extension> Public Function SizeParser(pt$) As Size
            Return pt.FloatSizeParser.ToSize
        End Function

        ''' <summary>
        ''' Parse <see cref="SizeF"/> from a given string expression
        ''' </summary>
        ''' <param name="pt$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function FloatSizeParser(pt$) As SizeF
            If pt.StringEmpty Then
                Return Nothing
            Else
                Dim x, y As Double
                Call Ranges.Parser(pt, x, y)
                Return New SizeF(x, y)
            End If
        End Function

        ''' <summary>
        ''' ``DirectCast(obj, T)``. 这个函数主要是为了解决Class类型之间的继承类型的转换，例如子类型向基础类型转换
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 可能会和向量的As类型转换有冲突
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsBaseType(Of TIn As Class, T)(obj As TIn) As T
            If obj Is Nothing Then
                Return Nothing
            Else
                Return DirectCast(CObj(obj), T)
            End If
        End Function

        ''' <summary>
        ''' Cast array type
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="list">在这里使用向量而非使用通用接口是因为和单个元素的As转换有冲突</param>
        ''' <returns></returns>
        <Extension> Public Function [As](Of T, TOut)(list As IEnumerable(Of T)) As IEnumerable(Of TOut)
            If list Is Nothing Then
                Return {}
            Else
                Return list.Select(Function(x) CType(CObj(x), TOut))
            End If
        End Function

        ''' <summary>
        ''' 用于解析出任意实数的正则表达式
        ''' </summary>
        Public Const RegexpDouble$ = "-?\d+(\.\d+)?"
        Public Const ScientificNotation$ = RegexpDouble & "[Ee][+-]\d+"
        Public Const RegexpFloat$ = RegexpDouble & "([Ee][+-]\d+)?"
        Public Const RegexInteger$ = "[-]?\d+"

        ''' <summary>
        ''' Parsing a real number from the expression text by using the regex expression <see cref="RegexpFloat"/>.
        ''' (使用正则表达式解析目标字符串对象之中的一个实数)
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Double.Match")>
        <Extension> Public Function RegexParseDouble(s As String) As Double
            Return Val(s.Match(RegexpFloat))
        End Function

        ''' <summary>
        ''' Will processing value NaN automatically and strip for the comma, percentage expression.
        ''' </summary>
        ''' <param name="s">
        ''' + numeric
        ''' + NaN, NA
        ''' + p%
        ''' + a/b
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function will mapping factor string ``NaN`` and ``NA`` as <see cref="System.Double.NaN"/>
        ''' </remarks>
        <Extension>
        Public Function ParseNumeric(s As String) As Double
            s = Strings.Trim(s)

            If String.IsNullOrEmpty(s) Then
                Return 0R
            ElseIf String.Equals(s, "NaN", StringComparison.Ordinal) OrElse
                String.Equals(s, "NA", StringComparison.Ordinal) Then

                ' R 语言之中是使用NA，.NET语言是使用NaN
                Return Double.NaN
            Else
                ' ,表示1000，需要删掉这个间隔符
                ' 才可以被正常的val出来
                s = s.Replace(",", "")
            End If

            If s.Last = "%"c Then
                Return Conversion.Val(Mid(s, 1, s.Length - 1)) / 100  ' 百分比
            ElseIf InStr(s, "/") > 0 Then
                Dim t$() = s.Split("/"c)
                ' 处理分数
                Return Val(t(0)) / Val(t(1))
            ElseIf InStr(s, "e", CompareMethod.Text) > 0 Then
                Dim t = s.ToLower.Split("e"c)
                Return Val(t(0)) * (10 ^ Val(t(1)))
            Else
                Return Conversion.Val(s)
            End If
        End Function

        ''' <summary>
        ''' 字符串是空值会返回空字符
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastChar(obj As String) As Char
            Return If(String.IsNullOrEmpty(obj), ASCII.NUL, obj.First)
        End Function

        ''' <summary>
        ''' 出错会返回默认是0
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastInteger(obj As String) As Integer
            Return CInt(ParseNumeric(obj))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastLong(obj As String) As Long
            Return CLng(ParseNumeric(obj))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastCharArray(obj As String) As Char()
            Return obj.ToArray
        End Function

        ''' <summary>
        ''' 支持日期字符串和unix timstamp对<see cref="Date"/>的转换
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastDate(obj As String) As DateTime
            If obj.StringEmpty OrElse obj = "0000-00-00 00:00:00" OrElse obj.ToUpper = "NULL" OrElse obj.ToUpper = "NA" Then
                Return New Date
            ElseIf obj.IsPattern("\d+") Then
#If NET_48 = 1 Or netcore5 = 1 Then
                ' unix timestamp
                Return CLng(Val(obj)).FromUnixTimeStamp
#Else
                Throw New NotImplementedException
#End If
            ElseIf obj.IsPattern("/Date\(\d+\+\d+\)/") Then
                ' /Date(1559115042272+0800)/
                ' json格式的
                Return DateTimeHelper.ToDate(obj.Match("\d+[+-]\d+"))
            Else
                Return DateTime.Parse(obj)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastStringBuilder(obj As String) As StringBuilder
            Return New StringBuilder(obj)
        End Function

        ''' <summary>
        ''' <see cref="CommandLine.TryParse"/>
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastCommandLine(obj As String) As CommandLine.CommandLine
            Return Parsers.TryParse(obj)
        End Function

        ''' <summary>
        ''' <see cref="LoadImage"/>
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastImage(path As String) As Image
            Return LoadImage(path)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastFileInfo(path As String) As FileInfo
            Return FileIO.FileSystem.GetFileInfo(path)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastGDIPlusDeviceHandle(path As String) As Graphics2D
            Return CanvasCreateFromImageFile(path)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastFont(face As String) As Font
            Return New Font(face, 10)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastIPEndPoint(addr As String) As System.Net.IPEndPoint
            Return New Net.IPEndPoint(addr).GetIPEndPoint
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastLogFile(path As String) As LogFile
            Return New LogFile(path)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastProcess(exe As String) As Process
            Return Process.Start(exe)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CastSingle(n As String) As Single
            Return CSng(ParseNumeric(n))
        End Function

        Public Function CastRegexOptions(name As String) As RegexOptions
            If String.Equals(name, RegexExtensions.NameOf.Compiled, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.Compiled
            ElseIf String.Equals(name, RegexExtensions.NameOf.CultureInvariant, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.CultureInvariant
            ElseIf String.Equals(name, RegexExtensions.NameOf.ECMAScript, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.ECMAScript
            ElseIf String.Equals(name, RegexExtensions.NameOf.ExplicitCapture, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.ExplicitCapture
            ElseIf String.Equals(name, RegexExtensions.NameOf.IgnoreCase, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.IgnoreCase
            ElseIf String.Equals(name, RegexExtensions.NameOf.IgnorePatternWhitespace, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.IgnorePatternWhitespace
            ElseIf String.Equals(name, RegexExtensions.NameOf.Multiline, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.Multiline
            ElseIf String.Equals(name, RegexExtensions.NameOf.RightToLeft, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.RightToLeft
            ElseIf String.Equals(name, RegexExtensions.NameOf.Singleline, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.Singleline
            Else
                Return RegexOptions.None
            End If
        End Function
    End Module
End Namespace
