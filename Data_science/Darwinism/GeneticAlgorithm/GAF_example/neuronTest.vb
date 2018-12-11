Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork

Module neuronTest

    Sub Main()

        Dim test As New TrainingUtils(6, {9, 8, 5}, 3)

        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({0, 0, 0, 1, 1, 1}, {1, 0, 0})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})
        Call test.Add({1, 0, 0, 1, 1, 1}, {0, 0, 1})

        Call test.Train()

        Dim classes1 = test.NeuronNetwork.Compute(0, 0, 0, 1, 1, 1)
        Dim classes2 = test.NeuronNetwork.Compute(1, 0, 0, 1, 1, 1)

        Call StoreProcedure.NeuralNetwork.Snapshot(test.NeuronNetwork).GetXml.SaveTo("./network.xml")


        Pause()
    End Sub
End Module
