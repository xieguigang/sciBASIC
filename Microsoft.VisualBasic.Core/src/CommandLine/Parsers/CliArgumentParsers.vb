#Region "Microsoft.VisualBasic::2269d46cdac80badc3c8dcb3d3122368, Microsoft.VisualBasic.Core\src\CommandLine\Parsers\CliArgumentParsers.vb"

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

    '   Total Lines: 150
    '    Code Lines: 94
    ' Comment Lines: 29
    '   Blank Lines: 27
    '     File Size: 5.82 KB


    '     Module CliArgumentParsers
    ' 
    '         Function: CreateParameterValues, GetLogicalFlags, IsPossibleLogicFlag
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace CommandLine.Parsers

    Public Module CliArgumentParsers

        ''' <summary>
        ''' Parsing parameters from a specific tokens.
        ''' (从给定的词组之中解析出参数的结构)
        ''' </summary>
        ''' <param name="tokens">个数为偶数的，但是假若含有开关的时候，则可能为奇数了</param>
        ''' <param name="includeLogicals">返回来的列表之中是否包含有逻辑开关</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension>
        Public Function CreateParameterValues(tokens$(), includeLogicals As Boolean, Optional note$ = Nothing) As List(Of NamedValue(Of String))
            Dim list As New List(Of NamedValue(Of String))
            Dim key As String

            If tokens.IsNullOrEmpty Then
                Return list
            ElseIf tokens.Length = 1 Then
                If IsPossibleLogicFlag(tokens(Scan0)) AndAlso includeLogicals Then
                    list += New NamedValue(Of String) With {
                        .Name = tokens(Scan0),
                        .Value = CStr(True),
                        .Description = note
                    }
                Else
                    Return list
                End If
            End If

            ' 下面都是多余或者等于两个元素的情况
            ' 数目多于一个的
            For i As Integer = 0 To tokens.Length - 1
                Dim [next] As Integer = i + 1

                If [next] = tokens.Length Then
                    ' 这个元素是开关，已经到达最后则没有了，跳出循环
                    If IsPossibleLogicFlag(tokens(i)) AndAlso includeLogicals Then
                        list += New NamedValue(Of String)(tokens(i), True, note)
                    End If

                    Exit For
                End If

                Dim s As String = tokens([next])

                ' 当前的这个元素是开关，下一个也是开关开头，则本元素肯定是一个开关
                If tokens(i).ToLower <> "/@set" AndAlso IsPossibleLogicFlag(s) Then
                    If includeLogicals Then
                        list += New NamedValue(Of String)(tokens(i), True, note)
                    End If

                    Continue For
                Else
                    ' 下一个元素不是开关，则当前元素为一个参数名，则跳过下一个元素
                    key = tokens(i).ToLower
                    list += New NamedValue(Of String)(key, s, note)

                    i += 1
                End If
            Next

            Return list
        End Function

        ''' <summary>
        ''' Get all of the logical parameters from the input tokens.
        ''' (这个函数所生成的逻辑参数的名称全部都是小写形式的)
        ''' </summary>
        ''' <param name="args">要求第一个对象不能够是命令的名称</param>
        ''' <returns></returns>
        <Extension>
        Public Function GetLogicalFlags(args As IEnumerable(Of String), ByRef singleValue$) As String()
            Dim tokens$() = args.SafeQuery.ToArray

            If tokens.IsNullOrEmpty Then
                Return New String() {}
            ElseIf tokens.Length = 1 Then
                ' 只有一个元素，则肯定为开关
                Return {tokens(0).ToLower}
            End If

            Dim tkList As New List(Of String)

            For i As Integer = 0 To tokens.Length - 1
                ' 数目多于一个的
                Dim next% = i + 1

                If [next] = tokens.Length Then
                    If IsPossibleLogicFlag(obj:=tokens(i)) Then
                        tkList += tokens(i)  '
                    End If

                    Exit For
                End If

                Dim s As String = tokens([next])

                If IsPossibleLogicFlag(obj:=s) Then
                    ' 当前的这个元素是开关，下一个也是开关开头，则本元素肯定是一个开关
                    If IsPossibleLogicFlag(obj:=tokens(i)) Then
                        tkList += tokens(i)
                    Else

                        If i = 0 Then
                            singleValue = tokens(i)
                        End If

                    End If
                Else
                    ' 下一个元素不是开关，则当前元素为一个参数名，则跳过下一个元素
                    i += 1
                End If

            Next

            Return (From s As String In tkList Select s.ToLower).ToArray
        End Function

        ''' <summary>
        ''' Is this string tokens is a possible <see cref="Boolean"/> value flag
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <ExportAPI("IsPossibleBoolFlag?")>
        Public Function IsPossibleLogicFlag(obj As String) As Boolean
            If String.IsNullOrEmpty(obj) Then
                Return False
            ElseIf obj.Contains(" ") Then
                Return False
            ElseIf IsNumeric(obj) Then
                Return False
            End If

            ' Linux上面全路径总是从/，即根目录开始的
            If obj.Count("/"c) > 1 Then
                Return False
            End If

            Return obj.StartsWith("-") OrElse obj.StartsWith("/")
        End Function
    End Module
End Namespace
