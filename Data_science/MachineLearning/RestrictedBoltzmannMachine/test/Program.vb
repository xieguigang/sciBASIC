Imports ClassLibrary1.math
Imports ClassLibrary1.nn.rbm.factory
Imports ClassLibrary1.nn.rbm.learn
Imports ClassLibrary1.utils
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Module Program
    Sub Main(args As String())
        Dim rbm = New RandomRBMFactory().build(6, 3)
        Dim contrastiveDivergence = New ContrastiveDivergence(New LearningParameters().setEpochs(25000))
        Dim training As Double()() = {
            New Double() {1.0, 1.0, 1.0, 0.0, 0.0, 0.0},
            New Double() {1.0, 0.0, 1.0, 0.0, 0.0, 0.0},
            New Double() {1.0, 1.0, 1.0, 0.0, 0.0, 0.0},
            New Double() {0.0, 0.0, 1.0, 1.0, 1.0, 0.0},
            New Double() {0.0, 0.0, 1.0, 1.0, 0.0, 0.0},
            New Double() {0.0, 0.0, 1.0, 1.0, 1.0, 0.0}
        }
        Dim buildBetterSampleTrainingData As New DenseMatrix(New NumericMatrix(training))

        contrastiveDivergence.learn(rbm, buildBetterSampleTrainingData)

        Dim testData = DenseMatrix.make(New Double()() {New Double() {0, 0, 0, 1, 1, 0}, New Double() {0, 0, 1, 1, 0, 0}})
        Dim hidden = contrastiveDivergence.runVisible(rbm, testData)
        Dim visual = contrastiveDivergence.runHidden(rbm, hidden)

        Call PrettyPrint.ToString(hidden.toArray)
        Call PrettyPrint.ToString(visual.toArray)

        Pause()
    End Sub
End Module
