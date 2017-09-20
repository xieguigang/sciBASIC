Imports Microsoft.VisualBasic.Data.Graph

Module TreeTest

    Sub Main()
        Dim tree As BinaryTree(Of String) = BinaryTree(Of String).ROOT
        Dim rand As New Random

        For i As Integer = 10 To 100
            tree.Insert(i, rand.Next(10, 10000000))
        Next

        Dim g = tree.CreateGraph

        Pause()
    End Sub
End Module
