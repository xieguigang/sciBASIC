Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.Debugger
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Module DEMO

    Sub Main()
        Dim network As New List(Of Sample)

        network += populate({1, 1, 1, 1, 1, -10, 99, 1}, 0, 100)
        network += populate({1, 1, 1, 1, 0, -10, 99, 1}, 100, 100)
        network += populate({0, 0, 0, 0, 0, 50, 1, 0}, -50, 300)
        network += populate({0, 1, 1, 1, 1, 50, -100, 1}, 50, 100)
        network += populate({0, 0, 0, 0, 1, 100, 10, 1}, 500, 100)
        network += populate({0, 0, 0, 0, 1, 50, 10, 0}, -500, 300)

        ' Call Program.RunFitProcess(network, network.First.status.Length, "./test_demo.Xml", Nothing, 5000)
        Call network.SampleSetCreator().GetXml.SaveTo("./DEMO_dataset.Xml")
    End Sub

    Private Iterator Function populate([in] As Double(), out As Double, n As Integer) As IEnumerable(Of Sample)
        For i As Integer = 0 To n
            Yield New Sample With {.status = [in], .target = {out}}
        Next
    End Function
End Module
