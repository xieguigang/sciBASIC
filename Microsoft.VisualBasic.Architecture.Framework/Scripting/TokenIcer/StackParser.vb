Imports System.Runtime.CompilerServices

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
        <Extension> Public Function Parsing(Of Tokens)(source As IEnumerable(Of Token(Of Tokens)), stackT As StackTokens(Of Tokens)) As Func(Of Tokens)
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
        Private Function __parsing(Of Tokens)(source As Queue(Of Token(Of Tokens)), stackT As StackTokens(Of Tokens)) As Func(Of Tokens)()
            Dim list As New List(Of Func(Of Tokens))
            Dim current As Func(Of Tokens) = New Func(Of Tokens) With {
                .Caller = New List(Of InnerToken(Of Tokens))
            }

            Do While Not source.IsNullOrEmpty
                Dim x As Token(Of Tokens) = source.Dequeue

                If Not stackT.Equals(x.TokenName, stackT.ParamDeli) Then
                    ' 例如 test3( (3+-5.66)  +  6^4.5,7!)
                    If stackT.Equals(x.TokenName, stackT.LPair) Then ' 连续的两个左括号进行堆栈
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

                If stackT.Equals(peek.TokenName, stackT.LPair) Then  ' 向下一层堆栈
                    Call source.Dequeue()

                    Dim currStack As New Func(Of Tokens)(current.Caller.Last)
                    Call current.Caller.RemoveLast
                    currStack.Args = __parsing(source, stackT)
                    Call current.Caller.Add(New InnerToken(Of Tokens)(stackT.Pretend, currStack))
                ElseIf stackT.Equals(peek.TokenName, stackT.RPair) Then  ' 向上一层退栈
                    Call source.Dequeue()
                    Call list.Add(current)
                    Exit Do
                ElseIf stackT.Equals(x.TokenName, stackT.ParamDeli) Then
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
    End Module
End Namespace


