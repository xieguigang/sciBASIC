#Region "Microsoft.VisualBasic::f691ecc78975d9bf9136da64bd9f8358, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Math\ScientificNotation.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions

Namespace Mathematical

    ''' <summary>
    ''' 科学记数法
    ''' </summary>
    Public Module ScientificNotation

        ''' <summary>
        ''' 返回零表示比较小的常数
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="INF">当位数超过这个值之后将会被判定为非常大或者非常小的一个数</param>
        ''' <returns></returns>
        Public Function PowerLog10(x#, Optional INF% = 5) As Single
            Dim pow# = Math.Log10(Math.Abs(x))

            If pow < -INF Then
                Return pow
            End If
            If pow > INF Then
                Return pow
            End If

            Return 0
        End Function

        ''' <summary>
        ''' 是否需要科学记数法来格式化？
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <param name="INF%"></param>
        ''' <returns></returns>
        Public Function UsingScientificNotation(x#, Optional INF% = 5) As Boolean
            Return ScientificNotation.PowerLog10(x, INF) <> 0!
        End Function

        ''' <summary>
        ''' 分别处理正常的小数或者科学记数法的小数
        ''' </summary>
        ''' <param name="n#"></param>
        ''' <param name="decimal%"></param>
        ''' <returns></returns>
        <Extension> Public Function FormatNumeric(n#, decimal%) As String
            Dim s$ = n.ToString

            If InStr(s, "E", CompareMethod.Text) > 0 Then
                ' 科学记数法
                Dim t$() = s.Split("e"c, "E"c)
                t(0) = Math.Round(Val(t(0)), [decimal])
                s = t(0) & "E" & t(1)

                Return s
            End If

            If UsingScientificNotation(n) Then
                s = FormatScientificNotation(n, [decimal])
            Else
                s = Math.Round(n, [decimal]).ToString
            End If

            Return s
        End Function

        ''' <summary>
        ''' 强制格式化为科学记数法
        ''' </summary>
        ''' <param name="decimal%"></param>
        ''' <returns></returns>
        Public Function FormatScientificNotation(n#, decimal%) As String
            If n# = 0R Then
                Return "0"
            End If

            Dim power = Fix(Math.Log10(n))
            Dim s = n.ToString.Split("E"c, "e"c).First
            Dim t$() = s.Split("."c)

            ' 处理整数部分
            Dim int As String = t(Scan0)
            Dim intpower = int.Length - 1

            If intpower > 0 Then  ' 整数部分不止一个字符长度，即数位大于等于2
                int = int.First & "." & Mid(int, 1) & t(1)
                s = Math.Round(Val(int), [decimal])
            Else
                s = int & "." & t(1)
                s = Math.Round(Val(s), [decimal])
            End If

            s = s & $"E{power + intpower}"

            Return s
        End Function
    End Module
End Namespace
