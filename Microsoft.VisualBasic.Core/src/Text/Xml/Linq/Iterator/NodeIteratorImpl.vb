Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Text.Xml.Linq

    Friend Class NodeIteratorImpl

        ReadOnly start1$
        ReadOnly start3$
        ReadOnly start2$
        ReadOnly ends$

        Dim stack%
        Dim tagOpen As Boolean = False
        Dim lefts$
        Dim i%
        Dim buffer As New List(Of String)

        Sub New(tag As String)
            start1 = "<" & tag & "/"
            start2 = "<" & tag & " "
            start3 = "<" & tag & ">"
            ends = $"</{tag}>"
        End Sub

        Public Iterator Function PopulateData(lines As IEnumerable(Of String), name$, filter As Func(Of String, Boolean)) As IEnumerable(Of String)
            Dim break As Boolean = False
            Dim out As String = Nothing

            For Each line As String In lines
                If tagOpen Then
                    break = False
                    out = Nothing

                    Call processTagOpen(line, filter, break, out)

                    If Not out Is Nothing Then
                        Yield out
                    End If

                    If break Then
                        Continue For
                    End If
                Else
                    ' 需要一直遍历到开始标签为止
                    If checkStart(line, i) Then
                        tagOpen = True
                        buffer += Mid(line, i)
                    End If
                End If
            Next

            If buffer > 0 AndAlso Not buffer.All(Function(s) Strings.Trim(s).StringEmpty) Then
                Call $"[{name}] is an incomplete xml dataset!".Warning
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function PopulateData(path As String, filter As Func(Of String, Boolean)) As IEnumerable(Of String)
            Return PopulateData(path.IterateAllLines, path, filter)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function PopulateData(file As StreamReader, filter As Func(Of String, Boolean)) As IEnumerable(Of String)
            Return Iterator Function() As IEnumerable(Of String)
                       Do While Not file.EndOfStream
                           Yield file.ReadLine
                       Loop
                   End Function() _
 _
                .DoCall(Function(lines)
                            Return PopulateData(lines, file.ToString, filter)
                        End Function)
        End Function

        Private Sub processTagOpen(line As String, filter As Func(Of String, Boolean), ByRef break As Boolean, ByRef out As String)
            i = InStr(line, ends)

            If i > 0 Then
                ' 遇到了结束标签，则取出来
                If stack > 0 Then
                    ' 内部栈，还没有结束，则忽略当前的这个标签
                    stack -= 1
                Else
                    Dim xmlText$ = getXmlSection(line)

                    ' 这里要跳出来，否则后面buffer += line处任然会添加这个结束标签行的
                    break = True

                    If Not filter Is Nothing AndAlso filter(xmlText) Then
                        ' skip
                    Else
                        ' populate data
                        out = xmlText
                    End If

                    Return
                End If
            ElseIf checkStart(line, Nothing) Then
                stack += 1
            End If

            buffer += line
        End Sub

        Private Function getXmlSection(line As String) As String
            Dim xmlText$

            ' 这个是真正的结束标签
            lefts = Mid(line, i + ends.Length)
            buffer += ends
            tagOpen = False

            xmlText = buffer.JoinBy(vbLf)
            buffer *= 0
            buffer += lefts

            Return xmlText
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function checkStart(line As String, ByRef i As Integer) As Boolean
            i = InStr(line, start1)

            If i > 0 Then
                Return True
            End If

            i = InStr(line, start2)

            If i > 0 Then
                Return True
            End If

            i = InStr(line, start3)

            If i > 0 Then
                Return True
            Else
                Return False
            End If
        End Function
    End Class
End Namespace