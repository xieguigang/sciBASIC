#Region "Microsoft.VisualBasic::89920f5602d1758a6f6897401e67be3e, Data_science\Mathematica\Math\Math\Scripting\Arithmetic.Expression\FuncParser.vb"

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
        ''' 这个为自定义函数表达式的解析
        ''' 
        ''' > &lt;Function>(args) expression
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 例如可以解析一个用户输入的自定义的Lambda表达式:
        ''' 
        ''' ```vbnet
        ''' ' func(args) expression
        ''' add(x, y, z) x+y+z
        ''' ```
        ''' 
        ''' 也可以用来解析一个函数的调用表达式
        ''' 
        ''' ```vbnet
        ''' add(1, 2, z:=3)
        ''' ```
        ''' 
        ''' 因为这个函数是直接进行字符串分隔符切割的, 所以没有办法解析复杂表达式的参数
        ''' </remarks>
        Public Function TryParse(s As String) As Func
            Dim define As String = Mid(s, 1, InStr(s, ")"))
            Dim expr As String = Mid(s, define.Length + 1)
            Return define.__defineParser(expr)
        End Function

        <Extension> Private Function __defineParser(define As String, expr As String) As Func
            Dim name As String = Mid(define, 1, InStr(define, "(") - 1)
            Dim args As String = Mid(define, name.Length + 1).GetStackValue("(", ")")

            ' 因为这个函数是用来解析自定义函数申明的
            ' 因为函数申明之中的参数只需要填写变量名称即可
            ' 结构非常简单
            ' 所以在这里直接使用分隔符进行切割即可满足要求
            '
            ' 如果是函数调用表达式的解析的话,因为函数的参数可能是一个表达式,会比较复杂
            ' 所以这个函数解析对于函数调用表达式的解析可能不适用
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
