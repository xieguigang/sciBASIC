Namespace ComponentModel.Evaluation

    ''' <summary>
    ''' The regression classifier result.
    ''' </summary>
    Public Class RegressionClassify

        Public Property sampleID As String
        Public Property actual As Double
        Public Property predicts As Double

        Public ReadOnly Property errors As Double
            Get
                Return Math.Abs(predicts - actual)
            End Get
        End Property

    End Class
End Namespace