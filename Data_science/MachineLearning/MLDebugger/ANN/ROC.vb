Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Module ROC

    <Extension>
    Public Function AUC(training As TrainingUtils) As Double()
        Dim network = training.NeuronNetwork
        Dim result = training.TrainingSet _
            .Select(Function(sample)
                        Dim predicts = network.Compute(sample.status.vector)
                        Dim actuals = sample.target

                        Return New Validate With {
                            .actuals = actuals,
                            .predicts = predicts
                        }
                    End Function) _
            .ToArray
        Dim attributes = result(Scan0).actuals
        Dim thresholdSeq As New Sequence With {.n = 100, .range = {0, 1}}
        Dim evalAUC = Function(null As Double, i As Integer) As Double
                          Dim validations = Validation.ROC(Of Validate)(
                              entity:=result,
                              getValidate:=Function(x, threshold) x.actuals(i) >= threshold,
                              getPredict:=Function(x, threshold) x.predicts(i) >= threshold,
                              threshold:=thresholdSeq
                          ).ToArray
                          Dim AUCValue = Validation.AUC(validations)

                          Return AUCValue
                      End Function
        Dim validateAUCs = attributes _
            .Select(evalAUC) _
            .ToArray

        Return validateAUCs
    End Function
End Module
