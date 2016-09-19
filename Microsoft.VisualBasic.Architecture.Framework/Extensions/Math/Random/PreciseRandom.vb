Imports Microsoft.VisualBasic.ComponentModel.Ranges

Namespace Mathematical

    ''' <summary>
    ''' 主要针对的是非常小的小数
    ''' </summary>
    Public Class PreciseRandom

        ReadOnly __rnd As New Random(Now.Millisecond)
        ReadOnly __digits As IntRange

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="digitMin">``10^?``</param>
        ''' <param name="digitMax">``10^?``</param>
        Sub New(digitMin As Integer, digitMax As Integer)
            __digits = New IntRange(digitMin, digitMax)
        End Sub

        Public Function NextNumber() As Double
            Dim d As Integer = __rnd.NextDouble * __digits.Length + __digits.Min      ' generates the digits
            Dim digits As Double = 10 ^ d
            Dim r As Double = __rnd.NextDouble
            Return r * digits
        End Function

        Public Function NextDouble(range As DoubleRange) As Double
            Return range.Min + range.Length * NextNumber()
        End Function
    End Class
End Namespace