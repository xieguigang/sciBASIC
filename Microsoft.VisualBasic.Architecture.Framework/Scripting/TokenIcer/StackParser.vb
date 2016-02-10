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
                .Caller = New List(Of Token(Of Tokens)) From {pretendDef},
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
                .Caller = New List(Of Token(Of Tokens))
            }

            Do While Not source.IsNullOrEmpty
                Dim x As Token(Of Tokens) = source.Dequeue

                If Not x.TokenName.Equals(stackT.ParamDeli) Then
                    Call current.Caller.Add(x)
                End If

                If source.Count = 0 Then
                    Call list.Add(current)
                    Exit Do
                End If

                Dim peek As Token(Of Tokens) = source.Peek

                If peek.TokenName.Equals(stackT.LPair) Then  ' 向下一层堆栈
                    Call source.Dequeue()
                    current.Args = __parsing(source, stackT)
                ElseIf peek.TokenName.Equals(stackT.RPair) Then  ' 向上一层退栈
                    Call source.Dequeue()
                    Call list.Add(current)
                    Exit Do
                ElseIf x.TokenName.Equals(stackT.ParamDeli) Then
                    Call list.Add(current)

                    current = New Func(Of Tokens) With {
                        .Caller = New List(Of Token(Of Tokens))
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


