#Region "Microsoft.VisualBasic::82e59f33b460ad6cb3377d88c6c2e98c, ..\sciBASIC#\Data_science\Mathematical\Math\Scripting\StringInterpolation.vb"

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
Imports System.Text
Imports System.Text.RegularExpressions

Namespace Scripting.Expressions

    ''' <summary>
    ''' 简单的字符串插值引擎，可以用来调试字符串表达式的处理结果
    ''' </summary>
    Public Module StringInterpolation

        ' "abcdefg$h$i is $k \$a"

        Const VB_str$ = "&VB_str"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="expr$"></param>
        ''' <param name="getValue">Get string value of the variable in the expression.</param>
        ''' <returns></returns>
        <Extension>
        Public Function Interpolate(expr$, getValue As Func(Of String, String)) As String
            Dim sb As New StringBuilder(expr.Replace("\$", VB_str))
            Dim t = Regex.Matches(sb.ToString, "[$][a-z][a-z0-9]*", RegexICSng).ToArray

            For Each v$ In t
                Dim value$ = getValue(Mid(v, 2))

                If Not value Is Nothing Then
                    Call sb.Replace(v, value)
                End If
            Next

            With sb
                .Replace(VB_str, "$")
                .Replace("\t", vbTab)
                .Replace("\n", vbLf)

                expr = .ToString
            End With

            Return expr
        End Function
    End Module
End Namespace
