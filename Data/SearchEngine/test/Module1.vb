Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Data.IO.SearchEngine.Index

Module Module1

    Sub Main()
        Dim tree As New AVLTree(Of String, Double)(Function(a, b) a.CompareTo(b))
        Dim rnd As New Random

        For i As Integer = 0 To 20
            Call tree.Add(RandomASCIIString(10, skipSymbols:=True), rnd.NextDouble)
            Call Thread.Sleep(100)
        Next

        Dim repo = tree.root.ToIndex
        Dim tree2 = repo.BinaryTree

        Call repo.GetXml.SaveTo("./test_index.xml")

        Pause()
    End Sub
End Module
