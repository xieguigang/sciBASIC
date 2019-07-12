Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations

Module activeTest

    Sub Main()
        Call runTest(-100, 100).ToArray.SaveTo("./ANN_actives.csv")
    End Sub

    Private Iterator Function runTest(min#, max#) As IEnumerable(Of DataSet)
        Dim BipolarSigmoid As New BipolarSigmoid
        Dim HyperbolicTangent As New HyperbolicTangent
        Dim ReLU As New ReLU
        Dim Sigmoid As New Sigmoid
        Dim SigmoidFunction As New SigmoidFunction
        Dim Threshold As New Threshold

        For x As Double = min To max Step 0.1
            Yield New DataSet With {
                .ID = x.ToHexString,
                .Properties = New Dictionary(Of String, Double) From {
                    {"input", x},
                    {"BipolarSigmoid", BipolarSigmoid(x)},
                    {"BipolarSigmoid/dt", BipolarSigmoid.Derivative(x)},
                    {"HyperbolicTangent", HyperbolicTangent(x)},
                    {"HyperbolicTangent/dt", HyperbolicTangent.Derivative(x)},
                    {"ReLU", ReLU(x)},
                    {"ReLU/dt", ReLU.Derivative(x)},
                    {"Sigmoid", Sigmoid(x)},
                    {"Sigmoid/dt", Sigmoid.Derivative(x)},
                    {"SigmoidFunction", SigmoidFunction(x)},
                    {"SigmoidFunction/dt", SigmoidFunction.Derivative(x)},
                    {"Threshold", Threshold(x)},
                    {"Threshold/dt", Threshold.Derivative(x)}
                }
            }
        Next
    End Function
End Module
