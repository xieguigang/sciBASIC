Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class TrainingSet

    Public X As Vector
    Public Y As Double
    Public targetID As String

    Sub New(sample As Sample)
        Me.X = sample.status.vector
        Me.Y = sample.target(Scan0)
        Me.targetID = sample.ID
    End Sub

    Friend Sub New()
    End Sub

    Public Overrides Function ToString() As String
        Return targetID
    End Function

End Class