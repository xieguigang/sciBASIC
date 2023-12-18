Namespace NDtw.Preprocessing

    Public Class NormalizationPreprocessor : Inherits IPreprocessor

        Private ReadOnly _minBoundary As Double
        Private ReadOnly _maxBoundary As Double

        ''' <summary>
        ''' Initialize to use normalization to range [0, 1]
        ''' </summary>
        Public Sub New()
            Me.New(0, 1)
        End Sub

        ''' <summary>
        ''' Initialize to use normalization to range [minBoundary, maxBoundary]
        ''' </summary>
        Public Sub New(minBoundary As Double, maxBoundary As Double)
            _minBoundary = minBoundary
            _maxBoundary = maxBoundary
        End Sub

        Public Overrides Function Preprocess(data As Double()) As Double()
            ' x = ((x - min_x) / (max_x - min_x)) * (maxBoundary - minBoundary) + minBoundary

            Dim min = data.Min()
            Dim max = data.Max()
            Dim constFactor = (_maxBoundary - _minBoundary) / (max - min)

            Return data.[Select](Function(x) (x - min) * constFactor + _minBoundary).ToArray()
        End Function

        Public Overrides Function ToString() As String
            Return "Normalization"
        End Function
    End Class
End Namespace
