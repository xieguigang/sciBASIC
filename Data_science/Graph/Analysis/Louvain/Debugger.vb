Imports System.IO
Imports System.Runtime.CompilerServices

Namespace Analysis.Louvain

    Public Module Debugger

        <Extension>
        Public Sub print(g As LouvainCommunity)
            For i = 0 To g.global_n - 1
                Console.WriteLine(i & ": " & g.global_cluster(i))
            Next

            Console.WriteLine("-------")
        End Sub

        <Extension>
        Public Sub WriteJSON(a As LouvainCommunity, json As TextWriter)
            json.Write("{" & vbLf & """nodes"": [" & vbLf)

            For i = 0 To a.global_n - 1
                json.Write("{""id"": """ & i & """, ""group"": " & a.global_cluster(i) & "}")

                If i + 1 <> a.global_n Then
                    json.Write(",")
                End If

                json.Write(vbLf)
            Next

            json.Write("]," & vbLf & """links"": [" & vbLf)

            For i = 0 To a.global_n - 1
                Dim j = a.global_head(i)

                While j <> -1
                    Dim k = a.global_edge(j).v
                    json.Write("{""source"": """ & i & """, ""target"": """ & k & """, ""value"": 1}")

                    If i + 1 <> a.global_n OrElse a.global_edge(j).next <> -1 Then
                        json.Write(",")
                    End If

                    json.Write(vbLf)
                    j = a.global_edge(j).next
                End While
            Next

            json.Write("]" & vbLf & "}" & vbLf)
            json.Flush()
        End Sub

        <Extension>
        Public Sub WriteLinks(a As LouvainCommunity, file As TextWriter)
            Dim list = New List(Of Integer)(a.global_n - 1) {}

            For i = 0 To a.global_n - 1
                list(i) = New List(Of Integer)()
            Next

            For i = 0 To a.global_n - 1
                list(a.global_cluster(i)).Add(i)
            Next

            For i = 0 To a.global_n - 1
                If list(i).Count = 0 Then
                    Continue For
                End If

                For j = 0 To list(i).Count - 1
                    file.Write(list(i)(j).ToString() & " ")
                Next

                file.Write(vbLf)
            Next

            file.Flush()
        End Sub
    End Module
End Namespace