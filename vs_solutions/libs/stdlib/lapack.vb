Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis

Public Class lapack

    Public Function t_test(x As IEnumerable(Of Double), y As IEnumerable(Of Double), Optional alternative As Hypothesis = Hypothesis.TwoSided) As TwoSampleResult
        Return t.Test(x, y, alternative)
    End Function
End Class
