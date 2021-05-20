#Region "Microsoft.VisualBasic::7aa365d9fe12cfeb3d5af60dae796238, Microsoft.VisualBasic.Core\src\Scripting\Expressions\StringInterpolation.vb"

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

    '     Module StringInterpolation
    ' 
    '         Function: GetValue, (+2 Overloads) Interpolate
    ' 
    '         Sub: Interpolate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports VBCodePatterns = Microsoft.VisualBasic.Scripting.SymbolBuilder.VBLanguage.Patterns

Namespace Scripting.Expressions

    ''' <summary>
    ''' 简单的字符串插值引擎，可以用来调试字符串表达式的处理结果
    ''' </summary>
    Public Module StringInterpolation

        ' "abcdefg$h$i is $k \$a"

        Const VB_str$ = "&VB_str"

        ''' <summary>
        ''' 允许下换线，点号，ASCII字母，以及数字作为标识符
        ''' 
        ''' ```
        ''' $var
        ''' $obj.property
        ''' $obj.property.value.method.etc
        ''' ```
        ''' </summary>
        Const VariablePattern$ = "[$]" & VBCodePatterns.Identifer & "(\." & VBCodePatterns.Identifer & ")*"

        ''' <summary>
        ''' 不存在的键名会自动返回空字符串
        ''' </summary>
        ''' <param name="resource"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetValue(resource As Dictionary(Of String, String)) As Func(Of String, String)
            Return Function(name$)
                       If resource.ContainsKey(name) Then
                           Return resource(name$)
                       Else
                           Return Nothing
                       End If
                   End Function
        End Function

        ''' <summary>
        ''' 对于<paramref name="getValue"/>方法而言，是不需要``$``前缀了的
        ''' </summary>
        ''' <param name="expr$">
        ''' 只有当变量的值不为空值的时候才会进行替换，但是当<paramref name="nullAsEmpty"/>为真的时候会被强行替换为空字符串进行替换
        ''' </param>
        ''' <param name="getValue">Get string value of the variable in the expression.</param>
        ''' <param name="escape">
        ''' 是否需要进行对\t\n这类字符的转义操作？假若是路径字符串，则不推荐开启这个选项了，因为路径的分隔符很容易引起误转义...
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function Interpolate(expr$, getValue As Func(Of String, String),
                                    Optional nullAsEmpty As Boolean = False,
                                    Optional escape As Boolean = True) As String

            With New StringBuilder(expr)
                Call .Interpolate(getValue, nullAsEmpty, escape)
                Return .ToString
            End With
        End Function

        ''' <summary>
        ''' 在解析出variable之后，variable前面的``$``是会被清除掉的，所以variable的source <paramref name="getValue"/>里面的变量名称应该是没有``$``前缀的
        ''' </summary>
        ''' <param name="sb"></param>
        ''' <param name="getValue"></param>
        ''' <param name="nullAsEmpty">只有当变量的值不为空值的时候才会进行替换，但是当<paramref name="nullAsEmpty"/>为真的时候会被强行替换为空字符串进行替换</param>
        ''' <param name="escape"></param>
        <Extension>
        Public Sub Interpolate(ByRef sb As StringBuilder, getValue As Func(Of String, String),
                               Optional nullAsEmpty As Boolean = False,
                               Optional escape As Boolean = True)

            Call sb.Replace("\$", VB_str)

            For Each v$ In Regex _
                .Matches(sb.ToString, VariablePattern, RegexICSng) _
                .ToArray _
                .OrderByDescending(Function(s) s.Length) _
                .ToArray

                Dim value$ = getValue(Mid(v, 2))

                If value Is Nothing AndAlso nullAsEmpty Then
                    value = ""
                End If

                ' 只对非空值进行替换
                If Not value Is Nothing Then
                    Call sb.Replace(v, value)
                End If
            Next

            With sb
                ' 这个必须要转义
                .Replace(VB_str, "$")

                ' 根据需要来进行转义，对于Windows文件路径而言，不推荐转义
                ' 因为Windows的文件路径分隔符为\，很容易引起误解，例如C:\tsv会被误转义为C:<TAB>sv而导致错误
                If escape Then
                    Call .Replace("\n", vbLf)
                    Call .Replace("\t", vbTab)
                End If
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Interpolate(expr$, table As Dictionary(Of String, String), Optional nullAsEmpty As Boolean = False) As String
            Return expr.Interpolate(table.GetValue, nullAsEmpty)
        End Function
    End Module
End Namespace
