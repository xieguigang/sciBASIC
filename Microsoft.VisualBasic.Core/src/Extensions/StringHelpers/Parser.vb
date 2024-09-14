#Region "Microsoft.VisualBasic::970007e29e74b7425195c7a13de07787, Microsoft.VisualBasic.Core\src\Extensions\StringHelpers\Parser.vb"

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

    '   Total Lines: 426
    '    Code Lines: 240 (56.34%)
    ' Comment Lines: 136 (31.92%)
    '    - Xml Docs: 80.88%
    ' 
    '   Blank Lines: 50 (11.74%)
    '     File Size: 13.85 KB


    ' Module PrimitiveParser
    ' 
    '     Function: Eval, IsBooleanFactor, IsInteger, isNaN, IsNumeric
    '               IsSimpleNumber, (+2 Overloads) ParseBoolean, ParseDate, ParseDouble, ParseInteger
    '               ParseLong, ParseSingle, ParseTimeSpan
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Text

''' <summary>
''' Simple type parser extension function for <see cref="String"/>
''' </summary>
Public Module PrimitiveParser

    ''' <summary>
    ''' Evaluate the given string expression as numeric value 
    ''' </summary>
    ''' <param name="expression$"></param>
    ''' <param name="default#"></param>
    ''' <returns></returns>
    Public Function Eval(expression$, default#) As Double
        If expression Is Nothing Then
            Return [default]
        Else
            Return Conversion.Val(expression)
        End If
    End Function

    ''' <summary>
    ''' 用于匹配任意实数的正则表达式
    ''' 
    ''' (这个正则表达式有一个bug，会匹配上一个单独的字母E)
    ''' </summary>
    ''' <remarks>
    ''' 这个表达式并不用于<see cref="IsNumeric"/>, 但是其他的模块的代码可能会需要这个通用的表达式来做一些判断
    ''' </remarks>
    Public Const NumericPattern$ = SimpleNumberPattern & "([eE][+-]?\d*)?"

    ''' <summary>
    ''' just a pattern for the simple number, without the scientific notation pattern
    ''' </summary>
    Public Const SimpleNumberPattern$ = "[-]?\d*(\.\d+)?"

#Region "text token pattern assert"
    ' 2019-04-17 正则表达式的执行效率过低

    ''' <summary>
    ''' NA literal is comes from the Rscript environment
    ''' </summary>
    ReadOnly NaN As Index(Of String) = {
        "正无穷大", "负无穷大", "非数字",
        "Infinity", "-Infinity",
        "NaN", "NA",
        "∞", "-∞"
    }

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function isNaN(s As String) As Boolean
        Return s Like NaN
    End Function

    ''' <summary>
    ''' 这个函数相较于<see cref="PrimitiveParser.IsNumeric"/>，仅仅做简单的数值格式判断
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>is regex pattern of <see cref="SimpleNumberPattern"/></remarks>
    <Extension>
    Public Function IsSimpleNumber(num As String) As Boolean
        Return num.IsPattern(SimpleNumberPattern)
    End Function

    ''' <summary>
    ''' Is this token value string is a number?
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 这个函数会判断科学计数法等格式
    ''' </remarks>
    <Extension>
    Public Function IsNumeric(num As String,
                              Optional includesNaNFactor As Boolean = False,
                              Optional includesInteger As Boolean = False) As Boolean

        Dim dotCheck As Boolean = False
        Dim c As Char
        Dim offset As Integer = 0

        If String.IsNullOrEmpty(num) Then
            Return False
        ElseIf includesInteger AndAlso num.IsInteger Then
            Return True
        Else
            c = num(Scan0)
        End If

        ' 修复正则匹配的bug
        If num = "e" OrElse num = "E" Then
            Return False
        ElseIf includesNaNFactor AndAlso isNaN(num) Then
            Return True
        End If

        If c = "-"c OrElse c = "+"c Then
            ' check for number sign symbol
            '
            ' +3.0
            ' -3.0
            offset = 1
        ElseIf c = "."c Then
            ' check for 
            ' 
            ' .1 (0.1)
            offset = 1
            dotCheck = True
        End If

        For i As Integer = offset To num.Length - 1
            c = num(i)

            If Not c Like numbers Then
                If c = "."c Then
                    If dotCheck Then
                        Return False
                    Else
                        dotCheck = True
                    End If
                ElseIf c = "E"c OrElse c = "e"c Then
                    Return IsInteger(num, i + 1)
                Else
                    Return False
                End If
            End If
        Next

        Return True
    End Function

    ReadOnly numbers As Index(Of Char) = {"0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c}

    ''' <summary>
    ''' test the given string is in integer pattern?
    ''' </summary>
    ''' <param name="num"></param>
    ''' <param name="offset"></param>
    ''' <returns>
    ''' this function will returns true if all of the char in 
    ''' the <paramref name="num"/> string is number.
    ''' </returns>
    ''' <remarks>
    ''' this function also checks for the negative integer/long value
    ''' </remarks>
    <Extension>
    Public Function IsInteger(num As String, Optional offset As Integer = 0) As Boolean
        Dim c As Char

        If num Is Nothing OrElse num = "" Then
            Return False
        ElseIf num.Last = "E" OrElse num.Last = "e" OrElse offset >= num.Length Then
            ' 4E -> offset = 2
            ' 3545e -> offset = 5
            Return False
        Else
            c = num(offset)
        End If

        ' check for number sign symbol
        If c = "-"c OrElse c = "+"c Then
            offset += 1
        End If

        For i As Integer = offset To num.Length - 1
            c = num(i)

            If Not c Like numbers Then
                Return False
            End If
        Next

        ' 20220922 handling of 2147483647
        If num.Length = 10 AndAlso Not num.Any(Function(cc) cc = "E"c OrElse cc = "e"c) Then
            If Integer.Parse(num.First) > 2 Then
                ' is long
                Return False
            Else
                ' is possibably an integer
                ' may be long
                Return True
            End If
        ElseIf num.Length > 10 Then
            ' is long
            Return False
        End If

        Return True
    End Function
#End Region

    ''' <summary>
    ''' <see cref="Integer"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns>
    ''' this is a safe function: this function will never throw an exception
    ''' when the given <paramref name="s"/> is not a valid integer string 
    ''' value, the zero value will be return in such situation.
    ''' </returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ParseInteger(s As String) As Integer
        Return CInt(Val(Trim(s)))
    End Function

    ''' <summary>
    ''' <see cref="Long"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ParseLong(s As String) As Long
        Return CLng(Val(Trim(s)))
    End Function

    ''' <summary>
    ''' <see cref="Double"/> text parser. (这个是一个基于<see cref="ParseNumeric"/>的非常安全的字符串解析函数)
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ParseDouble(s As String) As Double
        If s Is Nothing OrElse s = "" Then
            Return 0
        Else
            Return ParseNumeric(s)
        End If
    End Function

    ''' <summary>
    ''' <see cref="Single"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ParseSingle(s As String) As Single
        Return CSng(Val(Trim(s)))
    End Function

    ''' <summary>
    ''' <see cref="Date"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ParseDate(s As String) As Date
        Dim d As Date = Nothing

        Static empty_output As Index(Of String) = {"false", "na", "null", "n/a", "nan"}

        If s.ToLower Like empty_output Then
            Return Nothing
        End If

        If Date.TryParse(Trim(s), d) Then
            Return d
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Convert the string value into the boolean value, this is useful to the text format configuration file into data model.
    ''' </summary>
    ReadOnly booleans As New SortedDictionary(Of String, Boolean) From {
                                                                        _
        {"t", True}, {"true", True},
        {"1", True},
        {"y", True}, {"yes", True}, {"ok", True},
        {"ok!", True},
        {"success", True}, {"successful", True}, {"successfully", True}, {"succeeded", True},
        {"right", True},
        {"on", True}, {"off", False},
        {"wrong", False},
        {"failure", False}, {"failures", False},
        {"exception", False},
        {"error", False}, {"err", False},
        {"f", False}, {"false", False},
        {"0", False},
        {"n", False}, {"no", False}
    }

    ''' <summary>
    ''' 目标字符串是否可以被解析为一个逻辑值
    ''' </summary>
    ''' <param name="token"></param>
    ''' <param name="extendedLiteral">
    ''' something other string factor example like ``yes`` or ``no``
    ''' will also be interpreted as a valid logical factor string 
    ''' if this parameter value is set to TRUE. default value of 
    ''' this parameter is TRUE. 
    ''' </param>
    ''' <returns>
    ''' A boolean logical factor value indicates that the given 
    ''' string <paramref name="token"/> could be parsed as a boolean
    ''' value literal or not.
    ''' </returns>
    Public Function IsBooleanFactor(token As String, Optional extendedLiteral As Boolean = True) As Boolean
        If String.IsNullOrEmpty(token) Then
            Return False
        Else
            token = token.ToLower
        End If

        If extendedLiteral Then
            Return booleans.ContainsKey(token)
        Else
            ' just test for true or false literal
            Return token = "true" OrElse token = "false"
        End If
    End Function

    ''' <summary>
    ''' Convert the string value into the boolean value, this is useful 
    ''' to the text format configuration file into data model.
    ''' (请注意，空值字符串为False，如果字符串不存在与单词表之中，则也是False)
    ''' </summary>
    ''' <param name="str">
    ''' the string literal of the target boolean value to convert.
    ''' </param>
    ''' <returns>
    ''' The boolean value which is parsed from the string literal
    ''' </returns>
    ''' <remarks>the empty string input will be treated as FALSE.</remarks>
    <Extension>
    Public Function ParseBoolean(str As String) As Boolean
        If String.IsNullOrEmpty(str) Then
            Return False
        Else
            str = str.ToLower.Trim
        End If

        If booleans.ContainsKey(key:=str) Then
            Return booleans(str)
        Else
#If DEBUG Then
            Call $"""{str}"" {NameOf([Boolean])} (null_value_definition)  ==> False".__DEBUG_ECHO
#End If
            Return False
        End If
    End Function

    ''' <summary>
    ''' Convert the logical char literal to boolean value
    ''' </summary>
    ''' <param name="ch"></param>
    ''' <returns></returns>
    <ExportAPI("ParseBoolean")>
    <Extension>
    Public Function ParseBoolean(ch As Char) As Boolean
        If ch = ASCII.NUL Then
            Return False
        End If

        Select Case ch
            Case "y"c, "Y"c, "t"c, "T"c, "1"c
                Return True
            Case "n"c, "N"c, "f"c, "F"c, "0"c
                Return False
        End Select

        Return True
    End Function

    Public Function ParseTimeSpan(str As String) As TimeSpan
        Dim val As TimeSpan = Nothing

        str = Strings.Trim(str)

        If str = "" Then
            Return Nothing
        End If

        If TimeSpan.TryParse(str, val) Then
            Return val
        End If

        Dim tokens = str.Split(":"c)

        If tokens.Length = 2 Then
            ' hh:mm 
            Dim hh = Integer.Parse(tokens(0))
            Dim mm = Integer.Parse(tokens(1))

            val = TimeSpan.FromHours(hh) + TimeSpan.FromMinutes(mm)
        ElseIf tokens.Length = 3 Then
            ' hh:mm:ss
            Dim hh = Integer.Parse(tokens(0))
            Dim mm = Integer.Parse(tokens(1))
            Dim ss = Integer.Parse(tokens(2))

            val = TimeSpan.FromHours(hh) + TimeSpan.FromMinutes(mm) + TimeSpan.FromSeconds(ss)
        Else
            If str.IsPattern("\d+\s*((min)|(hour)|(second))") Then
                Dim n As Integer = Integer.Parse(str.Match("\d+"))
                Dim unit As String = str.Split.Last

                Select Case unit.ToLower
                    Case "min" : val = TimeSpan.FromMinutes(n)
                    Case "hour", "hours" : val = TimeSpan.FromHours(n)
                    Case "second", "seconds" : val = TimeSpan.FromSeconds(n)
                    Case "day", "days" : val = TimeSpan.FromDays(n)
                    Case Else
                        Throw New NotImplementedException(unit)
                End Select
            Else
                Throw New InvalidExpressionException(str)
            End If
        End If

        Return val
    End Function
End Module
