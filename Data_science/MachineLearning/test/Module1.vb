Imports Microsoft.VisualBasic.MachineLearning

Module Module1

    Sub Main()
        Call functionParserTest()
    End Sub

    Sub functionParserTest()

        Dim func As New NeuralNetwork.Activations.BipolarSigmoid(alpha:=3)
        Dim str = func.ToString
        Dim model = NeuralNetwork.StoreProcedure.ActiveFunction.Parse(str)
        Dim func2 As NeuralNetwork.Activations.BipolarSigmoid = model

        Pause()
    End Sub

End Module
