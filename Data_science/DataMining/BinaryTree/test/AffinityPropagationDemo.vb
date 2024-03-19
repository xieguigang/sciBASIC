Imports Microsoft.VisualBasic.DataMining.BinaryTree.AffinityPropagation
Imports Microsoft.VisualBasic.Serialization.JSON

Module AffinityPropagationDemo

    Sub Main2()
        Dim matrix_raw As IEnumerable(Of Double()) = demo()
        Dim method As New AffinityPropagation(matrix_raw)
        Dim clusters = method.Fit

        Call Console.WriteLine(clusters.GetJson)

        Pause()
    End Sub

    Private Iterator Function demo() As IEnumerable(Of Double())
        ' [0,0,0,3,3,3]

        Yield {1, 2}
        Yield {1, 4}
        Yield {1, 0}
        Yield {4, 2}
        Yield {4, 4}
        Yield {4, 0}
    End Function
End Module
