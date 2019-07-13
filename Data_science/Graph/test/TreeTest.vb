#Region "Microsoft.VisualBasic::763644d6b47d19b79abfdddadb9041d2, Data_science\Graph\test\TreeTest.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module TreeTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

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
