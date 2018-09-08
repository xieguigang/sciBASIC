#Region "Microsoft.VisualBasic::8d66f423ef870a6376347a9cf74276f8, Data_science\Mathematica\Math\Math\Scripting\Arithmetic.Expression\FuncParser.vb"

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

    '     Module FuncParser
    ' 
    '         Function: __defineParser, TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Scripting

    ''' <summary>
    ''' Parser for the user define <see cref="Func"/>.(用户自定义函数的解析器)
    ''' </summary>
    Public Module FuncParser

        ''' <summary>
        ''' &lt;Function>(args) expression
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Function TryParse(s As String) As Func
            Dim define As String = Mid(s, 1, InStr(s, ")"))
            Dim expr As String = Mid(s, define.Length + 1)
            Return define.__defineParser(expr)
        End Function

        <Extension> Private Function __defineParser(define As String, expr As String) As Func
            Dim name As String = Mid(define, 1, InStr(define, "(") - 1)
            Dim args As String = Mid(define, name.Length + 1).GetStackValue("(", ")")

            Return New Func With {
                .Args = args _
                    .Split(","c) _
                    .Select(Function(s) s.Trim) _
                    .ToArray,
                .Expression = expr,
                .Name = name
            }
        End Function
    End Module
End Namespace
