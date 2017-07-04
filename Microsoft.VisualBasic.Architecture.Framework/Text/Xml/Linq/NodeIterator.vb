Imports System.Runtime.CompilerServices
Imports System.Xml
Imports Microsoft.VisualBasic.Language

Namespace Text.Xml.Linq

    Public Module NodeIterator

        ''' <summary>
        ''' 使用<see cref="XmlDocument.Load"/>方法加载XML文档依旧是一次性的全部加载所有的文本到内存之中，第一次加载效率会比较低
        ''' 则可以使用这个方法来加载非常大的XML文档
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <returns></returns>
        <Extension> Public Iterator Function IterateArrayNodes(path$, tag$) As IEnumerable(Of String)
            Dim buffer As New List(Of String)
            Dim start$ = "<" & tag
            Dim ends$ = $"</{tag}>"
            Dim stack%
            Dim tagOpen As Boolean = False
            Dim lefts$
            Dim i%

            For Each line As String In path.IterateAllLines
                If tagOpen Then

                    i = InStr(line, ends)

                    If i > 0 Then
                        ' 遇到了结束标签，则取出来
                        If stack > 0 Then
                            stack -= 1 ' 内部栈，还没有结束，则忽略当前的这个标签
                        Else
                            ' 这个是真正的结束标签
                            lefts = Mid(line, i + ends.Length)
                            buffer += ends
                            tagOpen = False

                            Yield buffer.JoinBy("")

                            buffer *= 0
                            buffer += lefts

                            ' 这里要跳出来，否则后面buffer += line处任然会添加这个结束标签行的
                            Continue For
                        End If
                    ElseIf InStr(line, start) > 0 Then
                        stack += 1
                    End If

                    buffer += line
                Else
                    ' 需要一直遍历到开始标签为止
                    i = InStr(line, start)

                    If i > 0 Then
                        tagOpen = True
                        buffer += Mid(line, i)
                    End If
                End If
            Next
        End Function
    End Module
End Namespace