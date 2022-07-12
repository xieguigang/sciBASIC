Imports Microsoft.VisualBasic.Data.GraphTheory

Public Class ClusterTree : Inherits Tree(Of String)

    Public Property Members As New List(Of String)

    ''' <summary>
    ''' build tree
    ''' </summary>
    ''' <param name="tree"></param>
    ''' <param name="target"></param>
    ''' <param name="alignment"></param>
    ''' <param name="threshold"></param>
    Public Overloads Shared Sub Add(tree As ClusterTree, target As String, alignment As ComparisonProvider, threshold As Double)
        If tree.Data.StringEmpty Then
            tree.Data = target
            tree.Childs = New Dictionary(Of String, Tree(Of String))
            tree.Members = New List(Of String)
        Else
            Dim score As Double = alignment.GetSimilarity(tree.Data, target)
            Dim key As String = ""

            For v As Double = 0.1 To 1 Step 0.1
                If score < v Then
                    key = $"<{v.ToString("F1")}"
                    Exit For
                ElseIf v >= threshold Then
                    key = ""
                    Exit For
                End If
            Next

            If key = "" Then
                ' is cluster member
                tree.Members.Add(target)
            ElseIf tree.Childs.ContainsKey(key) Then
                Call Add(tree(key), target, alignment, threshold)
            Else
                Call tree.Add(key)
                Call Add(tree(key), target, alignment, threshold)
            End If
        End If
    End Sub

    Private Overloads Sub Add(label As String)
        Call Add(New ClusterTree With {.label = label})
    End Sub

End Class
