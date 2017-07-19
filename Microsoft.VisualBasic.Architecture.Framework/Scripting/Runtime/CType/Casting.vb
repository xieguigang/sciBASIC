#Region "Microsoft.VisualBasic::c646f7593584b80ac439ca3b457dbf76, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Scripting\Casting.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Text

Namespace Scripting.Runtime

    ''' <summary>
    ''' Methods for convert the <see cref="System.String"/> to some .NET data types.
    ''' </summary>
    Public Module Casting

        <Extension>
        Public Function Expression(size As Size) As String
            With size
                Return $"{ .Width},{ .Height}"
            End With
        End Function

        <Extension>
        Public Function Expression(size As SizeF) As String
            With size
                Return $"{ .Width},{ .Height}"
            End With
        End Function

        Public Function PointParser(pt$) As Point
            Dim x, y As Double
            Call Ranges.Parser(pt, x, y)
            Return New Point(x, y)
        End Function

        Public Function FloatPointParser(pt$) As PointF
            Dim x, y As Double
            Call Ranges.Parser(pt, x, y)
            Return New PointF(x, y)
        End Function

        <Extension> Public Function SizeParser(pt$) As Size
            Dim x, y As Double
            Call Ranges.Parser(pt, x, y)
            Return New Size(x, y)
        End Function

        <Extension>
        Public Function FloatSizeParser(pt$) As SizeF
            Dim x, y As Double
            Call Ranges.Parser(pt, x, y)
            Return New SizeF(x, y)
        End Function

        ''' <summary>
        ''' ``DirectCast(obj, T)``
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <Extension> Public Function [As](Of T)(obj) As T
            If obj Is Nothing Then
                Return Nothing
            End If
            Return DirectCast(obj, T)
        End Function

        ''' <summary>
        ''' Cast array type
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="list">在这里使用向量而非使用通用接口是因为和单个元素的As转换有冲突</param>
        ''' <returns></returns>
        <Extension> Public Function [As](Of T, TOut)(list As T()) As TOut()
            If list.IsNullOrEmpty Then
                Return Nothing
            Else
                Return list _
                    .Select(Function(x) CType(CObj(x), TOut)) _
                    .ToArray
            End If
        End Function

        ''' <summary>
        ''' 用于解析出任意实数的正则表达式
        ''' </summary>
        Public Const RegexpDouble As String = "-?\d+(\.\d+)?"
        Public Const ScientificNotation$ = RegexpDouble & "[Ee][+-]\d+"
        Public Const RegexpFloat$ = RegexpDouble & "([Ee][+-]\d+)?"

        ''' <summary>
        ''' Parsing a real number from the expression text by using the regex expression <see cref="RegexpFloat"/>.
        ''' (使用正则表达式解析目标字符串对象之中的一个实数)
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Double.Match")>
        <Extension> Public Function RegexParseDouble(s As String) As Double
            Return Val(s.Match(RegexpFloat))
        End Function

        ''' <summary>
        ''' Will processing value NaN automatically and strip for the comma, percentage expression.
        ''' </summary>
        ''' <param name="s">
        ''' + numeric
        ''' + NaN
        ''' + p%
        ''' + a/b
        ''' </param>
        ''' <returns></returns>
        ''' 
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
                s = s.Replace(",", "")
            End If

            If s.Last = "%"c Then
                Return Conversion.Val(Mid(s, 1, s.Length - 1)) / 100  ' 百分比
            ElseIf InStr(s, "/") > 0 Then
                Dim t$() = s.Split("/"c) ' 处理分数
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
        Public Function CastChar(obj As String) As Char
            Return If(String.IsNullOrEmpty(obj), ASCII.NUL, obj.First)
        End Function

        ''' <summary>
        ''' 出错会返回默认是0
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Function CastInteger(obj As String) As Integer
            Return CInt(ParseNumeric(obj))
        End Function

        Public Function CastLong(obj As String) As Long
            Return CLng(ParseNumeric(obj))
        End Function

        Public Function CastCharArray(obj As String) As Char()
            Return obj.ToArray
        End Function

        Public Function CastDate(obj As String) As DateTime
            Return DateTime.Parse(obj)
        End Function

        Public Function CastStringBuilder(obj As String) As StringBuilder
            Return New StringBuilder(obj)
        End Function

        ''' <summary>
        ''' <see cref="CommandLine.TryParse"/>
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Function CastCommandLine(obj As String) As CommandLine.CommandLine
            Return CommandLine.TryParse(obj)
        End Function

        ''' <summary>
        ''' <see cref="LoadImage"/>
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Function CastImage(path As String) As Image
            Return LoadImage(path)
        End Function

        Public Function CastFileInfo(path As String) As FileInfo
            Return FileIO.FileSystem.GetFileInfo(path)
        End Function

        Public Function CastGDIPlusDeviceHandle(path As String) As Graphics2D
            Return GDIPlusDeviceHandleFromImageFile(path)
        End Function

        Public Function CastFont(face As String) As Font
            Return New Font(face, 10)
        End Function

        Public Function CastIPEndPoint(addr As String) As System.Net.IPEndPoint
            Return New Net.IPEndPoint(addr).GetIPEndPoint
        End Function

        Public Function CastLogFile(path As String) As Logging.LogFile
            Return New Logging.LogFile(path)
        End Function

        Public Function CastProcess(exe As String) As Process
            Return Process.Start(exe)
        End Function

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
