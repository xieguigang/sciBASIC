﻿Namespace ComponentModel.Algorithm

    Public NotInheritable Class BipartiteMatching
        Private ReadOnly _v As Integer
        Private ReadOnly _g As List(Of Integer)()

        Public Sub New(v As Integer)
            _v = v
            _g = Enumerable.Repeat(0, v).[Select](Function(__) New List(Of Integer)()).ToArray()
        End Sub

        Public Sub AddEdge(u As Integer, v As Integer)
            _g(u).Add(v)
            _g(v).Add(u)
        End Sub

        Public Function Match() As Integer
            Dim res As Integer = 0
            Dim matches = Enumerable.Repeat(-1, _v).ToArray()
            For v As Integer = 0 To _v - 1
                If matches(v) < 0 Then
                    Dim used = New Boolean(_v - 1) {}
                    If Dfs(v, used, matches) Then
                        res += 1
                    End If
                End If
            Next
            Return res
        End Function

        Private Function Dfs(v As Integer, used As Boolean(), matches As Integer()) As Boolean
            used(v) = True
            For i As Integer = 0 To _g(v).Count - 1
                Dim u = _g(v)(i), w = matches(u)
                If w < 0 OrElse Not used(w) AndAlso Dfs(w, used, matches) Then
                    matches(v) = u
                    matches(u) = v
                    Return True
                End If
            Next
            Return False
        End Function
    End Class
End Namespace