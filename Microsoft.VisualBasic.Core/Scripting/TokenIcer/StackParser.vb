#Region "Microsoft.VisualBasic::abe9626741aeb1516607af3f6d1a4a8c, Microsoft.VisualBasic.Core\Scripting\TokenIcer\StackParser.vb"

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

    '     Module StackParser
    ' 
    '         Function: __parsing, Parsing
    ' 
    '         Sub: __printStack, PrintStack
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Scripting.TokenIcer

    ''' <summary>
    ''' Stack tree parser
    ''' </summary>
    Public Module StackParser

        ''' <summary>
        ''' 返回顶层的根节点
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="stackT">Pretend the root tokens as a true node</param>
        ''' <returns></returns>
        <Extension> Public Function Parsing(Of Tokens As IComparable)(source As IEnumerable(Of Token(Of Tokens)), stackT As StackTokens(Of Tokens)) As Func(Of Tokens)
            Dim pretendDef As Token(Of Tokens) =
                New Token(Of Tokens)(stackT.Pretend, "Pretend")
            Dim sourceQue As Queue(Of Token(Of Tokens)) =
                New Queue(Of Token(Of Tokens))(source)
            Dim root As Func(Of Tokens) =
                New Func(Of Tokens) With {
                .Caller = New List(Of InnerToken(Of Tokens)) From {New InnerToken(Of Tokens)(pretendDef)},
                .Args = __parsing(source:=sourceQue, stackT:=stackT)
            }
            Return root
        End Function

        ''' <summary>
        ''' 主要是解析当前的栈层之中的使用，逗号分隔的参数列表
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Private Function __parsing(Of Tokens As IComparable)(source As Queue(Of Token(Of Tokens)), stackT As StackTokens(Of Tokens)) As Func(Of Tokens)()
            Dim list As New List(Of Func(Of Tokens))
            Dim current As Func(Of Tokens) = New Func(Of Tokens) With {
                .Caller = New List(Of InnerToken(Of Tokens))
            }

            Do While Not source.IsNullOrEmpty
                Dim x As Token(Of Tokens) = source.Dequeue

                If Not stackT.Equals(x.name, stackT.ParamDeli) Then
                    ' 例如 test3( (3+-5.66)  +  6^4.5,7!)
                    If stackT.Equals(x.name, stackT.LPair) Then ' 连续的两个左括号进行堆栈
                        Dim stack As Func(Of Tokens)() = __parsing(source, stackT)
                        Dim inner As New InnerToken(Of Tokens)(New Token(Of Tokens)(stackT.Pretend, "Pretend"), stack)
                        Call current.Caller.Add(inner)
                    Else
                        Call current.Caller.Add(New InnerToken(Of Tokens)(x))
                    End If
                End If

                If source.Count = 0 Then
                    Call list.Add(current)
                    Exit Do
                End If

                Dim peek As Token(Of Tokens) = source.Peek

                If stackT.Equals(peek.name, stackT.LPair) Then  ' 向下一层堆栈
                    Call source.Dequeue()

                    Dim currStack As New Func(Of Tokens)(current.Caller.Last)
                    Call current.Caller.RemoveLast
                    currStack.Args = __parsing(source, stackT)
                    Call current.Caller.Add(New InnerToken(Of Tokens)(stackT.Pretend, currStack))
                ElseIf stackT.Equals(peek.name, stackT.RPair) Then  ' 向上一层退栈
                    Call source.Dequeue()
                    Call list.Add(current)
                    Exit Do
                ElseIf stackT.Equals(x.name, stackT.ParamDeli) Then
                    Call list.Add(current)

                    current = New Func(Of Tokens) With {
                        .Caller = New List(Of InnerToken(Of Tokens))
                    }
                End If
            Loop

            If list.Count = 0 Then
                Call list.Add(current)
            End If

            Return list.ToArray
        End Function

        <Extension> Public Sub PrintStack(Of Tokens As IComparable)(x As Func(Of Tokens))
            Call x.__DEBUG_ECHO
            Call Console.WriteLine(New String("+", 120))
            Call __printStack(x, Scan0)
        End Sub

        Private Sub __printStack(Of Tokens As IComparable)(obj As Func(Of Tokens), i As Integer)
            Dim indent As String = New String(" ", i * 3)

            For Each x As InnerToken(Of Tokens) In obj.Caller
                Call Console.WriteLine(indent & Scripting.ToString(x.obj))

                If Not x.InnerStack.IsNullOrEmpty Then
                    For Each inner In x.InnerStack
                        Call __printStack(inner, i + 1)
                    Next
                End If
            Next

            If Not obj.Args.IsNullOrEmpty Then
                For Each x As Func(Of Tokens) In obj.Args
                    Call __printStack(x, i + 1)
                    Call Console.WriteLine(indent & "[Delimiter]")
                Next
            End If
        End Sub
    End Module
End Namespace
