#Region "Microsoft.VisualBasic::1dc8f2ae695690c348365c02d785831f, Microsoft.VisualBasic.Core\src\CommandLine\InteropService\SharedORM\API.vb"

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

    '   Total Lines: 196
    '    Code Lines: 131
    ' Comment Lines: 39
    '   Blank Lines: 26
    '     File Size: 8.07 KB


    '     Module API
    ' 
    '         Function: BuildArguments, CommandLineModel, Tokenize
    ' 
    '         Sub: Tokenize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Parsers
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace CommandLine.InteropService.SharedORM

    ''' <summary>
    ''' CommandLine source code mapper API
    ''' </summary>
    Public Module API

        ' Usage:= /command1 /param1 <value1, means a string value?> /param2 <value2> [/boolean1 /boolean2 /opt <blabla, default=value233>]

        ''' <summary>
        ''' 从<see cref="ExportAPIAttribute.Usage"/>之中解析出命令行的模型
        ''' </summary>
        ''' <param name="usage$">
        ''' + 由于习惯于在命令行的usage说明之中使用``&lt;>``括号对来包裹参数值，所以usage字符串数据还不能直接使用普通的命令行函数进行解析
        ''' + ``[]``方括号所包裹的参数对象都是可选参数
        ''' + 对于可选参数的默认值，默认值的解析形式为``default=...``，如果没有这个表达式，则默认为Nothing空值为默认值
        ''' </param>
        ''' <returns></returns>
        <Extension> Public Function CommandLineModel(usage As String) As CommandLine
            Dim name$ = Nothing
            Dim arguments$() = Nothing
            Dim optionals$() = Nothing

            usage = usage.Trim
            usage.Tokenize(name, arguments, optionals)

            ' 逻辑符号只存在于optional之中
            Dim booleans$() = Nothing
            Dim params As NamedValue(Of String)()

            Try
                With arguments.BuildArguments(optionals, booleans)
                    If .ByRef Like GetType(NamedValue(Of String)) Then
                        params = {
                            .TryCast(Of NamedValue(Of String))
                        }
                    Else
                        params = .TryCast(Of NamedValue(Of String)())
                    End If
                End With
            Catch ex As Exception
                Dim msg$ = $"Invalid commandline usage({usage})!" & vbCrLf & vbCrLf
                Dim details = New Dictionary(Of String, String()) From {
                    {NameOf(arguments), arguments},
                    {NameOf(optionals), optionals},
                    {NameOf(booleans), booleans}
                }.GetJson

                msg = msg & details

                Throw New ArgumentException(msg, ex)
            End Try

            Dim model As New CommandLine With {
                .Name = name,
                .arguments = params.AsList,
                .BoolFlags = booleans _
                    .SafeQuery _
                    .Select(AddressOf LCase) _
                    .ToArray,
                .cliCommandArgvs = usage
            }

            Return model
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="args$">因为逻辑变量只存在于可选参数之中，所以在这里必须参数直接split2即可</param>
        ''' <param name="optionals$"></param>
        ''' <param name="booleans$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function BuildArguments(args$(), optionals$(), ByRef booleans$()) As [Variant](Of NamedValue(Of String)(), NamedValue(Of String))
            If args.Length = 1 Then
                ' 类似于 /command term 这样子的情况
                Return New NamedValue(Of String) With {
                    .Name = Nothing,
                    .Value = args(Scan0)
                }
            End If

            Dim out As List(Of NamedValue(Of String)) = args _
                .Split(2) _
                .Select(Function(a)
                            Return New NamedValue(Of String) With {
                                .Name = a(0),
                                .Value = a(1)
                            }
                        End Function) _
                .AsList

            booleans = GetLogicalFlags(optionals, Nothing)
            out += optionals _
                .CreateParameterValues(False, note:=NameOf(optionals)) _
                .ToArray

            Return out.ToArray
        End Function

        <Extension>
        Public Sub Tokenize(usage$, ByRef name$, ByRef arguments$(), ByRef optionals$())
            Dim opts$ = Regex.Match(usage, "\[.+\]").Value

            If opts.Length > 0 Then
                usage = usage.Replace(opts, "")
                opts = opts.GetStackValue("[", "]")
            End If

            ' 命令的名称肯定是没有空格的，所以在里直接split取第一个元素
            name = usage.Split.First
            usage = Mid(usage, name.Length + 1).Trim

            ' 在下面将arguments和optionals参数进行分词就行了
            ' 因为在usage之中并不会使用双引号来分割value值，而是使用尖括号
            ' 所以在这里需要额外注意下
            arguments = usage.Tokenize
            optionals = opts.Tokenize
        End Sub

        ''' <summary>
        ''' 使用空格分隔，但是需要对value额外注意
        ''' 
        ''' + 在这里面分隔符为空格
        ''' + 可以使用双引号包裹值，则双引号之中的字符串在可选参数之中都被看作为可选参数值
        ''' + 可以使用尖括号包裹值，则尖括号之中的default=表达式则是可选参数之中的默认参数值
        ''' </summary>
        ''' <param name="s$"></param>
        ''' <returns></returns>
        <Extension> Public Function Tokenize(s As String) As String()
            Dim t As New Pointer(Of Char)(s)
            Dim c As Char
            Dim valueEscape As Boolean = False
            Dim tmp As New List(Of Char)
            Dim out As New List(Of String)
            Dim escapeType As Char ' 转义的起始只有双引号或者左边的尖括号
            Dim isValueEnd =
                Function()
                    Dim last As Char = tmp.LastOrDefault
                    Return (escapeType = "<"c AndAlso last = ">"c) OrElse
                           (escapeType = ASCII.Quot AndAlso last = ASCII.Quot)
                End Function

            Do While Not t.EndRead
                c = ++t

                If c = " "c OrElse c = "="c Then
                    If Not valueEscape Then
                        out += New String(tmp)
                        tmp *= 0
                    Else
                        ' 检查上一个字符是不是value的结束符号: >
                        If isValueEnd() Then

                            ' 则这个空格表示value的结束
                            out += New String(tmp)
                            valueEscape = False
                            tmp *= 0
                            escapeType = " "c ' 重置转义类型

                        Else
                            ' 这个空格是value值之中的一部分，则添加到临时列表
                            tmp += c
                        End If
                    End If
                Else
                    ' 检查是否是value的开始符号: <或者双引号
                    If (c = "<"c OrElse c = ASCII.Quot) AndAlso tmp = 0 Then
                        valueEscape = True
                        escapeType = c
                    End If

                    tmp += c
                End If
            Loop

            If tmp.Count > 0 Then
                Return out + New String(tmp)
            Else
                Return out
            End If
        End Function
    End Module
End Namespace
