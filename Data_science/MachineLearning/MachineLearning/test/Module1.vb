#Region "Microsoft.VisualBasic::764bb4b55498d061560b2d276415672d, Data_science\MachineLearning\MachineLearning\test\Module1.vb"

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
    '     Sub: functionParserTest, Main
    ' 
    ' /********************************************************************************/

#End Region

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
