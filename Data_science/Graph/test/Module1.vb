#Region "Microsoft.VisualBasic::9b06c4a6eaeff7cb31813c0ae2d0924d, Data_science\Graph\test\Module1.vb"

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

    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Graph

Module Module1

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
