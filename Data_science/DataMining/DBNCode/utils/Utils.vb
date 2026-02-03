Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace utils

    Public Class Utils

        Public Shared Function readFile(pathName As String) As IList(Of String)
            Return pathName.ReadAllLines().AsList()
        End Function

        Public Shared Sub writeToFile(fileName As String, contents As String)
            Call contents.SaveTo(fileName)
        End Sub

        Public Shared Function isNumeric(str As String) As Boolean
            ' TODO: change to non-exception method
            Try
                Double.Parse(str)
            Catch __unusedFormatException1__ As FormatException
                Return False
            End Try
            Return True
        End Function

        Private Shared Sub dfs(graph As IList(Of IList(Of Integer)), used As Boolean(), res As IList(Of Integer), u As Integer)
            used(u) = True
            For Each v In graph(u)
                If Not used(v) Then
                    Utils.dfs(graph, used, res, v)
                End If
            Next
            res.Add(u)
        End Sub

        ' adapted from
        ' https://sites.google.com/site/indy256/algo/topological_sorting
        Public Shared Function topologicalSort(graph As IList(Of IList(Of Integer))) As IList(Of Integer)
            Dim n = graph.Count
            Dim used = New Boolean(n - 1) {}
            Dim res As IList(Of Integer) = New List(Of Integer)()
            For i = 0 To n - 1
                If Not used(i) Then
                    Utils.dfs(graph, used, res, i)
                End If
            Next
            res.Reverse()
            Return res
        End Function

        Public Shared Function adjacencyMatrix(edges As IList(Of Edge), n As Integer) As Boolean()()

            Dim adj = RectangularArray.Matrix(Of Boolean)(n, n)

            For Each e As Edge In edges
                adj(e.Head) = adj(e.Tail)
            Next

            Return adj

        End Function


    End Class

End Namespace
