Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Serialization.JSON

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
            __digits = New IntRange(digitMin, digitMax + 1)  ' 假若max是1e10的话，则最高的位数是10，这时候由于计算公式的原因最多只能够到9所以在这里需要手动添加一来避免这个问题
        End Sub

        Sub New(from As Double, [to] As Double)
            Call Me.New(
                CInt(If(from = 0R, 0, Math.Log10(from))), ' 避免出现log(0)的情况
                CInt(If([to] = 0R, 0, Math.Log10([to]))))
        End Sub

        Public Overrides Function ToString() As String
            Return __digits.GetJson
        End Function

        Public Function NextNumber() As Double
            Dim d As Integer = __rnd.NextDouble * __digits.Length + __digits.Min      ' generates the digits
            Dim digits As Double = 10 ^ d
            Dim r As Double = __rnd.NextDouble
            Return r * digits
        End Function

        ''' <summary>
        ''' 这个方法可能只适用于很小的数，例如1e-100到1e-10这样子的
        ''' </summary>
        ''' <param name="range"></param>
        ''' <returns></returns>
        Public Function NextDouble(range As DoubleRange) As Double
            Return range.Min + range.Length * NextNumber()
        End Function
    End Class
End Namespace