Imports std = System.Math

Namespace NDtw.Preprocessing

    ''' <summary>
    ''' f(x) = (x - mean) / std dev
    ''' </summary>
    Public Class StandardizationPreprocessor : Implements IPreprocessor

        Public Function Preprocess(data As Double()) As Double() Implements IPreprocessor.Preprocess
            'http://stats.stackexchange.com/questions/1944/what-is-the-name-of-this-normalization
            'http://stats.stackexchange.com/questions/13412/what-are-the-primary-differences-between-z-scores-and-t-scores-and-are-they-bot
            'http://mathworld.wolfram.com/StandardDeviation.html

            ' x = (x - mean) / std dev
            Dim mean = data.Average()
            Dim N As Integer = (data.Length - 1)
            Dim stdDev = std.Sqrt(SIMD.Subtract.f64_op_subtract_f64_scalar(data, mean).Sum(Function(x) x ^ 2) / N)

            Return data.[Select](Function(x) (x - mean) / stdDev).ToArray()
        End Function

        Public Overrides Function ToString() As String Implements IPreprocessor.ToString
            Return "Standardization"
        End Function
    End Class
End Namespace
